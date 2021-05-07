using UnityEngine;
using Photon.Pun;


public enum PickupType {
    Health,
    Ammo
}

public class Pickup : MonoBehaviourPun {

    public PickupType type;
    public int value;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {

            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController.photonView.IsMine) {
                if (type == PickupType.Health) {
                    if (playerController.Heal(value))
                        Destroy(gameObject);
                }
                else if (type == PickupType.Ammo)
                    if (playerController.AddAmmo(value))
                        Destroy(gameObject);
            }

        }
    }

}
