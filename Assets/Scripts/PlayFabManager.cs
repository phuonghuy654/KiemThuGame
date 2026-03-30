using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Newtonsoft.Json;


public class PlayFabManager : MonoBehaviour
{
    [Header("SignIn")]
    public GameObject registerPanel;
    public TextMeshProUGUI messageText;

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField usernameInput;

    [Header("LogIn")]
    public GameObject loginPanel;
    public TextMeshProUGUI loginMessageText;

    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;


    public void RegisterButton()
    {

        if (passwordInput.text.Length < 6)
        {
            messageText.text = "Password too short!";
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text,
            Username = usernameInput.text,
            RequireBothUsernameAndEmail = true,
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }
    void Start()
    {
        //Login();
    }

    void Login()
    {
        var request = new LoginWithCustomIDRequest 
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);
    }

    void OnSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");

    }
    void OnError(PlayFabError error)
    {
        messageText.text = error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "Registration successful!";
    }

    public void LoginButton()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = loginEmailInput.text,
            Password = loginPasswordInput.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess (LoginResult result)
    {
        loginMessageText.text = "Login successful!";
        Debug.Log("Login successful!");
        
    }
    public void signInClick()
    {
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);
    }
    public void backClick()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }


}
