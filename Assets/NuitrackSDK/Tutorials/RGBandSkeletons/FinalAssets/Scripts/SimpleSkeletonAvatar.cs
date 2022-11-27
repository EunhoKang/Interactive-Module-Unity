using System.Collections.Generic;
using UnityEngine;


namespace NuitrackSDK.Tutorials.RGBandSkeletons
{
    [AddComponentMenu("NuitrackSDK/Tutorials/RGB and Skeletons/Simple Skeleton Avatar")]
    public class SimpleSkeletonAvatar : MonoBehaviour
    {
        public bool autoProcessing = true;
        [SerializeField] GameObject jointPrefab = null, connectionPrefab = null;
        RectTransform parentRect;

        nuitrack.JointType[] jointsInfo = new nuitrack.JointType[]
        {
        nuitrack.JointType.Head,
        nuitrack.JointType.Neck,
        nuitrack.JointType.LeftCollar,
        nuitrack.JointType.Torso,
        nuitrack.JointType.Waist,
        nuitrack.JointType.LeftShoulder,
        nuitrack.JointType.RightShoulder,
        nuitrack.JointType.LeftElbow,
        nuitrack.JointType.RightElbow,
        nuitrack.JointType.LeftWrist,
        nuitrack.JointType.RightWrist,
        nuitrack.JointType.LeftHand,
        nuitrack.JointType.RightHand,
        nuitrack.JointType.LeftHip,
        nuitrack.JointType.RightHip,
        nuitrack.JointType.LeftKnee,
        nuitrack.JointType.RightKnee,
        nuitrack.JointType.LeftAnkle,
        nuitrack.JointType.RightAnkle
        };

        nuitrack.JointType[,] connectionsInfo = new nuitrack.JointType[,]
        { //Right and left collars are currently located at the same point, that's why we use only 1 collar,
        //it's easy to add rightCollar, if it ever changes
        {nuitrack.JointType.Neck,           nuitrack.JointType.Head},
        {nuitrack.JointType.LeftCollar,     nuitrack.JointType.Neck},
        {nuitrack.JointType.LeftCollar,     nuitrack.JointType.LeftShoulder},
        {nuitrack.JointType.LeftCollar,     nuitrack.JointType.RightShoulder},
        {nuitrack.JointType.LeftCollar,     nuitrack.JointType.Torso},
        {nuitrack.JointType.Waist,          nuitrack.JointType.Torso},
        {nuitrack.JointType.Waist,          nuitrack.JointType.LeftHip},
        {nuitrack.JointType.Waist,          nuitrack.JointType.RightHip},
        {nuitrack.JointType.LeftShoulder,   nuitrack.JointType.LeftElbow},
        {nuitrack.JointType.LeftElbow,      nuitrack.JointType.LeftWrist},
        {nuitrack.JointType.LeftWrist,      nuitrack.JointType.LeftHand},
        {nuitrack.JointType.RightShoulder,  nuitrack.JointType.RightElbow},
        {nuitrack.JointType.RightElbow,     nuitrack.JointType.RightWrist},
        {nuitrack.JointType.RightWrist,     nuitrack.JointType.RightHand},
        {nuitrack.JointType.LeftHip,        nuitrack.JointType.LeftKnee},
        {nuitrack.JointType.LeftKnee,       nuitrack.JointType.LeftAnkle},
        {nuitrack.JointType.RightHip,       nuitrack.JointType.RightKnee},
        {nuitrack.JointType.RightKnee,      nuitrack.JointType.RightAnkle}
        };

        List<RectTransform> connections;
        Dictionary<nuitrack.JointType, RectTransform> joints;

        void Start()
        {
            CreateSkeletonParts();
            parentRect = GetComponent<RectTransform>();
        }

        void CreateSkeletonParts()
        {
            joints = new Dictionary<nuitrack.JointType, RectTransform>();

            for (int i = 0; i < jointsInfo.Length; i++)
            {
                if (jointPrefab != null)
                {
                    GameObject joint = Instantiate(jointPrefab, transform);
                    joint.SetActive(false);

                    RectTransform jointRectTransform = joint.GetComponent<RectTransform>();
                    joints.Add(jointsInfo[i], jointRectTransform);
                }
            }

            connections = new List<RectTransform>();

            for (int i = 0; i < connectionsInfo.GetLength(0); i++)
            {
                if (connectionPrefab != null)
                {
                    GameObject connection = Instantiate(connectionPrefab, transform);
                    connection.SetActive(false);

                    RectTransform connectionRectTransform = connection.GetComponent<RectTransform>();
                    connections.Add(connectionRectTransform);
                }
            }
        }

        void Update()
        {
            if (autoProcessing)
                ProcessSkeleton(NuitrackManager.Users.Current);
        }

        public void ProcessSkeleton(UserData user)
        {
            if (user == null || user.Skeleton == null)
                return;

            for (int i = 0; i < jointsInfo.Length; i++)
            {
                UserData.SkeletonData.Joint j = user.Skeleton.GetJoint(jointsInfo[i]);

                if (j.Confidence > 0.01f)
                {
                    joints[jointsInfo[i]].gameObject.SetActive(true);
                    joints[jointsInfo[i]].anchoredPosition = j.AnchoredPosition(parentRect.rect, joints[jointsInfo[i]]);
                }
                else
                {
                    joints[jointsInfo[i]].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < connectionsInfo.GetLength(0); i++)
            {
                RectTransform startJoint = joints[connectionsInfo[i, 0]];
                RectTransform endJoint = joints[connectionsInfo[i, 1]];

                if (startJoint.gameObject.activeSelf && endJoint.gameObject.activeSelf)
                {
                    connections[i].gameObject.SetActive(true);

                    connections[i].anchoredPosition = startJoint.anchoredPosition;
                    connections[i].transform.right = endJoint.position - startJoint.position;
                    float distance = Vector3.Distance(endJoint.anchoredPosition, startJoint.anchoredPosition);
                    connections[i].transform.localScale = new Vector3(distance, 1f, 1f);
                }
                else
                {
                    connections[i].gameObject.SetActive(false);
                }
            }
        }
    }
}