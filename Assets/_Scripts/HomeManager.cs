using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class HomeManager : MonoBehaviour
{
    [Header("References")]
    [Space(10)]
    public NetworkManager networkManager;
    [Space(10)]
    [Tooltip("Setting panel reference")]
    public GameObject canvas;
    public GameObject settingPanel;
    public Slider musicSlider;
    public Slider soundSlider;
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;
    [Space(10)]
    [Tooltip("Account panel reference")]
    public GameObject accountPanel;
    public Image avatarImage;
    public TMP_Text usernameText;
    public Button logoutButton;
    [Space(10)]
    [Tooltip("Main menu buttons")]
    public Button playButton;
    public Button rankButton;
    public Button allModeButton;
    [Space(10)]
    [Tooltip("Currency amount texts")]
    public TMP_Text coinAmountText;
    public TMP_Text goldAmountText;
    public TMP_Text diamondAmountText;
    [Space(10)]
    [Tooltip("Lobby panel reference")]
    public GameObject lobbyPanel;
    public Button createRoomButton;
    public TMP_InputField roomNameCreateInputField;
    public TMP_Dropdown mountOfPeopleDropdown;
    public Toggle isUsePasswordToggle;
    public TMP_InputField roomPasswordCreateInputField;
    public Button joinRoomButton;
    public TMP_InputField roomNameJoinInputField;
    [Space(10)]
    [Tooltip("Game panel reference")]
    public Button shopButton;
    public Button inventoryButton;
    public Button friendsButton;
    public Button settingsButton;
    public Button closeButton;
    [Space(10)]
    public AudioSource musicAudioController;
    public AudioSource sfxAudioController;
    public AudioClip buttonClickClip;

    // Loại bỏ AudioMixer
    // [Header("Audio Settings")]
    // public AudioMixer audioMixer;

    private void Awake()
    {
        canvas.gameObject.SetActive(true);

        playButton.onClick.AddListener(OnPlayButtonClicked);
        rankButton.onClick.AddListener(OnRankButtonClicked);
        allModeButton.onClick.AddListener(OnAllModeButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);

        musicSlider.onValueChanged.AddListener(delegate { OnMusicSliderValueChanged(); });
        soundSlider.onValueChanged.AddListener(delegate { OnSoundSliderValueChanged(); });

        resolutionDropdown.onValueChanged.AddListener(delegate { OnResolutionDropdownValueChanged(); });
        fullscreenToggle.onValueChanged.AddListener(delegate { OnFullscreenToggleValueChanged(); });
        isUsePasswordToggle.onValueChanged.AddListener(delegate { OnIsUsePasswordToggleValueChanged(); });
        roomPasswordCreateInputField.onValueChanged.AddListener(delegate { OnroomPasswordCreateInputFieldValueChanged(); });

        lobbyPanel.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);

        musicAudioController.Play();
    }

    void OnPlayButtonClicked()
    {
        lobbyPanel.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
        roomPasswordCreateInputField.gameObject.SetActive(isUsePasswordToggle.isOn);
        PlayButtonClickSound();
    }

    void OnRankButtonClicked()
    {
        Debug.Log("Rank button clicked");
        PlayButtonClickSound();
    }

    void OnAllModeButtonClicked()
    {
        Debug.Log("All Mode button clicked");
        PlayButtonClickSound();
    }

    void OnSettingsButtonClicked()
    {
        settingPanel.SetActive(true);
        closeButton.gameObject.SetActive(true);
        PlayButtonClickSound();
    }

    void PlayButtonClickSound()
    {
        if (sfxAudioController != null && buttonClickClip != null)
        {
            sfxAudioController.PlayOneShot(buttonClickClip);
        }
    }

    void OnMusicSliderValueChanged()
    {
        if (musicAudioController != null)
        {
            musicAudioController.volume = musicSlider.value;
        }
        SaveSettings();
    }

    void OnSoundSliderValueChanged()
    {
        if (sfxAudioController != null)
        {
            sfxAudioController.volume = soundSlider.value;
        }
        SaveSettings();
    }

    void OnResolutionDropdownValueChanged()
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution selectedResolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
        SaveSettings();
    }

    void OnFullscreenToggleValueChanged()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
        SaveSettings();
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SoundVolume", soundSlider.value);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 1f);
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 0);
        Screen.fullScreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        OnMusicSliderValueChanged();
        OnSoundSliderValueChanged();
        OnResolutionDropdownValueChanged();
        OnFullscreenToggleValueChanged();
    }

    public void OpenAccountPanel()
    {
        accountPanel.SetActive(true);
        closeButton.gameObject.SetActive(true);
        PlayButtonClickSound();
    }

    public void CloseAccountPanel()
    {
        accountPanel.SetActive(false);
        PlayButtonClickSound();
    }

    public void CloseLobbyPanel()
    {
        lobbyPanel.SetActive(false);
        roomPasswordCreateInputField.gameObject.SetActive(false);
        PlayButtonClickSound();
    }

    void OnroomPasswordCreateInputFieldValueChanged()
    {
        roomPasswordCreateInputField.gameObject.SetActive(isUsePasswordToggle.isOn);
    }

    void OnIsUsePasswordToggleValueChanged()
    {
        roomPasswordCreateInputField.gameObject.SetActive(isUsePasswordToggle.isOn);
    }

    public void JoinRoom(string roomName)
    {
        networkManager.JoinRoom(roomName);
    }

    void OnCreateRoomButtonClicked()
    {
        int maxPlayers = int.Parse(mountOfPeopleDropdown.options[mountOfPeopleDropdown.value].text);
        networkManager.CreateRoom(roomNameCreateInputField.text, maxPlayers, isUsePasswordToggle.isOn, roomPasswordCreateInputField.text);
        canvas.SetActive(false);
        GameManager.instance.LoadNewScene("BattleScene");
    }
    void OnJoinRoomButtonClicked()
    {
        networkManager.JoinRoom(roomNameJoinInputField.text, null);
        GameManager.instance.LoadNewScene("BattleScene");
    }    
    void OnCloseButtonClicked()
    {
        PlayButtonClickSound();
        settingPanel.gameObject.SetActive(false);
        lobbyPanel.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }
}