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
    
    string message = "";
    public nuitrack.JointType[] typeJoint;
    GameObject[] CreatedJoint;
    public GameObject PrefabJoint;

    void Start()
    {
        CreatedJoint = new GameObject[typeJoint.Length];
        for (int q = 0; q < typeJoint.Length; q++)
        {
            CreatedJoint[q] = Instantiate(PrefabJoint);
            CreatedJoint[q].transform.SetParent(transform);
        }
        message = "Skeleton created";
    }

    void Update()
    {
        if (NuitrackManager.Users.Current != null && NuitrackManager.Users.Current.Skeleton != null)
        {
            message = "Skeleton found";

            for (int q = 0; q < typeJoint.Length; q++)
            {
                UserData.SkeletonData.Joint joint = NuitrackManager.Users.Current.Skeleton.GetJoint(typeJoint[q]);
                Vector3 jointPos = joint.Position;
                jointPos.x *= -1;
                CreatedJoint[q].transform.localPosition = jointPos;
            }
        }
        else
        {
            message = "Skeleton not found";
        }
    }

        // Display the message on the screen
    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.skin.label.fontSize = 50;
        GUILayout.Label(message);
    }
}