using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomInputField;
    public GameObject lobbyPanel;

    public TMP_Text errorText;
    public GameObject roomPanel;
    public TMP_Text roomName;
    public RoomItem roomItemPrefab;
    public TestHome voiceChat;
    List<RoomItem> roomItemsList = new List<RoomItem>();

    public Transform contentObject;

    public float timeBetweenUpdates = 1.5f;
    float nextT;

    public List<PlayerItem> playerItemsList = new List<PlayerItem>();
    public PlayerItem playerItemPrefab;
    public Transform playerItemParent;

    public GameObject playButton;

    private void Start() 
    {
        PhotonNetwork.JoinLobby();
        
    }
    public void OnClickCreate()
    {
        if(roomInputField.text.Length >=1)
        {
            PhotonNetwork.CreateRoom(roomInputField.text,new RoomOptions(){ MaxPlayers = 4, BroadcastPropsChangeToAll = true});
            voiceChat.onJoinButtonClicked(roomInputField.text);;
        }
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text="Room Name: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }   

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if(Time.time >= nextT){
            UpdateRoomList(roomList);
            nextT = Time.time + timeBetweenUpdates;
        }
    }
    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach(RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach(RoomInfo room in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab,contentObject);
            newRoom.SetRoomName(room.Name);
            roomItemsList.Add(newRoom); 
        }
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        voiceChat.onJoinButtonClicked(roomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message){

        //Comprobar el error de retorno para comprobar distintos fallos
        Debug.Log(message);
        errorText.text = "Ya te encuentras en esa sala";

    }
     public void OnClickLeaveRoom()
    {
     PhotonNetwork.LeaveRoom();
     voiceChat.onLeaveButtonClicked();
     }
    public override void OnLeftRoom()
    {
    roomPanel.SetActive(false);
    lobbyPanel.SetActive(true);
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    void UpdatePlayerList()
    {
        foreach(PlayerItem item in playerItemsList)
        {
            if (item!=null)
            Destroy(item.gameObject);
        }
        playerItemsList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }
        foreach (KeyValuePair<int,Player> player in PhotonNetwork.CurrentRoom.Players)
        {
           PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent); 
           newPlayerItem.SetPlayerInfo(player.Value);

           if (player.Value == PhotonNetwork.LocalPlayer)
           {
            newPlayerItem.ApplyLocalChanges();
           }
           playerItemsList.Add(newPlayerItem);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdatePlayerList();
        }
    public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            UpdatePlayerList();
        }
    private void Update() {
        {
            if (SceneManager.GetActiveScene().name =="Lobby"){
                if(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 1)
                {
                    playButton.SetActive(true);
                }else{
                    playButton.SetActive(false);
                }
            }
        }
    }

    public void OnClickPlayButton()
    {
        PhotonNetwork.LoadLevel("Mapa1");
    }
}