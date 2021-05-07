using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class RoomBrowserScreen : MonoBehaviour {

    public GameObject roomButtonPrefab;
    public RectTransform roomButtonContainer;
    private string filter = "";

    private void OnEnable() {
        Refresh();
    }

    private void CreateRoomButton(RoomInfo roomInfo) {
        GameObject roomButton = Instantiate(roomButtonPrefab, roomButtonContainer.transform);
        roomButton.GetComponent<RoomButton>().Refresh(roomInfo);
    }

    public void Refresh() {
        foreach (Transform roomButton in roomButtonContainer.transform)
            Destroy(roomButton.gameObject);

        foreach (RoomInfo roomInfo in MenuManager.instance.roomInfoList)
            if (roomInfo.Name.Contains(filter))
                CreateRoomButton(roomInfo);
    }

    public void OnFilterValueChanged(InputField inputField) {
        filter = inputField.text;
        Refresh();
    }

}
