using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JointType = nuitrack.JointType;

using NuitrackSDK.Calibration;


namespace NuitrackSDK.Avatar
{
    [AddComponentMenu("NuitrackSDK/Avatar/3D/Skeleton Avatar")]
    public class SkeletonAvatar : BaseAvatar
    {
        [SerializeField] GameObject jointPrefab = null, connectionPrefab = null;
        [SerializeField] Transform headTransform; //if not null, skeletonAvatar will move it
        [SerializeField] Transform headDirectionTransform; //part of head preab that rotates 
        [SerializeField] bool rotate180 = true;
        [SerializeField] Vector3 neckHMDOffset = new Vector3(0f, 0.15f, 0.08f);
        [SerializeField] Vector3 startPoint;
        Vector3 basePivotOffset;
        Vector3 basePivot;
        public static Vector3 leftHandPos, rightHandPos;

        JointType[] jointsInfo = new JointType[]
        {
        JointType.Head,
        JointType.Neck,
        JointType.LeftCollar,
        JointType.RightCollar,
        JointType.Torso,
        JointType.Waist,
        JointType.LeftShoulder,
        JointType.RightShoulder,
        JointType.LeftElbow,
        JointType.RightElbow,
        JointType.LeftWrist,
        JointType.RightWrist,
        JointType.LeftHand,
        JointType.RightHand,
        JointType.LeftHip,
        JointType.RightHip,
        JointType.LeftKnee,
        JointType.RightKnee,
        JointType.LeftAnkle,
        JointType.RightAnkle
        };

        GameObject skeletonRoot;
        GameObject[] connections;
        Dictionary<JointType, GameObject> joints;
        Quaternion q180 = Quaternion.Euler(0f, 180f, 0f);
        Quaternion q0 = Quaternion.identity;

        void Start()
        {
            CreateSkeletonParts();
        }

        void CreateSkeletonParts()
        {
            skeletonRoot = new GameObject();
            skeletonRoot.name = "SkeletonRoot";

            joints = new Dictionary<JointType, GameObject>();

            for (int i = 0; i < jointsInfo.Length; i++)
            {
                if (jointPrefab != null)
                {
                    GameObject joint = (GameObject)Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
                    joints.Add(jointsInfo[i], joint);
                    joint.transform.parent = skeletonRoot.transform;
                    joint.SetActive(false);
                }
            }

            //connections = new GameObject[connectionsInfo.GetLength(0)];
            connections = new GameObject[jointsInfo.Length];

            for (int i = 0; i < connections.Length; i++)
            {
                if (connectionPrefab != null)
                {
                    GameObject conn = (GameObject)Instantiate(connectionPrefab, Vector3.zero, Quaternion.identity);
                    connections[i] = conn;
                    conn.transform.parent = skeletonRoot.transform;
                    conn.SetActive(false);
                }
            }
        }

        void DeleteSkeletonParts()
        {
            Destroy(skeletonRoot);
            joints = null;
            connections = null;
        }

        void Update()
        {
            UserData user = ControllerUser;

            if (user == null || user.Skeleton == null)
                return;

            if (headTransform != null)
            {
#if UNITY_IOS
			headTransform.position = headDirectionTransform.rotation * neckHMDOffset + (rotate180 ? q180 : q0) * (Vector3.up * CalibrationInfo.FloorHeight + CalibrationInfo.SensorOrientation * GetJoint(JointType.Neck).Position) + basePivotOffset;
#else
                headTransform.position = (rotate180 ? q180 : q0) * (Vector3.up * CalibrationInfo.FloorHeight + CalibrationInfo.SensorOrientation * GetJoint(JointType.Head).Position) + basePivotOffset;
#endif

                basePivot = (rotate180 ? q180 : q0) * (Vector3.up * CalibrationInfo.FloorHeight + CalibrationInfo.SensorOrientation * GetJoint(JointType.Waist).Position) + basePivotOffset;
            }

            if (!skeletonRoot.activeSelf) skeletonRoot.SetActive(true);

            for (int i = 0; i < jointsInfo.Length; i++)
            {
                UserData.SkeletonData.Joint j = user.Skeleton.GetJoint(jointsInfo[i]);

                if (j.Confidence > JointConfidence)
                {
                    if (!joints[jointsInfo[i]].activeSelf) joints[jointsInfo[i]].SetActive(true);

                    joints[jointsInfo[i]].transform.position = (rotate180 ? q180 : q0) * (Vector3.up * CalibrationInfo.FloorHeight + CalibrationInfo.SensorOrientation * j.Position) + basePivotOffset;
                    joints[jointsInfo[i]].transform.rotation = (rotate180 ? q180 : q0) * CalibrationInfo.SensorOrientation * j.RotationMirrored;

                    leftHandPos = (rotate180 ? q180 : q0) * (Vector3.up * CalibrationInfo.FloorHeight + CalibrationInfo.SensorOrientation * GetJoint(JointType.LeftHand).Position) + basePivotOffset;
                    rightHandPos = (rotate180 ? q180 : q0) * (Vector3.up * CalibrationInfo.FloorHeight + CalibrationInfo.SensorOrientation * GetJoint(JointType.RightHand).Position) + basePivotOffset;
                }
                else
                {
                    if (joints[jointsInfo[i]].activeSelf) joints[jointsInfo[i]].SetActive(false);
                }
            }

            for (int i = 0; i < jointsInfo.Length; i++)
            {
                JointType jointType = jointsInfo[i];
                JointType parentType = jointsInfo[i].GetParent();
                if (parentType != JointType.None)
                {
                    if (joints[jointType].activeSelf && joints[parentType].activeSelf)
                    {
                        if (!connections[i].activeSelf) connections[i].SetActive(true);

                        Vector3 diff = joints[parentType].transform.position - joints[jointType].transform.position;

                        connections[i].transform.position = joints[jointType].transform.position;
                        connections[i].transform.rotation = Quaternion.LookRotation(diff);
                        connections[i].transform.localScale = new Vector3(1f, 1f, diff.magnitude);
                    }
                    else
                    {
                        if (connections[i].activeSelf) connections[i].SetActive(false);
                    }
                }
            }
        }

        void OnEnable()
        {
            if(CalibrationHandler.Instance != null)
                CalibrationHandler.Instance.onSuccess += OnSuccessCalib;
        }

        void OnDisable()
        {
            if (CalibrationHandler.Instance != null)
                CalibrationHandler.Instance.onSuccess -= OnSuccessCalib;
        }

        private void OnSuccessCalib(Quaternion rotation)
        {
            StartCoroutine(CalculateOffset());
        }

        public IEnumerator CalculateOffset()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            basePivotOffset = startPoint - basePivot + basePivotOffset;
            basePivotOffset.x = 0;
        }

        void OnDestroy()
        {
            DeleteSkeletonParts();
        }
    }
}