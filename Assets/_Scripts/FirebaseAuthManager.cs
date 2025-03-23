using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;
using TMPro;

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    private FirebaseAuth auth;

    [Tooltip("Tham chiếu GameObject")]
    [Header("GameObject")]
    [Space(10)]
    public GameObject confirmForm;
    public GameObject notifyPanel;
    [Space(10)]
    public Button accountButton;
    public Button switchFormButton;
    public Button seePasswordButton;

    [Space(10)]
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField ConfirmpasswordInputField;
    [Space(10)]
    public TMP_Text statusText;
    public TMP_Text noteText;
    public TMP_Text stateText;

    [Tooltip("Khai báo biến")]
    [Header("Varialbles")]
    public bool isBeingLogIn = true;
    public bool isShowPassword = false;



    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            StartCoroutine(NotifyStatus("Firebase initialized!"));
        });

        accountButton.onClick.AddListener(OnAccountButtonClicked);
        switchFormButton.onClick.AddListener(OnSwitchFormClicked);

    }

    void OnSwitchFormClicked()
    {
        if (isBeingLogIn)
        {
            confirmForm.SetActive(true);
            noteText.text = "Already have an account";
            stateText.text = "REGISTER";
            switchFormButton.GetComponentInChildren<TMP_Text>().text = "Log In Now";
            accountButton.GetComponentInChildren<TMP_Text>().text = "Register";
        }
        else
        {
            confirmForm.SetActive(false);
            noteText.text = "Do not have an account";
            stateText.text = "LOG IN";
            switchFormButton.GetComponentInChildren<TMP_Text>().text = "Register Now";
            accountButton.GetComponentInChildren<TMP_Text>().text = "Log In";
        }    
        isBeingLogIn = !isBeingLogIn;
    }
        
    void OnAccountButtonClicked()
    {
        if(isBeingLogIn)
        {
            LogInAccount();
        }
        else
        {
            RegisterAccount();
        } 
            
    }
        
    void RegisterAccount()
    {
        string email = usernameInputField.text;
        string password = passwordInputField.text;
        string confirmPassword = ConfirmpasswordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            StartCoroutine(NotifyStatus("Email and password cannot be empty!"));
            return;
        }
        if(passwordInputField.text != ConfirmpasswordInputField.text)
        {
            StartCoroutine(NotifyStatus("Password and Confirm Password do not match"));
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                StartCoroutine(NotifyStatus("Registration canceled."));
                return;
            }
            if (task.IsFaulted)
            {
                StartCoroutine(NotifyStatus("Registration failed." /* task.Exception.GetBaseException().Message*/));
                return;
            }

            FirebaseUser newUser = task.Result.User;
                StartCoroutine(NotifyStatus("Registration successful! User: " + newUser.Email.Split('@')[0]));
        });
    }

    void LogInAccount()
    {
        string email = usernameInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            StartCoroutine(NotifyStatus("Email and password cannot be empty!"));
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                StartCoroutine(NotifyStatus(statusText.text = "Login canceled."));
                return;
            }
            if (task.IsFaulted)
            {
                StartCoroutine(NotifyStatus(statusText.text = "Login failed." /*task.Exception.GetBaseException().Message*/));
                return;
            }

            FirebaseUser user = task.Result.User;
            StartCoroutine(NotifyStatus("Login successful! Welcome, " + user.Email.Split('@')[0]));
        });
    }
    IEnumerator NotifyStatus(string textToNotify)
    {
        notifyPanel.SetActive(true);
        statusText.text = textToNotify;
        yield return new WaitForSeconds(3);
        notifyPanel.SetActive(false);

    }
    void OnShowPasswordButtonClicked()
    {
        if(isShowPassword)
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            ConfirmpasswordInputField.contentType = TMP_InputField.ContentType.Password;
        }
        else
        {
            passwordInputField.contentType = TMP_InputField.ContentType.Standard;
            ConfirmpasswordInputField.contentType = TMP_InputField.ContentType.Standard;
        }
        isShowPassword = !isShowPassword;
    }
        
}