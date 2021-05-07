using UnityEngine;


public class Destructable : MonoBehaviour {
    public GameObject hitFx;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Projectile"))
            spawnParticle(other.transform.position);
    }

    public void spawnParticle(Vector3 projectilePos) {
        if (hitFx == null)
            return;
        GameObject particle = Instantiate(hitFx);
        particle.transform.position = projectilePos + particle.transform.position;
        particle.SetActive(true);
    }


}
