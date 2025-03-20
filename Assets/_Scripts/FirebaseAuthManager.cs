using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;

    public InputField emailInputField;
    public InputField passwordInputField;
    public Text statusText;
    public Button registerButton;
    public Button loginButton;


    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            statusText.text = "Firebase initialized!";
        });

        registerButton.onClick.AddListener(RegisterUser);
        loginButton.onClick.AddListener(LoginUser);
    }

    void RegisterUser()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Email and password cannot be empty!";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                statusText.text = "Registration canceled.";
                return;
            }
            if (task.IsFaulted)
            {
                statusText.text = "Registration failed: " + task.Exception.GetBaseException().Message;
                return;
            }

            FirebaseUser newUser = task.Result.User;
            statusText.text = "Registration successful! User: " + newUser.Email;
        });
    }

    void LoginUser()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Email and password cannot be empty!";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                statusText.text = "Login canceled.";
                return;
            }
            if (task.IsFaulted)
            {
                statusText.text = "Login failed: " + task.Exception.GetBaseException().Message;
                return;
            }

            FirebaseUser user = task.Result.User;
            statusText.text = "Login successful! Welcome, " + user.Email;
        });
    }
}