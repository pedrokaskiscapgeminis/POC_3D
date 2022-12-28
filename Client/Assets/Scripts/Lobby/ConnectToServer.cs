using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Realtime;
using System.Linq;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    private LoadBalancingClient lbc;

    public TMP_Text TitleText;

    private void Start ()
    {
        PhotonNetwork.GameVersion = "0.0.1";
    }

    public void OnClickConnect()
    {
        //Obtenemos los campos de los inputs
        string username = usernameInput.text;
        string password = passwordInput.text;

        //Validación de los campos
        if(ValidatePassword(password) && ValidateUsername(username))
        {
            print("Trying to conect to server.");
            TitleText.text = "Validando...";

            //Creamos el diccionario para enviar la petición POST
            Dictionary<string, object> userdata = new Dictionary<string, object>();
            //Debug.Log(usernameInput.text);
            //Debug.Log(passwordInput.text);
            userdata.Add("username",usernameInput.text);
            userdata.Add("password",passwordInput.text);
     
            //Creamos el objeto AuthenticationValues que le pasaremos a Photon
            AuthenticationValues authValues = new AuthenticationValues();
            authValues.AuthType = CustomAuthenticationType.Custom;
            authValues.SetAuthPostData(userdata);
            PhotonNetwork.AuthValues = authValues;

            //?
            PhotonNetwork.AutomaticallySyncScene = true;
            //Realizamos la conexión
            PhotonNetwork.NickName = usernameInput.text;

            if(PhotonNetwork.ConnectUsingSettings())
            {
                TitleText.text = "Conectando...";
            }
            
        }
        else
        {
            Debug.Log("Credenciales invalidas");
        }
    }

    public override void OnConnectedToMaster()
    {
        print("Conected to master" + ", " + PhotonNetwork.LocalPlayer.UserId);
        SceneManager.LoadScene("Lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print("Disconected from server, reason: " + cause.ToString());
    }

    public override void OnCustomAuthenticationFailed(string falloauth = "AuthFailed")
    {
        TitleText.text = "Error de autenticacion, compruebe los campos";
    }

    public bool ValidatePassword(string password)
    {

        bool hasLowercase = false;
        bool hasUppercase = false;
        bool hasNumber = false;
        bool hasSpecial = false;

        // Check minimum length
        if (password.Length < 8)
        {
            return false;
        }

        // Check maximum length
        if (password.Length > 20)
        {
            return false;
        }

        // The pass must contain Check for required characters for security in user passwords
        foreach (char c in password)
        {
            if (char.IsLower(c))
            {
                hasLowercase = true;
            }
            else if (char.IsUpper(c))
            {
                hasUppercase = true;
            }
            else if (char.IsNumber(c))
            {
                hasNumber = true;
            }
            else if (char.IsSymbol(c) || char.IsPunctuation(c))
            {
                hasSpecial = true;
            }
        }
        if (!hasLowercase || !hasUppercase || !hasNumber || !hasSpecial)
        {
            Debug.Log("Not secure password");
            return false;//Not a secure password
        }

        // Check for forbidden characters
        string forbidden = "!@#$%^&*()+";

        foreach (char c in forbidden)
        {
            if (password.Contains(c))
            {
                Debug.Log("Invalid characters");
                return false; //Pass Invalid return false
            }
        }

        return true;// Password is valid when pass everycheck
    }

    public bool ValidateUsername(string username)
    {
        // Check minimum length
        if (username.Length < 3)
        {
            return false;
        }

        // Check maximum length
        if (username.Length > 20)
        {
            return false;
        }

        // Check allowed characters
        string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-.";
        foreach (char c in username)
        {
            if (!allowed.Contains(c))
            {
                return false;
            }
        }

        // Check forbidden characters
        string forbidden = "@#$%^&*()+=";
        foreach (char c in forbidden)
        {
            if (username.Contains(c))
            {
                return false;
            }
        }

        // Check reserved words
        string[] reserved = {"admin", "root", "system"};
        if (reserved.Contains(username.ToLower()))
        {
            return false;
        }

        // Username is valid
        return true;
    }
}
