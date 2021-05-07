using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.Animations;

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}


public class SimpleCarController : MonoBehaviourPun {
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public Transform playerSeat;
    public PlayerController driver;
    private float motor;
    private float steering;

    private void Start() {
        GetComponent<Rigidbody>().centerOfMass = new Vector3(0, -1, 0);
    }

    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider) {
        if (collider.transform.childCount == 0)
            return;

        Transform visualWheel = collider.transform.GetChild(0);

        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }


    public void FixedUpdate() {

        if (driver == null || !driver.photonView.IsMine || driver.dead)
            return;

        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.E)) {
            photonView.RPC("RemoveDriver", RpcTarget.All);
            return;
        }


        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }


        driver.transform.position = playerSeat.position;

    }


    private void OnTriggerStay(Collider other) {
        if (driver != null)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (!player.photonView.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.E)) {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            photonView.RPC("SetDriver", RpcTarget.All, player.id);
        }



    }

    [PunRPC]
    private void SetDriver(int id) {

        driver = GameManager.instance.GetPlayer(id);
        driver.isDriving = true;
        driver.GetComponent<Rigidbody>().isKinematic = true;

        if (driver.photonView.IsMine) {
            GetComponent<Rigidbody>().isKinematic = false;
        }
        else
            GetComponent<Rigidbody>().isKinematic = true;

    }

    [PunRPC]
    private void RemoveDriver() {

        driver.GetComponent<Rigidbody>().isKinematic = false;
        driver.isDriving = false;

        if (driver.photonView.IsMine)
            driver.GetComponent<Rigidbody>().AddForce(Vector3.up * 400, ForceMode.Impulse);

        driver = null;
        motor = 0;
        steering = 0;

        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }

    }

}
