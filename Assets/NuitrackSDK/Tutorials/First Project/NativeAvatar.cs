#region Description

// The script performs a direct translation of the skeleton using only the position data of the joints.
// Objects in the skeleton will be created when the scene starts.

#endregion


using UnityEngine;

namespace NuitrackSDK.Tutorials.FirstProject
{
    [AddComponentMenu("NuitrackSDK/Tutorials/First Project/Native Avatar")]
    public class NativeAvatar : MonoBehaviour
    {
        string message = "";

        public nuitrack.JointType[] typeJoint;
        GameObject[] CreatedJoint;
        public GameObject PrefabJoint;
        public GameObject LeftHandJoint;

        void Start()
        {
            CreatedJoint = new GameObject[typeJoint.Length];
            for (int q = 0; q < typeJoint.Length; q++)
            {
                if(typeJoint[q] == nuitrack.JointType.LeftHand)
                    CreatedJoint[q] = Instantiate(LeftHandJoint);
                else
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
                    CreatedJoint[q].transform.localPosition = joint.Position;
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
}