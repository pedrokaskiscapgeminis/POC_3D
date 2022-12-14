using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class PlayerSpawn : MonoBehaviour
{
public GameObject[] playerPrefabs;
public Transform[] spawnPoints;

private void Start() {
    {
        int randomNumber = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomNumber];
        GameObject playerToSpawn = playerPrefabs[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"]];
        playerToSpawn = (GameObject) PhotonNetwork.Instantiate(playerToSpawn.name, spawnPoint.position, Quaternion.identity);
        playerToSpawn.GetComponent<SC_FPSController>().enabled = true;
        playerToSpawn.transform.Find("PlayerCamera").gameObject.SetActive(true);
    }
}
}
