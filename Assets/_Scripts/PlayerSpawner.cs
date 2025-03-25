using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPun
{
    public GameObject playerCameraPrefab; 

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        // Sinh ra nhân vật
        GameObject player = PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-5, 5), 1, Random.Range(-10, 10)), Quaternion.identity);

        // Sinh ra camera cho người chơi cục bộ
        GameObject playerCamera = Instantiate(playerCameraPrefab);
        playerCamera.transform.SetParent(player.transform);
        playerCamera.transform.localPosition = Vector3.zero;
        playerCamera.transform.localRotation = Quaternion.identity;
    }
}