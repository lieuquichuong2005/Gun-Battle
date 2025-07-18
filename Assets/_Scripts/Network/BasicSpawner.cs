using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public static BasicSpawner instance;

    [Header("Network Settings")]
    NetworkRunner runner;
    public NetworkPrefabRef playerPrefab;
    public Transform[] spawnPoints;

    [Header("Game Settings")]
    public int maxPlayers = 8;
    public string roomName = "TestRoom";

    Dictionary<PlayerRef, NetworkObject> playerObjects = new Dictionary<PlayerRef, NetworkObject>();

    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize spawn points if not set
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            CreateDefaultSpawnPoints();
        }
    }

    void CreateDefaultSpawnPoints()
    {
        spawnPoints = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.position = new Vector3(
                UnityEngine.Random.Range(-10, 10),
                1,
                UnityEngine.Random.Range(-10, 10)
            );
            spawnPoints[i] = spawnPoint.transform;
        }
    }

    public async void StartMode(GameMode mode)
    {
        if (runner != null)
        {
            Debug.LogWarning("Runner already exists!");
            return;
        }
        Debug.Log($"Starting game in mode: {mode} with room name: {roomName} and max players: {maxPlayers}");
        //mode = HomeManager.instance.gameMode; // Use HomeManager's game mode if not specified

        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        SceneRef sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        StartGameArgs startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            Scene = sceneRef,
            PlayerCount = maxPlayers,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        };

        Debug.Log($"Attempting to start game with room: {roomName}, mode: {mode}, max players: {maxPlayers}");

        StartGameResult result = await runner.StartGame(startGameArgs);

        if (!result.Ok)
        {
            Debug.LogError($"Failed to start game: {result.ShutdownReason}");
            if (runner != null)
            {
                Destroy(runner);
                runner = null;
            }
            return;
        }

        Debug.Log($"Game started successfully as {mode}!");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} joined the game");

        if (runner.IsServer)
        {
            // Đợi một chút để đảm bảo runner đã sẵn sàng
            /*if (runner.LagCompensation == null)
            {
                Debug.LogWarning("LagCompensation not ready yet, waiting...");
                StartCoroutine(WaitForLagCompensation(runner, player));
                return;
            }*/

            SpawnPlayer(runner, player);
        }
    }

    IEnumerator WaitForLagCompensation(NetworkRunner runner, PlayerRef player)
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (runner.LagCompensation == null && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (runner.LagCompensation != null)
        {
            SpawnPlayer(runner, player);
        }
        else
        {
            Debug.LogError("LagCompensation failed to initialize!");
        }
    }

    void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        Vector3 spawnPosition = GetSpawnPosition(player);
        NetworkObject playerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

        if (playerObject != null)
        {
            playerObjects.Add(player, playerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

        if (playerObjects.TryGetValue(player, out NetworkObject playerObject))
        {
            if (playerObject != null)
            {
                runner.Despawn(playerObject);
            }
            playerObjects.Remove(player);
        }
    }

    Vector3 GetSpawnPosition(PlayerRef player)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = player.PlayerId % spawnPoints.Length;
            return spawnPoints[spawnIndex].position;
        }

        // Fallback to random position
        return new Vector3(
            UnityEngine.Random.Range(-20, 20),
            0,
            UnityEngine.Random.Range(-20, 20)
        );
    }

    public void ModeInput(int mode)
    {
        if (runner != null)
        {
            Debug.LogWarning("Game is already running!");
            return;
        }

        if(mode == 0)
        {
            StartMode(GameMode.Host);
        }
        else if(mode == 1)
        {
            StartMode(GameMode.Client);
        }

        //StartMode(GameMode.AutoHostOrClient); 
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        // Movement input
        Vector3 moveDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDir += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) moveDir += Vector3.back;
        if (Input.GetKey(KeyCode.A)) moveDir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveDir += Vector3.right;

        data.moveDirection = moveDir.normalized;

        // Jump input
        data.isJumping = Input.GetKey(KeyCode.Space);

        // Shooting input
        data.isShooting = Input.GetMouseButton(0);

        // Reload input
        data.isReloading = Input.GetKeyDown(KeyCode.R);

        // Weapon switching với số
        data.weaponSwitchIndex = -1;
        if (Input.GetKeyDown(KeyCode.Alpha1)) data.weaponSwitchIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) data.weaponSwitchIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) data.weaponSwitchIndex = 2;

        // Chuyển vũ khí tuần tự bằng phím T
        data.switchToNextWeapon = Input.GetKeyDown(KeyCode.T);

        // Mouse input for camera
        data.mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Cursor control
        data.isHidingCursor = Input.GetKeyDown(KeyCode.Escape);

        AnimationState anim = AnimationState.Idle;

        // Nếu bắn
        if (Input.GetMouseButton(0))
            anim = AnimationState.Shoot;

        // Nếu đứng yên 8s (xử lý ở phía PlayerController)
        data.animationState = anim;


        input.Set(data);
    }

    public void DisconnectPlayer()
    {
        if (runner != null)
        {
            runner.Shutdown();
        }
    }

    void OnDestroy()
    {
        if (runner != null)
        {
            runner.Shutdown();
        }
    }

    // Network Runner Callbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"Connect failed: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        request.Accept();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug.Log("Custom authentication response received");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected from server: {reason}");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("Host migration occurred");
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        // Handle missing input
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // Handle object entering area of interest
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // Handle object exiting area of interest
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        // Handle reliable data progress
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        // Handle reliable data received
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene load completed");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("Scene load started");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"Session list updated: {sessionList.Count} sessions");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Runner shutdown: {shutdownReason}");

        // Clean up
        playerObjects.Clear();

        if (this.runner == runner)
        {
            this.runner = null;
        }
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        // Handle user simulation messages
    }
}