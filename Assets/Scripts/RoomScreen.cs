using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class RoomScreen : MonoBehaviourPun {

    public Text roomNameText;
    public Text playerListText;
    public Text chatText;
    public InputField chatInputField;
    public Button startButton;
    public ScrollRect scrollRect;

    private void OnEnable() {
        chatText.text = "";
        chatInputField.text = "";
        chatInputField.Select();
        string message = PhotonNetwork.LocalPlayer.NickName + " joined the room!\n";
        photonView.RPC("AddChatText", RpcTarget.AllBuffered, message);
    }

    [PunRPC]
    public void Refresh() {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        playerListText.text = "";
        foreach (Player player in PhotonNetwork.PlayerList)
            playerListText.text += player.NickName + "\n";

        startButton.interactable = PhotonNetwork.IsMasterClient;
    }

    public void OnStartGameButtonPressed() {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel("Game");
    }

    public void OnLeaveRoomButtonPressed() {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return))
            OnSendButtonPressed();
    }

    public void OnSendButtonPressed() {
        chatInputField.ActivateInputField();

        if (chatInputField.text == "")
            return;

        string message = PhotonNetwork.LocalPlayer.NickName + ": " + chatInputField.text + "\n";
        photonView.RPC("AddChatText", RpcTarget.AllBuffered, message);
        chatInputField.text = "";
    }

    [PunRPC]
    public void AddChatText(string message) {
        chatText.text += message;
        scrollRect.verticalNormalizedPosition = 0f;
    }

}
