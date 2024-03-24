using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        // EDITOR ONLY
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Load");
            return;
        }

        Vector3 randomPos = new Vector3(Random.Range(-13f, 13f), 4f, Random.Range(-13f, 13f));
        PhotonNetwork.Instantiate(playerPrefab.name, randomPos, Quaternion.identity);
    }
}
