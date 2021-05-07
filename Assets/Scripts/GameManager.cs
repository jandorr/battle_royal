using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class GameManager : MonoBehaviourPunCallbacks {

    public static GameManager instance;
    public bool paused = false;
    public int playersAlive;
    public GameObject localPlayer;
    public List<PlayerController> playerControllers = new List<PlayerController>();
    private GameObject[] spawnPoints;

    private void Start() {
        instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        playersAlive = PhotonNetwork.CurrentRoom.PlayerCount;

        GameObject spawnPoint = spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1];
        localPlayer = PhotonNetwork.Instantiate("Player", spawnPoint.transform.position, Quaternion.identity);
        localPlayer.GetComponent<PlayerController>().photonView.RPC("Initialize", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);


    }

    private void GoBackToMenu() {
        PhotonNetwork.LoadLevel("Menu");
    }

    public PlayerController GetPlayer(int playerId) {
        foreach (PlayerController playerController in playerControllers) {
            if (playerController != null && playerController.id == playerId)
                return playerController;
        }
        return null;
    }

    public PlayerController GetPlayer(GameObject playerObject) {
        foreach (PlayerController playerController in playerControllers) {
            if (playerController != null && playerController.gameObject == playerObject)
                return playerController;
        }
        return null;
    }

    public void CheckWinCondition() {
        if (playersAlive == 1)
            photonView.RPC("WinGame", RpcTarget.All, playerControllers.First(x => !x.dead).id);
    }

    public void PauseGame() {
        paused = true;
        localPlayer.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public void ResumeGame() {
        paused = false;
    }

    [PunRPC]
    private void WinGame(int winningPlayer) {
        CanvasController.instance.SetCrosshairActive(false);
        CanvasController.instance.SetCenterText(GetPlayer(winningPlayer).photonPlayer.NickName + " Wins!");
        Invoke("GoBackToMenu", 5f);
    }

    // Overrides

    public override void OnDisconnected(DisconnectCause disconnectCause) {
        Debug.Log("Disconnected from master server: " + disconnectCause.ToString());
        PhotonNetwork.LoadLevel("Menu");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        playersAlive--;
        CanvasController.instance.Refresh();
        if (PhotonNetwork.IsMasterClient)
            CheckWinCondition();
    }

    public override void OnLeftRoom() {
        GoBackToMenu();
    }

}
