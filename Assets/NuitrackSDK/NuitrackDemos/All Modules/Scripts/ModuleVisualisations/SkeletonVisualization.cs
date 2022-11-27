using UnityEngine;
using System.Collections.Generic;

namespace NuitrackSDK.NuitrackDemos
{
    // TODO switch to Skeleton prefab
    public class SkeletonVisualization : MonoBehaviour
    {
        [SerializeField] GameObject jointPrefab = null, connectionPrefab = null;

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
        {
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

        Dictionary<int, Dictionary<nuitrack.JointType, GameObject>> joints = new Dictionary<int, Dictionary<nuitrack.JointType, GameObject>>();
        Dictionary<int, GameObject[]> connections = new Dictionary<int, GameObject[]>();
        Dictionary<int, GameObject> skeletonsRoots = new Dictionary<int, GameObject>();

        IssuesProcessor issuesProcessor;

        void Start()
        {
            issuesProcessor = IssuesProcessor.Instance;
        }

        void Update()
        {
            ProcessSkeletons();
        }

        void HideAllSkeletons()
        {

            int[] skelIds = new int[skeletonsRoots.Keys.Count];
            skeletonsRoots.Keys.CopyTo(skelIds, 0);

            for (int i = 0; i < skelIds.Length; i++)
            {
                skeletonsRoots[skelIds[i]].SetActive(false);
            }
        }

        void ProcessSkeletons()
        {
            if (NuitrackManager.Users.Count == 0)
            {
                HideAllSkeletons();
                return;
            }
            //Debug.Log("NumUsers: " + skeletonData.NumUsers.ToString());

            int[] skelIds = new int[skeletonsRoots.Keys.Count];
            skeletonsRoots.Keys.CopyTo(skelIds, 0);

            for (int i = 0; i < skelIds.Length; i++)
            {
                UserData user = NuitrackManager.Users.GetUser(skelIds[i]);
                if (user == null || user.Skeleton == null)
                {
                    skeletonsRoots[skelIds[i]].SetActive(false);
                }
            }

            foreach (UserData user in NuitrackManager.Users)
            {
                if (user.Skeleton == null)
                    continue;

                if (!skeletonsRoots.ContainsKey(user.ID)) // if don't have gameObjects for skeleton ID, create skeleton gameobjects (root, joints and connections)
                {
                    GameObject skelRoot = new GameObject();
                    skelRoot.name = "Root_" + user.ID.ToString();

                    skeletonsRoots.Add(user.ID, skelRoot);

                    Dictionary<nuitrack.JointType, GameObject> skelJoints = new Dictionary<nuitrack.JointType, GameObject>();


                    for (int i = 0; i < jointsInfo.Length; i++)
                    {
                        GameObject joint = (GameObject)Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
                        skelJoints.Add(jointsInfo[i], joint);
                        joint.transform.parent = skelRoot.transform;
                        joint.SetActive(false);
                    }

                    joints.Add(user.ID, skelJoints);

                    GameObject[] skelConnections = new GameObject[connectionsInfo.GetLength(0)];

                    for (int i = 0; i < skelConnections.Length; i++)
                    {
                        GameObject conn = (GameObject)Instantiate(connectionPrefab, Vector3.zero, Quaternion.identity);
                        skelConnections[i] = conn;
                        conn.transform.parent = skelRoot.transform;
                        conn.SetActive(false);
                    }

                    connections.Add(user.ID, skelConnections);
                }

                if (!skeletonsRoots[user.ID].activeSelf) 
                    skeletonsRoots[user.ID].SetActive(true);

                for (int i = 0; i < jointsInfo.Length; i++)
                {
                    UserData.SkeletonData.Joint j = user.Skeleton.GetJoint(jointsInfo[i]);
                    if (j.Confidence > 0.01f)
                    {
                        if (!joints[user.ID][jointsInfo[i]].activeSelf) 
                            joints[user.ID][jointsInfo[i]].SetActive(true);

                        joints[user.ID][jointsInfo[i]].transform.position = j.Position;

                        //skel.Joints[i].Orient.Matrix:
                        // 0,       1,      2, 
                        // 3,       4,      5,
                        // 6,       7,      8
                        // -------
                        // right(X),  up(Y),    forward(Z)

                        joints[user.ID][jointsInfo[i]].transform.rotation = j.Rotation;
                    }
                    else
                    {
                        if (joints[user.ID][jointsInfo[i]].activeSelf)
                            joints[user.ID][jointsInfo[i]].SetActive(false);
                    }
                }

                for (int i = 0; i < connectionsInfo.GetLength(0); i++)
                {
                    if (joints[user.ID][connectionsInfo[i, 0]].activeSelf && joints[user.ID][connectionsInfo[i, 1]].activeSelf)
                    {
                        if (!connections[user.ID][i].activeSelf) connections[user.ID][i].SetActive(true);

                        Vector3 diff = joints[user.ID][connectionsInfo[i, 1]].transform.position - joints[user.ID][connectionsInfo[i, 0]].transform.position;

                        connections[user.ID][i].transform.position = joints[user.ID][connectionsInfo[i, 0]].transform.position;
                        connections[user.ID][i].transform.rotation = Quaternion.LookRotation(diff);
                        connections[user.ID][i].transform.localScale = new Vector3(1f, 1f, diff.magnitude);
                    }
                    else
                    {
                        if (connections[user.ID][i].activeSelf) 
                            connections[user.ID][i].SetActive(false);
                    }
                }
            }
        }

        void OnDestroy()
        {
            int[] idsInDict = new int[skeletonsRoots.Count];
            skeletonsRoots.Keys.CopyTo(idsInDict, 0);

            for (int i = 0; i < idsInDict.Length; i++)
            {
                Destroy(skeletonsRoots[idsInDict[i]]);
            }

            skeletonsRoots = new Dictionary<int, GameObject>();
            joints = new Dictionary<int, Dictionary<nuitrack.JointType, GameObject>>();
            connections = new Dictionary<int, GameObject[]>();

            if (issuesProcessor != null) 
                Destroy(issuesProcessor.gameObject);
        }
    }
}