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
    public void OnClickDisconnect()
    {
        PhotonNetwork.Disconnect();
        Application.Quit();
    }
      public void OnClickBackLobby()
    {   
        
    }
   
   
}
            