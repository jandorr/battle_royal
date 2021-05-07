using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
//using Photon.Voice.Unity;
//using Photon.Voice.PUN;

public class PlayerController : MonoBehaviourPun {

    public int id;
    public int curHp = 100;
    public int maxHp = 100;
    public int kills = 0;
    public float speed = 4;
    public float jumpForce = 7;
    public bool dead = false;
    public bool isDriving;
    public Player photonPlayer;
    public Weapon currentWeapon;
    public Transform weaponContainer;
    public Camera playerCamera;
    public Image balloonMessageImage;
    public Text balloonMessageText;

    private int curAttackerId;
    private bool flashingDamage;
    private Rigidbody rb;
    private MeshRenderer mr;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        mr = GetComponent<MeshRenderer>();
    }

    private void Start() {

    }

    void Update() {
        if (!photonView.IsMine || dead || GameManager.instance.paused)
            return;

        if (!isDriving) {
            Move();

            if (Input.GetButtonDown("Jump"))
                Jump();
        }

        if (Input.GetButtonDown("Fire1"))
            Shoot();
    }

    void Move() {
        // get input axis
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // animate weapon
        if (currentWeapon != null && currentWeapon.animator != null)
            currentWeapon.animator.SetFloat("Speed", Mathf.Abs(h) + Mathf.Abs(v));

        // calculate direction
        Vector3 dir = (transform.forward * v + transform.right * h) * speed;
        dir.y = rb.velocity.y;

        // set that as our velocity
        rb.velocity = dir;
    }

    void Jump() {
        // create a ray facing down
        Ray ray = new Ray(transform.position, Vector3.down);

        // shoot the ray
        if (Physics.Raycast(ray, 1.5f))
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void Shoot() {
        if (currentWeapon == null)
            return;
        currentWeapon.Shoot(id);
        CanvasController.instance.Refresh();
    }

    public void DropWeapon() {
        if (currentWeapon == null)
            return;
        currentWeapon.transform.parent = null;
        currentWeapon.GetComponent<Collider>().enabled = true;
        currentWeapon.GetComponent<Rigidbody>().isKinematic = false;
        currentWeapon.GetComponent<Rigidbody>().AddForce(playerCamera.transform.forward * 10, ForceMode.Impulse);
        CanvasController.instance.Refresh();
        if (currentWeapon != null && currentWeapon.animator != null)
            currentWeapon.animator.SetFloat("Speed", 0);
    }

    public void PickUpWeapon(Weapon weapon) {
        currentWeapon = weapon;
        weapon.transform.parent = weaponContainer.transform;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.forward = weaponContainer.transform.forward;
        currentWeapon.GetComponent<Collider>().enabled = false;
        weapon.GetComponent<Rigidbody>().isKinematic = true;
        CanvasController.instance.Refresh();
    }

    [PunRPC]
    public void Initialize(Player player) {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.playerControllers.Add(this);

        if (!photonView.IsMine) {
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
            GetComponentInChildren<AudioSource>().enabled = false;
            GetComponentInChildren<CameraController>().enabled = false;
            rb.isKinematic = true;
            GetComponentInChildren<Text>().text = player.NickName;
            //Speaker speaker = gameObject.AddComponent<Speaker>();
            //GetComponent<PhotonVoiceView>().SpeakerInUse = speaker;
        }
        else {
            CanvasController.instance.Refresh();
            GetComponentInChildren<Canvas>().gameObject.SetActive(false);
            //GetComponent<PhotonVoiceView>().UsePrimaryRecorder = true;
        }
    }

    [PunRPC]
    public void TakeDamage(int attackerId, int damage) {
        if (dead)
            return;

        curHp -= damage;
        curAttackerId = attackerId;

        // flash the player red
        photonView.RPC("DamageFlash", RpcTarget.Others);

        // update the health bar UI
        CanvasController.instance.Refresh();

        //die if no health left
        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }

    [PunRPC]
    public void AddKill() {
        kills++;
        CanvasController.instance.Refresh();
    }

    [PunRPC]
    public bool Heal(int amountToHeal) {
        if (curHp == maxHp)
            return false;
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);
        CanvasController.instance.Refresh();
        return true;
    }

    [PunRPC]
    public bool AddAmmo(int num) {
        if (currentWeapon == null || currentWeapon.curAmmo == currentWeapon.maxAmmo)
            return false;
        Debug.Log("adding ammo");
        currentWeapon.AddAmmo(num);
        return true;
    }

    [PunRPC]
    private void DamageFlash() {
        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine() {
            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }

    [PunRPC]
    private void Die() {
        DropWeapon();

        curHp = 0;
        dead = true;

        GameManager.instance.playersAlive--;
        CanvasController.instance.Refresh();

        // host will check the win condition
        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        // is this our local player?
        if (photonView.IsMine) {
            if (curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            // set the cam to spectator
            GetComponentInChildren<CameraController>().SetAsSpectator();

            // disable the physics and hide the player
            rb.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }

    [PunRPC]
    public void Message(string value) {
        balloonMessageText.text = value;
        balloonMessageText.color = Color.black;
        balloonMessageImage.color = Color.white;
        StopAllCoroutines();
        StartCoroutine(FadeMessage());
    }

    IEnumerator FadeMessage() {
        yield return new WaitForSeconds(5f);

        while (balloonMessageImage.color.a != 0) {
            balloonMessageImage.color = new Color(1, 1, 1, balloonMessageImage.color.a - 0.1f);
            balloonMessageText.color = new Color(0, 0, 0, balloonMessageText.color.a - 0.1f);
            yield return new WaitForSeconds(.02f);
        }

    }
}
