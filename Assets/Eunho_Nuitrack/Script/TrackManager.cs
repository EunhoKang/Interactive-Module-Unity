using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using NuitrackSDK;
using UnityEngine.Events;
using System;

public class TrackManager : MonoBehaviour
{
    public static TrackManager Instance;
    void Awake(){
        if(Instance == null) Instance = this;
        else if(Instance != this){
            Destroy(this.gameObject);
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    } 
    public nuitrack.JointType[] TypeJoint;
    public GameObject PrefabJoint;
    public Vector3 CameraOffset;
    GameObject[] createdJoint;
    UserData user;
    Vector3 leftHandPosition;
    Vector3 rightHandPosition;
    bool leftHandGrabbed;
    bool rightHandGrabbed;
    bool leftFirstGrabbed;
    ZodiacSign[] targetZodiacSigns = new ZodiacSign[2];
    #region Tracking Event
    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeRight;
    public UnityEvent OnSwipeUpWithGrab;
    public UnityEvent OnSwipeUpWithoutGrab;
    public UnityEvent OnSwipeDownWithGrab;
    public UnityEvent OnSwipeDownWithoutGrab;
    public UnityEvent OnCollideWithStar;
    public UnityEvent OnPush;
    #endregion
    #region Unity Event
    void Start()
    {
        gameObject.transform.position = Camera.main.gameObject.transform.position - CameraOffset;
        createdJoint = new GameObject[TypeJoint.Length];
        for ( int i = 0; i < TypeJoint.Length; i++ )
        {
            createdJoint[i] = Instantiate(PrefabJoint);
            createdJoint[i].transform.SetParent(transform);
        }
    }
    void Update()
    {
        user = NuitrackManager.Users.Current;
        if( user != null ) {
            handTrack();
            raycastToEmphasize();
            handClick();
            gestureRecognize();
        }
    }
    #endregion
    #region API
    public ZodiacSign[] GetTargetZodiacSigns(){
        return targetZodiacSigns;
    }
    public ZodiacSign GetTargetZodiacSign(){
        if( leftHandGrabbed && rightHandGrabbed ) return targetZodiacSigns[ leftFirstGrabbed ? 0 : 1 ];
        else return targetZodiacSigns[ leftHandGrabbed ? 0 : 1 ];
    }
    public Vector3 GetLeftHandPosition(){
        return leftHandPosition;
    }
    public Vector3 GetRightHandPosition(){
        return rightHandPosition;
    }
    public bool GetLeftHandGrabbed(){
        return leftHandGrabbed;
    }
    public bool GetRightHandGrabbed(){
        return rightHandGrabbed;
    }
    #endregion
    #region HandTrack
    void handTrack(){
        if ( user.Skeleton != null ) {
            for ( int i = 0; i < TypeJoint.Length; i++ )
                updateJoint(i);
        }
        else 
            Debug.Log("Skeleton not found");
    }
    void updateJoint(int jointNumber){
        Vector3 jointPos = flipRightToLeft(user.Skeleton.GetJoint(TypeJoint[jointNumber]).Position);
        createdJoint[jointNumber].transform.localPosition = jointPos;
        if ( TypeJoint[jointNumber] == nuitrack.JointType.LeftHand ) leftHandPosition = jointPos;
        else if ( TypeJoint[jointNumber] == nuitrack.JointType.RightHand ) rightHandPosition = jointPos;
    }
    Vector3 flipRightToLeft(Vector3 objectPosition){
        objectPosition.x *= -1;
        return objectPosition;
    }
    void handClick(){
        if( user.LeftHand != null ) 
            leftHandGrabbed = user.LeftHand.Click;
        if( user.RightHand != null )
            rightHandGrabbed = user.RightHand.Click;
        if( leftHandGrabbed && !rightHandGrabbed ) leftFirstGrabbed = true;
        if( !leftHandGrabbed ) leftFirstGrabbed = false;
    }
    #endregion
    #region Raycast
    void raycastToEmphasize() {
        bool leastOneHitted= false;
        
        if( ! leftHandGrabbed ) {
            RaycastHit leftHit;
            if ( Physics.Raycast( leftHandPosition, transform.forward, out leftHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Star") ) ) {
                Star hittedStar = leftHit.collider.gameObject.GetComponent<Star>();
                if( hittedStar != null ) {
                    targetZodiacSigns[0] = hittedStar.GetParentZodiacSign();
                    leastOneHitted = true;
                }
            }
            else
                targetZodiacSigns[0] = null;
        }
        
        if( ! rightHandGrabbed ) {
            RaycastHit rightHit;
            if ( Physics.Raycast( rightHandPosition, transform.forward, out rightHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Star") ) ) {
                Star hittedStar = rightHit.collider.gameObject.GetComponent<Star>();
                if( hittedStar != null ) {
                    targetZodiacSigns[1] = hittedStar.GetParentZodiacSign();
                    leastOneHitted = true;
                }
                else 
                    targetZodiacSigns[1] = null;
            }
        }

        if( leastOneHitted ) OnCollideWithStar.Invoke();
    }
    #endregion
    #region Gesture
    bool readyToRecognize = true;
    void gestureRecognize(){
        if( user.GestureType != null && readyToRecognize) {
            TriggerEventByGesture(user.GestureType);
        }
    }
    void TriggerEventByGesture(nuitrack.GestureType? gesture){
        switch(gesture){
        case nuitrack.GestureType.GestureSwipeLeft:
            OnSwipeLeft.Invoke();
            break;
        case nuitrack.GestureType.GestureSwipeRight:
            OnSwipeRight.Invoke();
            break;
        case nuitrack.GestureType.GestureSwipeUp:
            if( ( leftHandGrabbed && targetZodiacSigns[0] != null ) || ( rightHandGrabbed && targetZodiacSigns[1] != null ) )
                OnSwipeUpWithGrab.Invoke();
            else{
                OnSwipeUpWithoutGrab.Invoke();
            }
            break;
        case nuitrack.GestureType.GestureSwipeDown:
            if( ( leftHandGrabbed && targetZodiacSigns[0] != null ) || ( rightHandGrabbed && targetZodiacSigns[1] != null ) )
                OnSwipeDownWithGrab.Invoke();
            else{
                OnSwipeDownWithoutGrab.Invoke();
            }
            break;
        case nuitrack.GestureType.GesturePush:
            OnPush.Invoke();
            break;
        default:
            break;
        }
    }
    #endregion
}