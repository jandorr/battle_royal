using UnityEngine;
using Photon.Pun;


public class Projectile : MonoBehaviour {

    public int attackerId;
    public int damage;
    public float speed;
    public float hitForce = 10f;
    public Rigidbody rb;
    public AudioClip launchSound;
    public AudioClip flySound;
    public GameObject hitFx;

    private void Awake() {
        GetComponent<AudioSource>().clip = flySound;
    }

    public void Launch(int attackerId) {
        this.attackerId = attackerId;

        if (launchSound)
            AudioSource.PlayClipAtPoint(launchSound, transform.position);

        if (flySound)
            GetComponent<AudioSource>().Play();

        rb.velocity = transform.forward * speed;
        Destroy(gameObject, 5f);
    }

    // Projectiles need to be triggers because it will be instantiated on all clients.
    private void OnTriggerEnter(Collider other) {
        Destroy(gameObject);
        spawnParticle();

        if (other.CompareTag("Player")) {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController.photonView.IsMine && playerController.id != attackerId)
                playerController.TakeDamage(attackerId, damage);
        }
        else if (other.GetComponent<PUN2_RigidbodySync>() != null && PhotonNetwork.IsMasterClient)
            other.GetComponent<Rigidbody>().AddForce(transform.forward.normalized * hitForce, ForceMode.Impulse);

        if (other.GetComponent<Destructable>())
            other.GetComponent<Destructable>().spawnParticle(transform.position);
    }

    private void spawnParticle() {
        if (hitFx == null)
            return;

        GameObject particle = Instantiate(hitFx);
        particle.transform.position = transform.position + particle.transform.position;
        particle.SetActive(true);
    }

}
