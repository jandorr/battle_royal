using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class MenuManager : MonoBehaviourPunCallbacks {

    public static MenuManager instance;

    [Header("Network")]
    public byte maxPlayersPerRoom = 10;
    public List<RoomInfo> roomInfoList = new List<RoomInfo>();

    [Header("Screens")]
    public GameObject connectingScreen;
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject roomScreen;
    public GameObject roomBrowserScreen;

    private void Awake() {
        Cursor.lockState = CursorLockMode.None;
        instance = this;
    }

    private void Start() {
        if (!PhotonNetwork.IsConnected) {
            Debug.Log("Connecting to master server ...");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        if (PhotonNetwork.CurrentRoom == null)
            PhotonNetwork.JoinLobby();
        else {
            SetScreen(roomScreen);
            roomScreen.GetComponent<RoomScreen>().Refresh();
        }
    }

    public void SetScreen(GameObject screen) {
        // disable all screens
        connectingScreen.SetActive(false);
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);

        // enable screen
        screen.SetActive(true);
    }

    public void ExitGame() {
        Application.Quit();
    }

    // Overrides

    public override void OnConnectedToMaster() {
        Debug.Log("Connected to master server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() {
        Debug.Log("Joined lobby: " + PhotonNetwork.CurrentLobby.Name);
        SetScreen(mainScreen);
    }

    public override void OnJoinedRoom() {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        SetScreen(roomScreen);
        roomScreen.GetComponent<RoomScreen>().photonView.RPC("Refresh", RpcTarget.All);
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        Debug.LogWarningFormat("Failed to join room: {0}", message);
    }

    public override void OnPlayerEnteredRoom(Player other) {
        Debug.LogFormat("{0} entered room.", other.NickName); // not seen if you're the player connecting
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        Debug.LogFormat("{0} left the room.", otherPlayer.NickName); // seen when other disconnects
        roomScreen.GetComponent<RoomScreen>().Refresh();
    }

    public override void OnDisconnected(DisconnectCause cause) {
        Debug.LogWarningFormat("Disconnected from server: {0}", cause);
        SetScreen(connectingScreen);
        Invoke(nameof(Start), 5);
    }

    public override void OnRoomListUpdate(List<RoomInfo> allRooms) {
        roomInfoList = allRooms;
    }

}
