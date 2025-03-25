using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject loadingScene;
    public Slider loadingSlider; 
    public TMP_Text loadingText; 
    public TMP_Text loadingPercent;
    private string baseText = "Loading";
    private int dotCount = 0;
    private float delay = 0.5f;

    public GameObject playerCameraPrefab;
    private void Awake()
    {
        loadingScene.SetActive(false);
        DontDestroyOnLoad(this.gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void LoadNewScene(string sceneName)
    {
        loadingScene.SetActive(true);
        StartCoroutine(UpdateLoadingText());
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            // Cập nhật giá trị slider theo tiến độ
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // 0.9 là mức tối đa của AsyncOperation
            loadingSlider.value = progress; // Cập nhật giá trị slider
            loadingPercent.text = (progress * 100).ToString("F0") + "%"; // Hiển thị tỷ lệ phần trăm

            yield return null; // Chờ đến frame tiếp theo
        }
        loadingScene.SetActive(false);
        
    }
    private IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            loadingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 6;
            yield return new WaitForSeconds(delay);
        }
    }
    void SpawnPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject player = PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-5, 5), 0, Random.Range(-10, 10)), Quaternion.identity);

            GameObject playerCamera = Instantiate(playerCameraPrefab);
            playerCamera.transform.SetParent(player.transform);
            playerCamera.transform.localPosition = Vector3.zero;
            playerCamera.transform.localRotation = Quaternion.identity;
        }
    }
}