using Fusion;
using UnityEngine;

public enum AnimationState
{
    Idle,
    Shoot,
    CheckGun
}

public struct NetworkInputData : INetworkInput
{
    public Vector3 moveDirection;
    public bool isJumping;
    public bool isShooting;
    public bool isReloading;
    public int weaponSwitchIndex;
    public Vector2 mouseInput;
    public bool isHidingCursor;
    public bool switchToNextWeapon;

    public AnimationState animationState;
}