using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Realtime;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text buttonText;

    public TMP_Text TitleText;
    public void OnClickConnect()
    {

        print("Connecting to server.");
        TitleText.text = "Conectando...";

        //Validación de los campos


        //Creamos el diccionario para enviar la petición POST
        Dictionary<string, object> userdata = new Dictionary<string, object>();
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
        PhotonNetwork.ConnectUsingSettings();
        
    }
    public override void OnConnectedToMaster()
    {
       print(PhotonNetwork.LocalPlayer.UserId);
      SceneManager.LoadScene("Lobby");
    }
}
