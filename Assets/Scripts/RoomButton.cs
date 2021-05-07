using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class RoomButton : MonoBehaviour {

    public Text roomName;
    public Text playerCount;
    public Button button;

    public void Refresh(RoomInfo roomInfo) {
        roomName.text = roomInfo.Name;
        playerCount.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
        button.onClick.AddListener(() => { PhotonNetwork.JoinRoom(roomInfo.Name); });
    }

}
