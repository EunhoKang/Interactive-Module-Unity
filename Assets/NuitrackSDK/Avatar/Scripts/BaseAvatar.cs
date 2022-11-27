using UnityEngine;


namespace NuitrackSDK.Avatar
{
    /// <summary>
    /// The base class of the avatar. Use it to create your own avatars.
    /// </summary>
    public abstract class BaseAvatar : TrackedUser
    {
        [SerializeField, Range(0, 1), NuitrackSDKInspector]
        float jointConfidence = 0.1f;

        /// <summary>
        /// Confidence threshold for detected joints
        /// </summary>
        public float JointConfidence
        {
            get
            {
                return jointConfidence;
            }
            set
            {
                jointConfidence = value;
            }
        }

        /// <summary>
        /// Get a shell object for the specified joint
        /// </summary>
        /// <param name="jointType">Joint type</param>
        /// <returns>Shell object <see cref="UserData.SkeletonData.Joint"/></returns>
        public UserData.SkeletonData.Joint GetJoint(nuitrack.JointType jointType)
        {
            if (!IsActive || ControllerUser.Skeleton == null)
                return null;

            return ControllerUser.Skeleton.GetJoint(jointType);
        }
    }
}