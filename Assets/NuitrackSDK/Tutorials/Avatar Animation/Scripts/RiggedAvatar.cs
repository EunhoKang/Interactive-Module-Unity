using UnityEngine;
using System.Collections.Generic;
using NuitrackSDK.Calibration;


namespace NuitrackSDK.Tutorials.AvatarAnimation
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Avatar Animation/Rigged Avatar")]
    public class RiggedAvatar : MonoBehaviour
    {
        [Header("Rigged model")]
        [SerializeField]
        ModelJoint[] modelJoints;
        [SerializeField]
        nuitrack.JointType rootJoint = nuitrack.JointType.LeftCollar;
        /// <summary> Model bones </summary>
        Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = new Dictionary<nuitrack.JointType, ModelJoint>();

        void Start()
        {
            for (int i = 0; i < modelJoints.Length; i++)
            {
                modelJoints[i].baseRotOffset = modelJoints[i].bone.rotation;
                jointsRigged.Add(modelJoints[i].jointType.TryGetMirrored(), modelJoints[i]);
            }
        }

        void Update()
        {
            if (NuitrackManager.Users.Current != null && NuitrackManager.Users.Current.Skeleton != null)
                ProcessSkeleton(NuitrackManager.Users.Current.Skeleton);
        }

        void ProcessSkeleton(UserData.SkeletonData skeleton)
        {
            //Calculate the model position: take the root position and invert movement along the Z axis
            Vector3 rootPos = Quaternion.Euler(0f, 180f, 0f) * skeleton.GetJoint(rootJoint).Position;
            transform.position = rootPos;

            foreach (var riggedJoint in jointsRigged)
            {
                //Get joint from the Nuitrack
                UserData.SkeletonData.Joint joint = skeleton.GetJoint(riggedJoint.Key);

                ModelJoint modelJoint = riggedJoint.Value;

                //Calculate the model bone rotation: take the mirrored joint orientation, add a basic rotation of the model bone, invert movement along the Z axis
                Quaternion jointOrient = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * joint.RotationMirrored * modelJoint.baseRotOffset;

                modelJoint.bone.rotation = jointOrient;
            }
        }
    }
}