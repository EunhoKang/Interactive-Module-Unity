using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform camerarig;
    public float rotateSpeed;

    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.E))
        {
            transform.RotateAround(camerarig.position, Vector3.up, rotateSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.RotateAround(camerarig.position, -Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}
