using Fusion;
using UnityEngine;

public class NetworkedBullet : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public float lifetime = 5f;
    public LayerMask hitLayers = -1;

    // Network Properties
    [Networked] public Vector3 Direction { get; set; }
    [Networked] public float Speed { get; set; }
    [Networked] public int Damage { get; set; }
    [Networked] public TickTimer LifetimeTimer { get; set; }
    [Networked] public PlayerRef Owner { get; set; }

    private Rigidbody rb;
    private bool hasHit = false;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;

        LifetimeTimer = TickTimer.CreateFromSeconds(Runner, lifetime);

        //if (Direction != Vector3.zero)
        //{
        //    rb.linearVelocity = Direction * Speed;
        //}
        if (Direction != Vector3.zero)
            transform.forward = Direction;
    }

    // ✅ FIX: Thêm parameter cho Owner
    public void Initialize(Vector3 direction, WeaponData weaponData, PlayerRef owner)
    {
        Direction = direction.normalized;
        Speed = weaponData.bulletSpeed > 0 ? weaponData.bulletSpeed : 1;
        Damage = weaponData.damage;
        Owner = owner; // ✅ Set owner từ parameter
    }

    public override void FixedUpdateNetwork()
    {
        // ✅ CHỈ SERVER XỬ LÝ PHYSICS VÀ HIT DETECTION
        if (!Object.HasStateAuthority) return;

        // Check if bullet should be destroyed
        if (LifetimeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        // Update bullet position
        if (!hasHit)
        {
            Vector3 movement = Direction * Speed ;

            // Kiểm tra đầy đủ trước khi sử dụng LagCompensation
            if (CanUseLagCompensation())
            {
                // Perform raycast to check for hits
                if (Runner.LagCompensation.Raycast(
                    transform.position,
                    Direction,
                    movement.magnitude,
                    Owner,
                    out var hit,
                    hitLayers))
                {
                    HandleHit(hit);
                }
                else
                {
                    rb.linearVelocity += movement;
                }
            }
            else
            {
                // Fallback: sử dụng Physics.Raycast thông thường
                if (Physics.Raycast(transform.position, Direction, out RaycastHit hit, movement.magnitude, hitLayers))
                {
                    HandleHitFallback(hit);
                }
                else
                {
                    transform.position += movement;
                }
            }
        }
    }

    bool CanUseLagCompensation()
    {
        return Runner != null &&
               Runner.LagCompensation != null &&
               Runner.IsServer &&
               Owner.IsRealPlayer;
    }

    void HandleHit(LagCompensatedHit hit)
    {
        if (hasHit) return;
        hasHit = true;

        // Move bullet to hit position
        transform.position = hit.Point;

        // Check if we hit a player
        if (hit.GameObject.TryGetComponent<NetworkedPlayerStats>(out var playerStats))

        {

            // ✅ FIX: Kiểm tra đúng cách để tránh tự bắn mình

            var hitPlayerNetworkObject = hit.GameObject.GetComponent<NetworkObject>();

            if (hitPlayerNetworkObject != null)

            {

                PlayerRef hitPlayer = hitPlayerNetworkObject.InputAuthority;

                Debug.Log($"Hit player: {hitPlayer}, Bullet owner: {Owner}");



                // Chỉ gây damage nếu không phải chính mình

                if (hitPlayer != Owner)

                {

                    Debug.Log($"Dealing {Damage} damage to player {hitPlayer}");

                    playerStats.RPC_TakeDamage(Damage, Owner);

                }

                else

                {

                    Debug.Log("Bullet hit own player, no damage dealt");

                }

            }
        }

        // Spawn hit effects
        RPC_SpawnHitEffects(hit.Point, hit.Normal);

        // Destroy bullet
        Runner.Despawn(Object);
    }

    void HandleHitFallback(RaycastHit hit)
    {
        if (hasHit) return;
        hasHit = true;

        // Move bullet to hit position
        transform.position = hit.point;

        // Check if we hit a player (fallback method)
        if (hit.collider.TryGetComponent<NetworkedPlayerStats>(out var playerStats))
        {
            // Don't damage ourselves
            var networkObject = hit.collider.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.InputAuthority != Owner)
            {
                playerStats.RPC_TakeDamage(Damage, Owner);
            }
        }

        // Spawn hit effects
        RPC_SpawnHitEffects(hit.point, hit.normal);

        // Destroy bullet
        if (Runner != null)
        {
            Runner.Despawn(Object);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]  // ✅ FIX: Chỉ server gọi RPC
    void RPC_SpawnHitEffects(Vector3 hitPoint, Vector3 hitNormal)
    {
        // Spawn particle effects, bullet holes, etc.
        GameObject impact = new GameObject("BulletImpact");
        impact.transform.position = hitPoint;
        impact.transform.rotation = Quaternion.LookRotation(hitNormal);

        // Destroy after some time
        Destroy(impact, 2f);
    }

    void OnTriggerEnter(Collider other)
    {
        // ✅ CHỈ SERVER XỬ LÝ COLLISION
        if (!Object.HasStateAuthority) return;

        // Backup collision detection for non-networked objects
        if (!hasHit && other.gameObject.layer != gameObject.layer)
        {
            hasHit = true;
            Debug.Log($"Bullet trigger hit: {other.name}");
            // Check for player stats component
            if (other.TryGetComponent<NetworkedPlayerStats>(out var playerStats))
            {
                var networkObject = other.GetComponent<NetworkObject>();
                if (networkObject != null && networkObject.InputAuthority != Owner)
                {
                    Debug.Log($"Trigger damage to player: {networkObject.InputAuthority}");
                    playerStats.RPC_TakeDamage(Damage, Owner);
                }
            }
            // Spawn hit effects
            RPC_SpawnHitEffects(transform.position, -Direction);
            // Destroy bullet
            if (Object != null && Runner != null)
            {
                Runner.Despawn(Object);
            }
        }
    }
}