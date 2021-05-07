using UnityEngine;


public class CameraController : MonoBehaviour {

    public float sensitivity = 10;
    public float spectatorSpeed = 10;

    private float mouseX;
    private float mouseY;
    private bool isSpectator = false;


    private void FixedUpdate() {
        if (GameManager.instance.paused)
            return;

        // get mouse input
        //mouseX += Input.GetAxis("Mouse X") * sensitivity;
        //mouseY += Input.GetAxis("Mouse Y") * sensitivity;

        mouseX += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * sensitivity;
        mouseY += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * sensitivity;

        // clamp the vertical rotation
        mouseY = Mathf.Clamp(mouseY, -90, 90);

        // are we spectating?
        if (isSpectator) {
            // rotate cam
            transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);

            // move cam
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            float y = 0;

            if (Input.GetKey(KeyCode.E))
                y = 1;
            else if (Input.GetKey(KeyCode.Q))
                y = -1;

            Vector3 dir = transform.right * x + transform.up * y + transform.forward * z;
            transform.position += dir * spectatorSpeed * Time.deltaTime;
        }
        else {
            // rotate cam vertically
            transform.localRotation = Quaternion.Euler(-mouseY, 0, 0);

            // rotate player horizontally
            transform.parent.rotation = Quaternion.Euler(0, mouseX, 0);
        }
    }

    public void SetAsSpectator() {
        isSpectator = true;
        transform.parent = null;

        // hide weapon
        transform.GetChild(0).gameObject.SetActive(false);
    }

}
