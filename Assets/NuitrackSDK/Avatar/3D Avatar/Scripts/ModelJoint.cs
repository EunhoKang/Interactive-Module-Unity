using UnityEngine;


namespace NuitrackSDK
{
    [System.Serializable]
    public class ModelJoint
    {
        /// <summary> Transform model bone </summary>
        public Transform bone = null;
        public nuitrack.JointType jointType = nuitrack.JointType.None;

        //For "Direct translation"
        public nuitrack.JointType parentJointType = nuitrack.JointType.None;
        /// <summary> Base model bones rotation offsets</summary>

        public Quaternion baseRotOffset
        {
            get;
            set;
        }

        public Transform parentBone
        {
            get;
            set;
        }

        // <summary> Base distance to parent bone </summary>
        public float baseDistanceToParent
        {
            get;
            set;
        }
    }
}