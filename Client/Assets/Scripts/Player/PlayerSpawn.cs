using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private Estados estado;
    private TestHome voiceChat;
    public GameObject[] playerPrefabs;
    public Transform[] spawnPoints;
    public GameObject Pausa;
    public GameObject Settings;
    public string mapName;
   
    bool escPul;
    //Variables estáticas
    static bool reload = false;
    static Vector3 spawnPoint;

    GameObject playerToSpawn;

    private void Start() {

        estado = Estados.Juego;
        escPul=false;
        //We check if it's the first time the user entered the room.
        if(reload == false){
        int randomNumber = Random.Range(0, spawnPoints.Length);
        spawnPoint = spawnPoints[randomNumber].position;
        }
        voiceChat=GameObject.Find("VoiceManager").GetComponent<TestHome>();
        
        if(PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"] == null || (int) PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"] == 6)
        {
           
            int value = Random.Range(0,5);
            PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"] = value;    
           
            Debug.Log(PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"] = value);
            
            
        }
       
            playerToSpawn = playerPrefabs[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"]];
            playerToSpawn = (GameObject) PhotonNetwork.Instantiate(playerToSpawn.name, spawnPoint, Quaternion.identity);
           
            playerToSpawn.GetComponent<SC_FPSController>().enabled = true;
            playerToSpawn.transform.Find("PlayerCamera").gameObject.SetActive(true);
            playerToSpawn.transform.Find("PlayerUIPrefab").gameObject.SetActive(true);
            voiceChat.CheckMicroImage();
<<<<<<< Updated upstream
            PhotonNetwork.IsMessageQueueRunning = true;

=======
            //PhotonNetwork.IsMessageQueueRunning = true;
}
>>>>>>> Stashed changes
}

public override void OnConnectedToMaster()
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
}

private void Update() {

    //Estado pausa
    if (estado == Estados.Juego)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !escPul)
            {
                Pausa.SetActive(true);
                //Time.timeScale = 0;
                playerToSpawn.GetComponent<SC_FPSController>().enabled = false;

                Cursor.visible = true;   
                Cursor.lockState = CursorLockMode.None; // Desactiva el bloqueo cursor
                estado = Estados.Pausa;
                escPul=true; //Escape activado
                Debug.Log(estado);
            }
    }      
    
    if (!Input.GetKeyDown(KeyCode.Escape)) escPul=false; // Detecta si no está pulsado

    //Estado juego
    if (estado == Estados.Pausa)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !escPul)
        {
            Settings.SetActive(false);
            Pausa.SetActive(false);
            Time.timeScale = 1;
            playerToSpawn.GetComponent<SC_FPSController>().enabled = true;
            Cursor.visible = false;   
            Cursor.lockState = CursorLockMode.Locked; // Menu de opciones, para que se bloquee la camara 
            estado = Estados.Juego;
            Debug.Log(estado);  
        }
    }
    }

    public enum Estados
    {
        Juego,
        Pausa
    }

//Recibir eventos
public void OnEvent(EventData photonEvent)
{
   if(photonEvent.Code == 1)
   {
    //We maintain the same state between reloads.
    reload = true;
    spawnPoint = playerToSpawn.transform.position;
   
   //We reload the level
   PhotonNetwork.LoadLevel(mapName);
   }
}
}
