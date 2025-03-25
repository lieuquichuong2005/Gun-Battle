using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Photon Settings")]
    public string gameVersion = "1.0";
    public GameObject roomListItemPrefab;
    public Transform roomListContent;

    private List<GameObject> roomListItems = new List<GameObject>();
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>(); // Danh sách phòng được lưu tạm

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        cachedRoomList = roomList; // Cập nhật danh sách phòng mới nhất
        UpdateRoomList(roomList);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room: " + PhotonNetwork.CurrentRoom.Name);
        //PhotonNetwork.LoadLevel("BattleScene");
        //SpawnPlayer(); // Tạo nhân vật khi vào phòng
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        //PhotonNetwork.LoadLevel("BattleScene");
        //SpawnPlayer(); // Tạo nhân vật khi vào phòng
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed: " + message);
    }

    public void CreateRoom(string roomName, int maxPlayers, bool isUsePassword, string password)
    {
        if(PhotonNetwork.IsConnected)
        {
            if (string.IsNullOrEmpty(roomName))
            {
                Debug.LogError("Room name is empty");
                return;
            }

            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = (byte)maxPlayers
            };

            if (isUsePassword && !string.IsNullOrEmpty(password))
            {
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "Password" };
                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "Password", password } };
            }

            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
        
    }

    public void JoinRoom(string roomName, string passwordInput = "")
    {
        RoomInfo targetRoom = cachedRoomList.Find(room => room.Name == roomName);

        if (targetRoom != null)
        {
            if (targetRoom.CustomProperties.ContainsKey("Password"))
            {
                string roomPassword = targetRoom.CustomProperties["Password"].ToString();
                if (roomPassword != passwordInput)
                {
                    Debug.LogError("Incorrect password!");
                    return;
                }
            }

            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.LogError("Room not found.");
        }
    }

    void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (GameObject item in roomListItems)
        {
            Destroy(item);
        }
        roomListItems.Clear();

        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) continue;

            GameObject roomListItem = Instantiate(roomListItemPrefab, roomListContent);
            string passwordStatus = room.CustomProperties.ContainsKey("Password") ? "🔒" : "🔓";
            roomListItem.GetComponent<RoomListItem>().Setup(room, this, passwordStatus);
            roomListItems.Add(roomListItem);
        }
    }

    
}