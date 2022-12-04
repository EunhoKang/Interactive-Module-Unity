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
    public nuitrack.JointType[] typeJoint;
    GameObject[] CreatedJoint;
    public GameObject PrefabJoint;
    public Vector3 cameraOffset;
    public double GestureDelayTime = 0.4f;
    UserData user;
    Vector3 leftHandPosition;
    Vector3 rightHandPosition;
    bool leftHandGrabbed;
    bool rightHandGrabbed;
    #region Tracking Event
    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeRight;
    public UnityEvent OnSwipeUp;
    public UnityEvent OnSwipeDown;
    public UnityEvent OnPush;
    #endregion
    #region Unity Event
    void Start()
    {
        gameObject.transform.position = Camera.main.gameObject.transform.position - cameraOffset;
        CreatedJoint = new GameObject[typeJoint.Length];
        for ( int i = 0; i < typeJoint.Length; i++ )
        {
            CreatedJoint[i] = Instantiate(PrefabJoint);
            CreatedJoint[i].transform.SetParent(transform);
        }
    }
    void Update()
    {
        user = NuitrackManager.Users.Current;
        if( user != null ) {
            HandTrack();
            HandClick();
            GestureRecognize();
        }
        else 
            Debug.Log("User not found");
    }
    #endregion
    #region API
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
    void HandTrack(){
        if ( user.Skeleton != null ) {
            for ( int i = 0; i < typeJoint.Length; i++ )
                UpdateJoint(i);
        }
        else 
            Debug.Log("Skeleton not found");
    }
    void UpdateJoint(int jointNumber){
        Vector3 jointPos = FlipRightToLeft(user.Skeleton.GetJoint(typeJoint[jointNumber]).Position);
        CreatedJoint[jointNumber].transform.localPosition = jointPos;
        if ( typeJoint[jointNumber] == nuitrack.JointType.LeftHand ) leftHandPosition = jointPos;
        else if ( typeJoint[jointNumber] == nuitrack.JointType.RightHand ) rightHandPosition = jointPos;
    }
    Vector3 FlipRightToLeft(Vector3 objectPosition){
        objectPosition.x *= -1;
        return objectPosition;
    }
    void HandClick(){
        if( user.LeftHand != null ) leftHandGrabbed = user.LeftHand.Click;
        if( user.RightHand != null ) rightHandGrabbed = user.RightHand.Click;
    }
    #endregion
    #region Gesture
    bool readyToRecognize = true;
    void GestureRecognize(){
        if( user.GestureType != null && readyToRecognize) {
            TriggerEventByGesture(user.GestureType);
            WaitForNextGesture();
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
            OnSwipeUp.Invoke();
            break;
        case nuitrack.GestureType.GestureSwipeDown:
            OnSwipeDown.Invoke();
            break;
        case nuitrack.GestureType.GesturePush:
            OnPush.Invoke();
            break;
        default:
            break;
        }
    }
    async void WaitForNextGesture(){
        readyToRecognize = false;
        await UniTask.Delay(TimeSpan.FromSeconds(GestureDelayTime));
        readyToRecognize = true;
    }
    #endregion
}