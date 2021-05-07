using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class GameLog : MonoBehaviour {

    public static GameLog instance;
    private Text text;

    private void Awake() {
        instance = this;
        text = GetComponent<Text>();
    }

    [PunRPC]
    public void Log(string msg) {
        text.text += msg + "\n";
    }

}
