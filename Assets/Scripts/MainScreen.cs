using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class MainScreen : MonoBehaviour {

    public InputField playerNameInputField;
    public Button createRoomButton;
    public Button findRoomButton;
    private const string nameKey = "playerName";

    void Start() {
        if (PlayerPrefs.HasKey(nameKey))
            playerNameInputField.text = PlayerPrefs.GetString(nameKey);
    }

    public void OnPlayerNameValueChanged(InputField playerNameInput) {
        if (playerNameInput.text == "") {
            createRoomButton.interactable = false;
            findRoomButton.interactable = false;
        }
        else {
            createRoomButton.interactable = true;
            findRoomButton.interactable = true;

            PhotonNetwork.NickName = playerNameInput.text;
            PlayerPrefs.SetString(nameKey, playerNameInput.text);
        }
    }

}
