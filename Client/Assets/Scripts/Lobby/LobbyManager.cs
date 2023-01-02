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

    //Variables

    //Paneles UI
    public GameObject lobbyPanel;
    public GameObject roomPanel;

    //Inputs
    //Input para crear una sala
    public TMP_InputField roomInputField;
    
    //Text Labels
    public TMP_Text errorText;
    public TMP_Text roomName;

    //Listas
     public TestHome voiceChat;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public List<PlayerItem> playerItemsList = new List<PlayerItem>();

    //Prefabs

    public RoomItem roomItemPrefab;
    public PlayerItem playerItemPrefab;

    //Otras variables
    public Transform contentObject;
    public Transform playerItemParent;
    public GameObject playButton;

    private void Start() 
    {
        //LoadBalancingClient loadBalancingClient = new LoadBalancingClient(null, TurnbasedAppId, "1.0"); // the master server address is not used when connecting via nameserver
        PhotonNetwork.JoinLobby();
    }
    

    
    private void Update() {
        {
            if(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 1)
            {
                playButton.SetActive(true);
            }else{
                playButton.SetActive(false);
            }
        }
    }

    //Botones

    //Método del botón que crea una sala.
    public void OnClickCreate()
    {
        if(roomInputField.text.Length >=1)
        {
            PhotonNetwork.CreateRoom(roomInputField.text,new RoomOptions(){ MaxPlayers = 4, BroadcastPropsChangeToAll = true, PublishUserId = true});
        }
    }

    //Método llamado por el botón RoomItem para unirse a una sala
    public void OnClickJoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    //Método del botón para abandonar la sala.
    public void OnClickLeaveRoom()
    {
     PhotonNetwork.LeaveRoom();
     }

    //Método del botón de selección de personaje para cargar el mapa
      public void OnClickPlayButton()
    {

        PhotonNetwork.LoadLevel("Mapa1");
    }



    //Funciones 

     //Función para añadir jugadores a la lista de jugadores

    public void AddPlayer(Player newPlayer)
    {
        //Instanciamos el nuevo jugador y lo añadimos a la lista de usuarios
        PlayerItem playerItem = Instantiate(playerItemPrefab, playerItemParent); 
        playerItem.SetPlayerInfo(newPlayer);
       
        //??????
           if (newPlayer == PhotonNetwork.LocalPlayer)
           {
            playerItem.ApplyLocalChanges();
           }

           playerItemsList.Add(playerItem);


    }

    //Función que borra los jugadores de la lista 
   
    public void DeletePlayer(Player oldPlayer)
    {
        //Recorre la lista y comprueba los ID's de los usuarios para borrarlo
         foreach(PlayerItem item in playerItemsList)
        {
           if (item.GetPlayerInfo().UserId == oldPlayer.UserId){
           
            Destroy(item.gameObject);
            playerItemsList.Remove(item);
           }

        }
    }

    //Método cuando un usuario entra por primera vez a una sala
    void UpdatePlayerList()
    {
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



    //Callbacks Photon


    /*
    **********

    IMPORTANTE
    COMO FUNCIONA CONEXIÓN PHOTON
    MASTER SERVER (ConnectUsingSettings) -> GAME SERVER (JoinLobby) -> ROOM (JoinOrCreateRoom)

    **********
    */

    //CallBack de Photon que se llama una vez de conecta al servidor principal
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    //CallBack de Photon que se llama cada x tiempo con nueva info de las listas.
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //Recorremos la nueva lista con información
        foreach (RoomInfo info in roomList)
        {
            //Si una sala ha sido destruida la eliminamos de la lista y de la interfaz
            if (info.RemovedFromList)
            {
                int index = roomItemsList.FindIndex( x => x.RoomInfo.Name == info.Name);
                if (index != -1)
                {
                    Destroy(roomItemsList[index].gameObject);
                    roomItemsList.RemoveAt(index);
                }
            }

            else
            {
                //Probar si este if hace falta.
                if (roomItemsList.FindIndex( x => x.RoomInfo.Name == info.Name) == -1)
                {
            
              //Instanciamos el item en la interfaz.
              RoomItem newRoom = Instantiate(roomItemPrefab,contentObject);
              if (newRoom != null)
              {
                newRoom.SetRoomInfo(info);
                roomItemsList.Add(newRoom);
              }
                }

            }
        }
    }

    //CallBack de Photon llamado cuando el usuario se une a una sala.
    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text="Room Name: " + PhotonNetwork.CurrentRoom.Name;
        PhotonNetwork.SetPlayerCustomProperties(null);

        //Método para actualizar la lista de jugadores

        UpdatePlayerList();

        //Destruimos la lista de rooms ya que no está actualizada y se actualizará cuando nos unamos de nuevo al Lobby.

         foreach(RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();    
    } 

     //Callback para cuando falla la conexión a una sala
    public override void OnJoinRoomFailed(short returnCode, string message){

        //Comprobar el error de retorno para comprobar distintos fallos
        Debug.Log(message);
        errorText.text = "Ya te encuentras en esa sala";

        //TO DO Hacer que sea visible o invisible
        //TO DO Cambiar error dependiendo del codigo

    }


    //CallBack de Photon que se llama al salir de una sala
    public override void OnLeftRoom()
    {

        //Eliminamos la lista de jugadores al salir de la sala

        foreach(PlayerItem item in playerItemsList)
        {
            Destroy(item.gameObject);
        }
        playerItemsList.Clear();

        //Activamos los paneles de la interfaz del Lobby.
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);



    }


        //CallBack de Photon cuando un jugador entra en la sala
    public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AddPlayer(newPlayer);
            //UpdatePlayerList();
        }

    //CallBack de Photon cuando un jugador deja la sala.
    public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            //UpdatePlayerList();
            DeletePlayer(otherPlayer);
        }
    
}