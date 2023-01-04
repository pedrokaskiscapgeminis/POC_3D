using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks, IOnEventCallback
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

    //Voice Chat
    private TestHome voiceChat;

    //Listas
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
        PhotonNetwork.JoinLobby();

        //find voice chat script
        voiceChat=GameObject.Find("VoiceManager").GetComponent<TestHome>();
    }
    

    
     private void Update() {
        {
            if (PhotonNetwork.CurrentRoom != null )
             
             if (PhotonNetwork.IsMasterClient || (bool) PhotonNetwork.CurrentRoom.CustomProperties["Init"])
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

            //Opciones personalizadas de la sala

            Hashtable customSettings = new Hashtable();

            //Mapa
            customSettings.Add("Map", 1);
            customSettings.Add("Init",false);


            //Creamos opciones de la sala

            RoomOptions opciones = new RoomOptions(){MaxPlayers = 4, BroadcastPropsChangeToAll = true, PublishUserId = true, CustomRoomProperties = customSettings};




            PhotonNetwork.CreateRoom(roomInputField.text, opciones);
            voiceChat.onJoinButtonClicked(roomInputField.text);
        }
    }

    //Método llamado por el botón RoomItem para unirse a una sala
    public void OnClickJoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        voiceChat.onJoinButtonClicked(roomName);
    }

    //Método del botón para abandonar la sala.
    public void OnClickLeaveRoom()
    {
     PhotonNetwork.LeaveRoom();
     voiceChat.onLeaveButtonClicked();
     }

    //Método del botón de selección de personaje para cargar el mapa
      public void OnClickPlayButton()
    {
        

        //Si es el cliente principal envía un evento a los demás para que se sincronicen
        if (PhotonNetwork.IsMasterClient)
        {

            //Cambiamos la sala a empezada

            Hashtable customSettings = PhotonNetwork.CurrentRoom.CustomProperties;
            customSettings["Init"] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customSettings);

            Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties["Init"]);

            //Enviamos el evento
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(2, "", raiseEventOptions, SendOptions.SendReliable);
        }
        else
        {


      
           //Envíamos evento si nos unimos después 
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; // You would have to set the Receivers to All in order to receive this event on the local client as well
            PhotonNetwork.RaiseEvent(1, "", raiseEventOptions, SendOptions.SendReliable);
             
            PhotonNetwork.IsMessageQueueRunning = false;
            PhotonNetwork.LoadLevel("Mapa1");
        }
            







    }

    //Funciones 

     //Función para añadir jugadores a la lista de jugadores que ya estan dentro de la sala

    public void AddPlayer(Player newPlayer)
    {
        //Instanciamos el nuevo jugador y lo añadimos a la lista de usuarios
        PlayerItem playerItem = Instantiate(playerItemPrefab, playerItemParent); 
        playerItem.SetPlayerInfo(newPlayer);
       
        //Establece(solo para el jugador de la sesion y no para los demas en la sala) las flechas visibles para poder seleccionar el avatar en el selectior
           if (newPlayer == PhotonNetwork.LocalPlayer)
           {
            playerItem.ApplyLocalChanges();//Metodo de PlayerItem.cs, unicamente hace un SetActive(true) en las flechas de seleccion
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

    //Método cuando un usuario entra a una sala, le actualiza la lista de jugadores ya en sala
    void UpdatePlayerList()
    {
        foreach (KeyValuePair<int,Player> player in PhotonNetwork.CurrentRoom.Players)
        {
           PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent); 
           newPlayerItem.InicializePlayerInfo(player.Value);

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

        Debug.Log("NULL -----------------------------------------------------");

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
            if (item!=null)
            Destroy(item.gameObject);
        }
        playerItemsList.Clear();

        //Activamos los paneles de la interfaz del Lobby.
        if (roomPanel!=null & lobbyPanel!=null){
            roomPanel.SetActive(false);
            lobbyPanel.SetActive(true);
        }


    }


        //CallBack de Photon cuando un jugador entra en la sala
    public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AddPlayer(newPlayer);
        }

    //CallBack de Photon cuando un jugador deja la sala.
    public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            DeletePlayer(otherPlayer);
        }


public void OnEvent(EventData photonEvent)
{
   if(photonEvent.Code == 2)
   {


            PhotonNetwork.IsMessageQueueRunning = false;
            PhotonNetwork.LoadLevel("Mapa1");
   }
}
    
}