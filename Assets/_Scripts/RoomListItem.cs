using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    public TMP_Text passwordStatusText;

    private NetworkManager networkManager;
    private string roomName;

    public Button joinRoomButton;

    private void Awake()
    {
        joinRoomButton.onClick.AddListener(OnJoinButtonClicked);
    }

    public void Setup(RoomInfo room, NetworkManager manager, string passwordStatus)
    {
        roomName = room.Name;
        roomNameText.text = room.Name;
        playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";
        passwordStatusText.text = passwordStatus;
        networkManager = manager;
    }

    public void OnJoinButtonClicked()
    {
        networkManager.JoinRoom(roomName);
    }
}
