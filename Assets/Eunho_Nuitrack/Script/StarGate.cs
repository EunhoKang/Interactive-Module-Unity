using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StarGate : MonoBehaviour
{
    public Vector3 cameraOffset;
    public float RPM = 6;
    public int RotateTickCount = 250;
    float currentAngle = 0;
    int currentLayer = 0;
    Vector3 vectorFront;
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
    public MeshRenderer cityLightEffect;
    public MeshRenderer streetLightEffect;
    #endregion
    #region Unity Events
    void Start(){
        //transform.position = Camera.main.gameObject.transform.position - cameraOffset;
        vectorFront = transform.forward;
        isEnlargementMode = false;
        zodiacSigns[3] = SpringZodiacSigns;
        zodiacSigns[2] = SummerZodiacSigns;
        zodiacSigns[1] = AutumnZodiacSigns;
        zodiacSigns[0] = WinterZodiacSigns;
        resetZodiac();
    }
    void Update(){
        bool leastOneHitted= false;
        Vector3 leftHandPosition = TrackManager.Instance.GetLeftHandPosition();
        bool leftHandGrabbed = TrackManager.Instance.GetLeftHandGrabbed();
        if( ! leftHandGrabbed ) {
            RaycastHit leftHit;
            int layerMask = 1 << getLayer();
            if ( Physics.Raycast( leftHandPosition, vectorFront, out leftHit, Mathf.Infinity, layerMask) ) {
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
            int layerMask = 1 << getLayer();
            if ( Physics.Raycast( rightHandPosition, vectorFront, out rightHit, Mathf.Infinity, layerMask ) ) {
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
        cityLightEffect.material.EnableKeyword("_EMISSION");
        streetLightEffect.material.EnableKeyword("_EMISSION");
    }
    public void SetCityLightFalse()
    {
        cityLightEffect.material.DisableKeyword("_EMISSION");
        streetLightEffect.material.DisableKeyword("_EMISSION");
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
        if(endAngle > startAngle){
            ArduinoManager.Instance.SpinClockwise();
            currentLayer = ( currentLayer + 1 ) % 4;
        }
        else{ 
            ArduinoManager.Instance.SpinCounterClockwise();
            currentLayer = ( currentLayer + 3 ) % 4;
        }
        for(double i = rotateTick; i <= rotateDelayTime; i += rotateTick){
            currentAngle = Mathf.Lerp(startAngle, endAngle, (float)(i / rotateDelayTime));
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            await UniTask.Delay(TimeSpan.FromSeconds(rotateTick));
        }
        targetZodiacSigns[0] = targetZodiacSigns[1] = null;
    }
    async void waitForNextTurn(double delayTime){
        notReadyToTurn = true;
        await UniTask.Delay(TimeSpan.FromSeconds(delayTime));
        notReadyToTurn = false;
    }
    void resetZodiac(){
        for ( int i = 0; i < 4; ++i ) {
            foreach ( var zodiacSign in zodiacSigns[i] )
                zodiacSign.gameObject.SetActive(true);
        }
    }
    int getLayer(){
        return LayerMask.NameToLayer($"s{currentLayer + 1}");   
    }
}
