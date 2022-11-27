using UnityEngine;

using System.Collections;
using System.Collections.Generic;


namespace NuitrackSDK.Poses
{
    [System.Serializable, CreateAssetMenuAttribute(fileName = "Pose", menuName = "Nuitrack/Poses/Pose")]
    public class NuitrackPose : ScriptableObject, IEnumerable<nuitrack.JointType>
    {
        static Quaternion MirrorSensorPlane(Quaternion sourceRotation)
        {
            sourceRotation.z *= -1;
            sourceRotation.w *= -1;

            return sourceRotation;
        }

        [System.Serializable]
        public class JointReference
        {
            [SerializeField] bool isActive = true;

            [SerializeField] Quaternion orientation;
            [SerializeField, Range(0.1f, 0.99f)] float tolerance = 0.9f;

            public bool IsActive
            {
                get
                {
                    return isActive;
                }
            }

            public Quaternion Orientation
            {
                get
                {
                    return orientation;
                }
            }

            public float Tolerance
            {
                get
                {
                    return tolerance;
                }
            }

            public JointReference()
            {
                tolerance = 0.9f;
                isActive = true;
                orientation = Quaternion.Euler(0, 0, 0);
            }

            public JointReference(UserData.SkeletonData.Joint joint, float tolerance = 0.9f)
            {
                Quaternion rotation = NuitrackManager.DepthSensor.IsMirror() ? MirrorSensorPlane(joint.Rotation) : joint.RotationMirrored;

                orientation = rotation;
                this.tolerance = tolerance;
            }

            public JointReference(JointReference sourceJointReference)
            {
                isActive = sourceJointReference.IsActive;
                orientation = sourceJointReference.Orientation;
                tolerance = sourceJointReference.Tolerance;
            }


            /// <summary>
            /// Absolute match of the joint orientation to the sample
            /// </summary>
            /// <param name="joint">Source Joint</param>
            /// <returns>
            /// 0 - joint orientation does not match the sample,
            /// 1 - joint orientation match to the sample with specified tolerance
            /// </returns>
            public float Match(UserData.SkeletonData.Joint joint)
            {
                if (isActive && NuitrackManager.DepthSensor != null)
                {
                    Quaternion rotation = NuitrackManager.DepthSensor.IsMirror() ? MirrorSensorPlane(joint.Rotation) : joint.RotationMirrored;

                    float dot = Vector3.Dot(orientation * joint.NuitrackType.GetNormalDirection(), rotation * joint.NuitrackType.GetNormalDirection());
                    return Mathf.Clamp01(dot / tolerance);
                }
                else
                    return 0;
            }

            /// <summary>
            /// Relative match of the joint orientation to the sample
            /// </summary>
            /// <param name="joint">Source Joint</param>
            /// <param name="skeletonInverseRotation"></param>
            /// <param name="poseInverseRotation"></param>
            /// <returns>
            /// 0 - joint orientation does not match the sample,
            /// 1 - joint orientation match to the sample with specified tolerance
            /// </returns>
            public float Match(UserData.SkeletonData.Joint joint, Quaternion skeletonInverseRotation, Quaternion poseInverseRotation)
            {
                if (isActive)
                {
                    Quaternion rotation = NuitrackManager.DepthSensor.IsMirror() ? MirrorSensorPlane(joint.Rotation) : joint.RotationMirrored;

                    Quaternion relativeSkeletonJointRotation = poseInverseRotation * orientation;
                    Quaternion relativeReferenceJointRotation = skeletonInverseRotation * rotation;

                    float dot = Vector3.Dot(relativeSkeletonJointRotation * joint.NuitrackType.GetNormalDirection(), relativeReferenceJointRotation * joint.NuitrackType.GetNormalDirection());
                    return Mathf.Clamp01(dot / tolerance);
                }
                else
                    return 0;
            }
        }

        static readonly List<nuitrack.JointType> jointsMask = new List<nuitrack.JointType>()
        {
            nuitrack.JointType.Waist,

            nuitrack.JointType.LeftHip,
            nuitrack.JointType.LeftKnee,

            nuitrack.JointType.RightHip,
            nuitrack.JointType.RightKnee,

            nuitrack.JointType.LeftShoulder,
            nuitrack.JointType.LeftElbow,

            nuitrack.JointType.RightShoulder,
            nuitrack.JointType.RightElbow
        };

        [SerializeField, NuitrackSDKInspector] bool relativeMode = true;

        [SerializeField, NuitrackSDKInspector] JointReference waist;

        [SerializeField, NuitrackSDKInspector] JointReference leftHip;
        [SerializeField, NuitrackSDKInspector] JointReference leftKnee;

        [SerializeField, NuitrackSDKInspector] JointReference rightHip;
        [SerializeField, NuitrackSDKInspector] JointReference rightKnee;

        [SerializeField, NuitrackSDKInspector] JointReference leftShoulder;
        [SerializeField, NuitrackSDKInspector] JointReference leftElbow;

        [SerializeField, NuitrackSDKInspector] JointReference rightShoulder;
        [SerializeField, NuitrackSDKInspector] JointReference rightElbow;

        public bool RelativeMode
        {
            get
            {
                return relativeMode;
            }
        }

        public JointReference this[nuitrack.JointType jointType]
        {
            get
            {
                return jointType switch
                {
                    nuitrack.JointType.Waist => waist,
                    nuitrack.JointType.LeftHip => leftHip,
                    nuitrack.JointType.LeftKnee => leftKnee,
                    nuitrack.JointType.RightHip => rightHip,
                    nuitrack.JointType.RightKnee => rightKnee,
                    nuitrack.JointType.LeftShoulder => leftShoulder,
                    nuitrack.JointType.LeftElbow => leftElbow,
                    nuitrack.JointType.RightShoulder => rightShoulder,
                    nuitrack.JointType.RightElbow => rightElbow,
                    _ => null,
                };
            }
        }

        /// <summary>
        /// Match of the skeleton pose with the sample of pose
        /// </summary>
        /// <param name="skeleton">Source Skeleton</param>
        /// <returns>
        /// 0 - pose of skeleton does not match the sample,
        /// 1 - pose of skeleton match to the sample with specified tolerance
        /// </returns>
        public float Match(UserData.SkeletonData skeleton)
        {
            if (relativeMode)
                return RelativeMatch(skeleton);
            else
                return AbsoluteMatch(skeleton);
        }

        /// <summary>
        /// Absolute match with the pose (the torso rotation is taken into account)
        /// </summary>
        /// <param name="skeleton">Source Skeleton</param>
        /// <returns>
        /// 0 - pose of skeleton does not match the sample,
        /// 1 - pose of skeleton match to the sample with specified tolerance
        /// </returns>
        public float AbsoluteMatch(UserData.SkeletonData skeleton)
        {
            float totalWeight = 0;
            float currentWeight = 0;

            foreach (nuitrack.JointType jointDescription in this)
            {
                JointReference jointReference = this[jointDescription];
                UserData.SkeletonData.Joint joint = skeleton.GetJoint(jointDescription);

                totalWeight += jointReference.IsActive ? 1 : 0;
                currentWeight += jointReference.Match(joint);
            }

            return currentWeight / totalWeight;
        }

        /// <summary>
        /// Relative match with the pose (torso rotation is not taken into account)
        /// </summary>
        /// <param name="skeleton">Source Skeleton</param>
        /// <returns>
        /// 0 - pose of skeleton does not match the sample,
        /// 1 - pose of skeleton match to the sample with specified tolerance
        /// </returns>
        public float RelativeMatch(UserData.SkeletonData skeleton)
        {
            if (NuitrackManager.DepthSensor == null)
                return 0;

            float totalWeight = 0;
            float currentWeight = 0;

            UserData.SkeletonData.Joint waistJoint = skeleton.GetJoint(nuitrack.JointType.Waist);
            
            Quaternion waistRotation = NuitrackManager.DepthSensor.IsMirror() ? MirrorSensorPlane(waistJoint.Rotation) : waistJoint.RotationMirrored;

            Quaternion skeletonInverseRotation = Quaternion.Inverse(waistRotation);
            Quaternion poseInverseRotation = Quaternion.Inverse(waist.Orientation);

            foreach (nuitrack.JointType jointDescription in this)
            {
                if (jointDescription != nuitrack.JointType.Waist)
                {
                    JointReference jointReference = this[jointDescription];
                    UserData.SkeletonData.Joint joint = skeleton.GetJoint(jointDescription);

                    totalWeight += jointReference.IsActive ? 1 : 0;
                    currentWeight += jointReference.Match(joint, skeletonInverseRotation, poseInverseRotation);
                }
            }

            return currentWeight / totalWeight;
        }

        public IEnumerator<nuitrack.JointType> GetEnumerator()
        {
            return jointsMask.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Create Pose from Skeleton reference
        /// <see cref="UserData.SkeletonData"/>
        /// </summary>
        /// <param name="name">Pose name</param>
        /// <param name="skeleton">Source skeleton</param>
        public NuitrackPose(string name, UserData.SkeletonData skeleton, bool relativeMode = true)
        {
            this.name = name;
            this.relativeMode = relativeMode;

            waist = new JointReference(skeleton.GetJoint(nuitrack.JointType.Waist));

            leftHip = new JointReference(skeleton.GetJoint(nuitrack.JointType.LeftHip));
            leftKnee = new JointReference(skeleton.GetJoint(nuitrack.JointType.LeftKnee));

            rightHip = new JointReference(skeleton.GetJoint(nuitrack.JointType.RightHip));
            rightKnee = new JointReference(skeleton.GetJoint(nuitrack.JointType.RightKnee));

            leftShoulder = new JointReference(skeleton.GetJoint(nuitrack.JointType.LeftShoulder));
            leftElbow = new JointReference(skeleton.GetJoint(nuitrack.JointType.LeftElbow));

            rightShoulder = new JointReference(skeleton.GetJoint(nuitrack.JointType.RightShoulder));
            rightElbow = new JointReference(skeleton.GetJoint(nuitrack.JointType.RightElbow));
        }

        /// <summary>
        /// Create Pose from Pose reference
        /// </summary>
        /// <param name="name">Pose name</param>
        /// <param name="sourcePose">Source pose</param>
        public NuitrackPose(string name, NuitrackPose sourcePose)
        {
            this.name = name;
            relativeMode = sourcePose.RelativeMode;

            waist = new JointReference(sourcePose[nuitrack.JointType.Waist]);

            leftHip = new JointReference(sourcePose[nuitrack.JointType.LeftHip]);
            leftKnee = new JointReference(sourcePose[nuitrack.JointType.LeftKnee]);

            rightHip = new JointReference(sourcePose[nuitrack.JointType.RightHip]);
            rightKnee = new JointReference(sourcePose[nuitrack.JointType.RightKnee]);

            leftShoulder = new JointReference(sourcePose[nuitrack.JointType.LeftShoulder]);
            leftElbow = new JointReference(sourcePose[nuitrack.JointType.LeftElbow]);

            rightShoulder = new JointReference(sourcePose[nuitrack.JointType.RightShoulder]);
            rightElbow = new JointReference(sourcePose[nuitrack.JointType.RightElbow]);
        }

        /// <summary>
        /// Create a pose (default T-pose)
        /// </summary>
        public NuitrackPose()
        {
            relativeMode = true;

            waist = new JointReference();

            leftHip = new JointReference();
            leftKnee = new JointReference();

            rightHip = new JointReference();
            rightKnee = new JointReference();

            leftShoulder = new JointReference();
            leftElbow = new JointReference();

            rightShoulder = new JointReference();
            rightElbow = new JointReference();
        }
    }
}
