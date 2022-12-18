using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class ModuleManager : MonoBehaviour
{
    public GameObject TargetObject;
    public Vector3 cameraOffset;
    public float RPM = 6;
    public int RotateTickCount = 250;
    float currentAngle = 0;
    #region Unity Event
    void Start()
    {
        TargetObject.transform.position = Camera.main.gameObject.transform.position - cameraOffset;
    }
    #endregion

    #region API
    public void TurnModule(float eulerAngle){
        rotateModule(currentAngle, currentAngle + eulerAngle);
    }
    #endregion
    bool notReadyToTurn = false;
    async void rotateModule(float startAngle, float endAngle){
        if( notReadyToTurn ) return;
        double spinAngle = Mathf.Abs(endAngle - startAngle) / 360 ;
        double spinPerSecond = RPM / 60;
        double rotateDelayTime = spinAngle / spinPerSecond;
        double rotateTick = rotateDelayTime / (float)RotateTickCount;
        waitForNextTurn(rotateDelayTime);
        for(double i = rotateTick; i <= rotateDelayTime; i += rotateTick){
            currentAngle = Mathf.Lerp(startAngle, endAngle, (float)(i / rotateDelayTime));
            TargetObject.transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            
            if(endAngle > startAngle)
                ArduinoManager.Instance.SpinClockwise();
            else 
                ArduinoManager.Instance.SpinCounterClockwise();
            
            await UniTask.Delay(TimeSpan.FromSeconds(rotateTick));
        }
    }
    async void waitForNextTurn(double delayTime){
        notReadyToTurn = true;
        await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
        notReadyToTurn = false;
    }
}
