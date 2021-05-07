using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class GameChat : MonoBehaviourPun {

    public Text chatText;
    public InputField chatInputField;

    private void Start() {
        string message = PhotonNetwork.LocalPlayer.NickName + " joined the game!\n";
        photonView.RPC("AddChatText", RpcTarget.AllBuffered, message);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            if (chatInputField.interactable)
                Send();
            else {
                chatInputField.interactable = true;
                chatInputField.ActivateInputField();
                GameManager.instance.paused = true;
            }
        }
    }

    public void Send() {
        chatInputField.interactable = false;
        chatInputField.DeactivateInputField();
        GameManager.instance.paused = false;

        if (chatInputField.text == "")
            return;

        string message = PhotonNetwork.LocalPlayer.NickName + ": " + chatInputField.text + "\n";
        photonView.RPC("AddChatText", RpcTarget.AllBuffered, message);
        GameManager.instance.localPlayer.GetComponent<PlayerController>().photonView.RPC("Message", RpcTarget.Others, chatInputField.text);
        chatInputField.text = "";

    }

    [PunRPC]
    public void AddChatText(string message) {
        chatText.text += message;
    }

}
