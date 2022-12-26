using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class PlayerSpawn : MonoBehaviour
{
    private Estados estado;
    public GameObject[] playerPrefabs;
    public Transform[] spawnPoints;
    public GameObject Pausa;
    bool escPul;

    GameObject playerToSpawn;

    private void Start() {

        estado = Estados.Juego;
        escPul=false;
        int randomNumber = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomNumber];

        playerToSpawn = playerPrefabs[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"]];
        playerToSpawn = (GameObject) PhotonNetwork.Instantiate(playerToSpawn.name, spawnPoint.position, Quaternion.identity);

        playerToSpawn.GetComponent<SC_FPSController>().enabled = true;
        playerToSpawn.transform.Find("PlayerCamera").gameObject.SetActive(true);
}
private void Update() {

    //Estado Juego
    if (estado == Estados.Juego)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !escPul)
            {
                Pausa.SetActive(true);
                Time.timeScale = 0;
                playerToSpawn.GetComponent<SC_FPSController>().enabled = false;

                Cursor.visible = true;   
                Cursor.lockState = CursorLockMode.None; // Desactiva el bloqueo cursor
                estado = Estados.Pausa;
                escPul=true; //Escape activado
                Debug.Log(estado);
            }
    }      
    
    if (!Input.GetKeyDown(KeyCode.Escape)) escPul=false; // Detecta si no está pulsado

    //Estado pausa
    if (estado == Estados.Pausa)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !escPul)
        {
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
}
