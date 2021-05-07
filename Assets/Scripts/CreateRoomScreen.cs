using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class CreateRoomScreen : MonoBehaviour {

    public void OnCreateButtonPressed(InputField roomNameInput) {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = MenuManager.instance.maxPlayersPerRoom;
        PhotonNetwork.CreateRoom(roomNameInput.text, options);
    }

}
