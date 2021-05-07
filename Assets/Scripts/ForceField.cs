using UnityEngine;
using Photon.Pun;


public class ForceField : MonoBehaviour {

    public int playerDamage;
    public float shrinkWaitTime;
    public float shrinkAmount;
    public float shrinkDuration;
    public float minShrinkAmount;

    private bool shrink;
    private float lastShrinkEndTime;
    private float targetDiameter;
    private float lastPlayerCheckTime;

    private void Start() {
        lastShrinkEndTime = Time.time;
        targetDiameter = transform.localScale.x;
    }

    private void Update() {
        if (shrink) {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * targetDiameter, (shrinkAmount / shrinkDuration) * Time.deltaTime);
            if (transform.localScale.x == targetDiameter) {
                shrink = false;
                lastShrinkEndTime = Time.time;
            }
        }
        else {
            if (Time.time - lastShrinkEndTime >= shrinkWaitTime && transform.localScale.x > minShrinkAmount) {
                // make sure we don't shrink bellow the min amount
                if (transform.localScale.x - shrinkAmount > minShrinkAmount)
                    targetDiameter -= shrinkAmount;
                else
                    targetDiameter = minShrinkAmount;

                shrink = true;
            }
        }

        CheckPlayers();
    }

    private void CheckPlayers() {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (Time.time - lastPlayerCheckTime > 1f) {
            lastPlayerCheckTime = Time.time;

            // loop through all playerControllers
            foreach (PlayerController player in GameManager.instance.playerControllers) {
                if (!player || player.dead)
                    continue;

                if (Vector3.Distance(Vector3.zero, player.transform.position) >= transform.localScale.x)
                    player.photonView.RPC("TakeDamage", player.photonPlayer, 0, playerDamage);
            }
        }
    }

}
