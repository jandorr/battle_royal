﻿using UnityEngine;


public class FaceMainCamera : MonoBehaviour {

    void Update() {
        transform.LookAt(Camera.main.transform);
    }

}
