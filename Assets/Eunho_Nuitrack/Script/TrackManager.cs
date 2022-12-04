using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NuitrackSDK;

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
    UserData user;
    Vector3 leftHandPosition;
    Vector3 rightHandPosition;
    bool leftHandGrabbed;
    bool rightHandGrabbed;
    nuitrack.GestureType? gestureType;
    #region API
    public Vector3 GetLeftHandPosition(){
        return leftHandPosition;
    }
    public Vector3 GetRightHandPosition(){
        return rightHandPosition;
    }
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
    #region HandTrack
    void HandTrack(){
        if ( user.Skeleton != null ) {
            for ( int i = 0; i < typeJoint.Length; i++ )
                UpdateJoint(i);
            ShowHandState();
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
    void ShowHandState(){
        Debug.Log($"\nLeft({leftHandGrabbed})");
        //Debug.Log($"\nLeft({leftHandGrabbed}) : {leftHandPosition.x}, {leftHandPosition.y}, {leftHandPosition.z}");
        //Debug.Log($"\nLeft({leftHandGrabbed}) : {user.LeftHand.Position.x}, {user.LeftHand.Position.y}, {user.LeftHand.Position.z}");
        //Debug.Log($"\nRight({rightHandGrabbed}) : {rightHandPosition.x}, {rightHandPosition.y}, {rightHandPosition.z}");
    }
    #endregion
    #region Gesture
    void GestureRecognize(){
        if( user.GestureType != null ) {
            gestureType = user.GestureType;
            Debug.Log(gestureType.ToString());
        }
    }
    #endregion
}