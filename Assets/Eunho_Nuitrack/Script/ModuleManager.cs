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
    public double RotateTick = 0.01f;
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
    bool readyToTurn = true;
    async void rotateModule(float startAngle, float endAngle){
        double gestureDelayTime = TrackManager.Instance.GetGestureDelayTime();
        double spinAngle = Mathf.Abs(endAngle - startAngle) / 360 ;
        double spinPerSecond = RPM / 60;
        double rotateDelayTime = spinAngle / spinPerSecond;
        double rotateTime = rotateDelayTime < gestureDelayTime ? rotateDelayTime : gestureDelayTime;
        waitForNextTurn(rotateTime);
        for(double i = RotateTick; i <= rotateTime; i += RotateTick){
            currentAngle = Mathf.Lerp(startAngle, endAngle, (float)(i / rotateTime));
            TargetObject.transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            await UniTask.Delay(TimeSpan.FromSeconds(RotateTick));
        }
    }
    async void waitForNextTurn(double delayTime){
        readyToTurn = false;
        await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
        readyToTurn = true;
    }
}
