using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleManager : MonoBehaviour
{
    public GameObject TargetObject;
    public Vector3 cameraOffset;
    public int RPM = 6;
    #region Unity Event
    void Start()
    {
        TargetObject.transform.position = Camera.main.gameObject.transform.position - cameraOffset;
    }
    #endregion

    #region API
    public void TurnLeft(float eulerAngle){
        Debug.Log("hi");
    }
    #endregion
}
