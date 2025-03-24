using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
    [Header("References")]
    [Space(10)]
    [Tooltip("Setting panel reference")]
    public GameObject settingPanel;
    public  Slider musicSlider;
    public Slider soundSlider;
    public Button fullscreenButton;
    public Dropdown resolutionDropdown;
    public Button applyButton;
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
    public Button joinRoomButton;
    public InputField roomNameInputField;
    public Button backToHomeButton;
    [Space(10)]
    [Tooltip("Game panel reference")]
    public Button shopButton;
    public Button inventoryButton;
    public Button friendsButton;
    public Button settingsButton;
    [Space(10)]
    public AudioSource homeAudioSource;
    public AudioClip buttonClickClip;

    private void Awake()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        rankButton.onClick.AddListener(OnRankButtonClicked);
        allModeButton.onClick.AddListener(OnAllModeButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }
    void OnPlayButtonClicked()
    {
        
    }
    void OnRankButtonClicked()
    {
        
    }
    void OnAllModeButtonClicked()
    {
        
    }
    void OnSettingsButtonClicked()
    {
        
    }
}
