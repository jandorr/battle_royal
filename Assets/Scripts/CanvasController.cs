using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class CanvasController : MonoBehaviour {
    public static CanvasController instance;
    public Text infoText;
    public Text ammoText;
    public Text centerText;
    public Slider healthBar;
    public GameObject crosshair;
    public GameObject pausePanel;

    private void Awake() {
        instance = this;
    }

    public void Refresh() {
        if (GameManager.instance.localPlayer == null)
            return;

        PlayerController playerController = GameManager.instance.localPlayer.GetComponent<PlayerController>();
        healthBar.maxValue = playerController.maxHp;
        infoText.text = "Alive: " + GameManager.instance.playersAlive + "\nKills: " + playerController.kills;

        healthBar.value = playerController.curHp;

        if (playerController.currentWeapon == null)
            ammoText.text = "";
        else
            ammoText.text = playerController.currentWeapon.curAmmo + " / " + playerController.currentWeapon.maxAmmo;

    }

    public void SetCrosshairActive(bool value) {
        crosshair.SetActive(value);
    }

    public void SetCenterText(string value) {
        centerText.text = value;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            if (pausePanel.activeSelf) {
                GameManager.instance.ResumeGame();
                SetCrosshairActive(true);
                pausePanel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
            }
            else {
                GameManager.instance.PauseGame();
                SetCrosshairActive(false);
                pausePanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void LeaveRoom() {
        Debug.Log("Leaving room ...");
        PhotonNetwork.Disconnect();
    }

    public void QuitGame() {
        Application.Quit();
    }

}
