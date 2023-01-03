using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Disconnect : MonoBehaviourPunCallbacks
{
  
    public GameObject Settings;
    public GameObject Pausa;
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

    public void OnClickSettings()
    {   
        Pausa.SetActive(false);
        Settings.SetActive(true);
    }

      public void OnClickReturnLobby()
    {
      PhotonNetwork.LeaveRoom();
      voiceChat.onLeaveButtonClicked();
    }

    public override void OnLeftRoom()
    {
      Debug.Log("Leaving");
    }

    public override void OnConnectedToMaster(){
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
            