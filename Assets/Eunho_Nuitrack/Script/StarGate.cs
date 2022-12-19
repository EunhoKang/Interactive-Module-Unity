using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StarGate : MonoBehaviour
{
    public Vector3 cameraOffset;
    public float RPM = 6;
    public int RotateTickCount = 250;
    float currentAngle = 0;
    private bool isEnlargementMode;
    private ZodiacSign enlargedZodiacSign;
    private List<ZodiacSign>[] zodiacSigns= new List<ZodiacSign>[4];
    ZodiacSign[] targetZodiacSigns = new ZodiacSign[2];
    #region Stars
    public List<ZodiacSign> SpringZodiacSigns = new List<ZodiacSign>();
    public List<ZodiacSign> SummerZodiacSigns = new List<ZodiacSign>();
    public List<ZodiacSign> AutumnZodiacSigns = new List<ZodiacSign>();
    public List<ZodiacSign> WinterZodiacSigns = new List<ZodiacSign>();
    public Transform EnlargeTransform;
    #endregion
    #region Effects
    public GameObject cityLightEffect;
    #endregion
    #region Unity Events
    void Start(){
        transform.position = Camera.main.gameObject.transform.position - cameraOffset;
        isEnlargementMode = false;
        zodiacSigns[0] = SpringZodiacSigns;
        zodiacSigns[1] = SummerZodiacSigns;
        zodiacSigns[2] = AutumnZodiacSigns;
        zodiacSigns[3] = WinterZodiacSigns;
        for ( int i = 0; i < 4; ++i ) {
            foreach ( var zodiacSign in zodiacSigns[i] ) {
                zodiacSign.gameObject.SetActive(true);
            }
        }
    }
    void Update(){
        bool leastOneHitted= false;
        Vector3 leftHandPosition = TrackManager.Instance.GetLeftHandPosition();
        bool leftHandGrabbed = TrackManager.Instance.GetLeftHandGrabbed();
        if( ! leftHandGrabbed ) {
            RaycastHit leftHit;
            if ( Physics.Raycast( leftHandPosition, transform.forward, out leftHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Star") ) ) {
                ZodiacSign hittedZodiacSign = leftHit.collider.gameObject.GetComponent<ZodiacSign>();
                if( hittedZodiacSign != null ) {
                    targetZodiacSigns[0] = hittedZodiacSign;
                    leastOneHitted = true;
                }
            }
            else
                targetZodiacSigns[0] = null;
        }
        
        Vector3 rightHandPosition = TrackManager.Instance.GetRightHandPosition();
        bool rightHandGrabbed = TrackManager.Instance.GetRightHandGrabbed();
        if( ! rightHandGrabbed ) {
            RaycastHit rightHit;
            if ( Physics.Raycast( rightHandPosition, transform.forward, out rightHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Star") ) ) {
                ZodiacSign hittedZodiacSign = rightHit.collider.gameObject.GetComponent<ZodiacSign>();
                if( hittedZodiacSign != null ) {
                    targetZodiacSigns[1] = hittedZodiacSign;
                    leastOneHitted = true;
                }
                else 
                    targetZodiacSigns[1] = null;
            }
        }

        if( leastOneHitted ) EmphasizeStar();
    }
    #endregion
    #region API
    public void TurnModule(float eulerAngle){
        rotateModule(currentAngle, currentAngle + eulerAngle);
    }
    public void EmphasizeStar(){
        for ( int i = 0; i < 4; ++i )
            foreach ( var zodiacSign in zodiacSigns[i] ) 
                zodiacSign.Unemphasize();
        foreach ( var zodiacSign in targetZodiacSigns) 
            if ( zodiacSign != null ) 
                zodiacSign.Emphasize();
    }
    public void EnlargeZodiacSign(){
        if( ( TrackManager.Instance.GetLeftHandGrabbed() && targetZodiacSigns[0] == null ) 
            || ( TrackManager.Instance.GetRightHandGrabbed() && targetZodiacSigns[1] == null ) ) return;
        if( isEnlargementMode ) return;
        isEnlargementMode = true;
        ZodiacSign targetZodiacSign = targetZodiacSigns[ TrackManager.Instance.GetLeftHandGrabbed() ? 0 : 1 ];
        for ( int i = 0; i < 4; ++i )
            foreach ( var zodiacSign in zodiacSigns[i] ) 
                zodiacSign.gameObject.SetActive(false);
        targetZodiacSign.gameObject.SetActive(true);
        targetZodiacSign.Enlarge(EnlargeTransform);
        enlargedZodiacSign = targetZodiacSign;
    }
    public void ReductZodiacSign(){
        if( ! isEnlargementMode ) return;
        isEnlargementMode = false;
        enlargedZodiacSign.Reduct();
        for ( int i = 0; i < 4; ++i )
            foreach ( var zodiacSign in zodiacSigns[i] ) 
                zodiacSign.gameObject.SetActive(true);
    }
    public void SetCityLightTrue()
    {
        cityLightEffect.SetActive(true);
    }
    public void SetCityLightFalse()
    {
        cityLightEffect.SetActive(false);
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
        if(endAngle > startAngle)
            ArduinoManager.Instance.SpinClockwise();
        else 
            ArduinoManager.Instance.SpinCounterClockwise();
        for(double i = rotateTick; i <= rotateDelayTime; i += rotateTick){
            currentAngle = Mathf.Lerp(startAngle, endAngle, (float)(i / rotateDelayTime));
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            await UniTask.Delay(TimeSpan.FromSeconds(rotateTick));
        }
    }
    async void waitForNextTurn(double delayTime){
        notReadyToTurn = true;
        await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
        notReadyToTurn = false;
    }
}
