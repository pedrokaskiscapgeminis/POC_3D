using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Realtime;

public class Disconnect : MonoBehaviourPunCallbacks
{
  
    //Voice Chat
    private TestHome voiceChat;

    void Start(){
      voiceChat = GameObject.Find("VoiceManager").GetComponent<TestHome>();
    }

    public void OnClickDisconnect()
    {   
        PhotonNetwork.Disconnect();
        voiceChat.OnApplicationQuit();
        Application.Quit();
    }

      public void OnClickReturnLobby()
    {
      //PhotonNetwork.AutomaticallySyncScene = false;
      PhotonNetwork.LeaveRoom();
      voiceChat.onLeaveButtonClicked();
      //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public override void OnLeftRoom()
    {
      Debug.Log("Leaving");
    }

    public override void OnConnectedToMaster(){
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
            