using UnityEngine;
using System.Collections;
using Photon.Pun;


public class Weapon : MonoBehaviourPun {

    public int curAmmo;
    public int maxAmmo;
    public float fireRate;
    public GameObject hitFx;
    public GameObject projectilePrefab;
    public Transform projectileTransform;
    public int rayDamage = 10;
    public float rayRange = 100;
    public float hitForce = 10;
    public AudioClip rayAudio;
    public Animator animator;

    private WaitForSeconds rayDuration = new WaitForSeconds(0.02f);
    private LineRenderer lineRenderer;
    private float nextFire;

    void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        animator = GetComponent<Animator>();
    }

    public void Shoot(int playerId) {
        if (curAmmo <= 0 || Time.time < nextFire)
            return;

        nextFire = Time.time + fireRate;
        curAmmo--;

        Vector3 cameraOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
        Vector3 cameraForward = Camera.main.transform.forward;

        if (projectilePrefab != null) {
            if (Physics.Raycast(cameraOrigin, cameraForward, out RaycastHit hit, rayRange))
                cameraForward = (hit.point - projectileTransform.position).normalized;
            photonView.RPC("SpawnProjectile", RpcTarget.All, projectileTransform.position, cameraForward, playerId);
        }
        else {
            photonView.RPC("SpawnRay", RpcTarget.All, projectileTransform.position, cameraOrigin, cameraForward, playerId);
        }

    }

    [PunRPC]
    void SpawnRay(Vector3 projectileSpawnPos, Vector3 cameraOrigin, Vector3 cameraForward, int playerId) {
        StartCoroutine(ShotEffect());
        lineRenderer.SetPosition(0, projectileSpawnPos);

        if (Physics.Raycast(cameraOrigin, cameraForward, out RaycastHit hit, rayRange)) {
            lineRenderer.SetPosition(1, hit.point);
            spawnParticle(hit.point);

            PlayerController playerController = hit.collider.GetComponent<PlayerController>();
            if (playerController != null && playerController.photonView.IsMine)
                playerController.TakeDamage(playerId, rayDamage);

            if (hit.collider.GetComponent<PUN2_RigidbodySync>() != null && PhotonNetwork.IsMasterClient)
                hit.rigidbody.AddForce(-hit.normal * hitForce, ForceMode.Impulse);

            if (hit.collider.gameObject.GetComponent<Destructable>())
                hit.collider.gameObject.GetComponent<Destructable>().spawnParticle(hit.point);
        }
        else
            lineRenderer.SetPosition(1, projectileSpawnPos + cameraForward * rayRange);
    }

    private void spawnParticle(Vector3 pos) {

        GameObject particle = Instantiate(hitFx);
        particle.transform.position = pos + particle.transform.position;
        particle.SetActive(true);

    }

    [PunRPC]
    void SpawnProjectile(Vector3 projectileSpawnPos, Vector3 forward, int playerId) {
        if (animator != null)
            animator.SetTrigger("Shoot");

        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPos, Quaternion.identity);
        projectile.transform.forward = forward;
        projectile.GetComponent<Projectile>().Launch(playerId);
    }

    private IEnumerator ShotEffect() {
        AudioSource.PlayClipAtPoint(rayAudio, transform.position);
        lineRenderer.enabled = true;
        yield return rayDuration;
        lineRenderer.enabled = false;
    }

    public void AddAmmo(int num) {
        Debug.Log("adding ammo");
        curAmmo = Mathf.Clamp(curAmmo + num, 0, maxAmmo);
        CanvasController.instance.Refresh();
    }

    private void OnCollisionEnter(Collision collision) {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        player.DropWeapon();
        player.PickUpWeapon(this);
    }

}
