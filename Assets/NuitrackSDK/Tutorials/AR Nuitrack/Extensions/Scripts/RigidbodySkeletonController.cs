using UnityEngine;
using System.Collections.Generic;

using NuitrackSDK.Avatar;


namespace NuitrackSDK.Tutorials.ARNuitrack.Extensions
{
    [AddComponentMenu("NuitrackSDK/Tutorials/AR Nuitrack/Extensions/Rigidbody Skeleton Controller")]
    public class RigidbodySkeletonController : BaseAvatar
    {
        [Header("Rigidbody")]
        [SerializeField] List<nuitrack.JointType> targetJoints;
        [SerializeField] GameObject rigidBodyJoint;

        [SerializeField, Range(0.1f, 64f)] float smoothSpeed = 24f;

        [SerializeField] Transform space;

        Dictionary<nuitrack.JointType, Rigidbody> rigidbodyObj;

        public void SetSpace(Transform newSpace)
        {
            space = newSpace;
        }

        void Awake()
        {
            rigidbodyObj = new Dictionary<nuitrack.JointType, Rigidbody>();

            foreach (nuitrack.JointType jointType in targetJoints)
            {
                GameObject jointObj = Instantiate(rigidBodyJoint, transform);
                jointObj.name = string.Format("{0}_rigidbody", jointType.ToString());

                Rigidbody rigidbody = jointObj.GetComponent<Rigidbody>();
                rigidbodyObj.Add(jointType, rigidbody);
            }
        }

        void FixedUpdate()
        {
            if (ControllerUser != null && ControllerUser.Skeleton != null)
            {
                foreach (KeyValuePair<nuitrack.JointType, Rigidbody> rigidbodyJoint in rigidbodyObj)
                {
                    Vector3 newPosition = ControllerUser.Skeleton.GetJoint(rigidbodyJoint.Key).Position;

                    Vector3 spacePostion = space == null ? newPosition : space.TransformPoint(newPosition);
                    Vector3 lerpPosition = Vector3.Lerp(rigidbodyJoint.Value.position, spacePostion, Time.deltaTime * smoothSpeed);

                    rigidbodyJoint.Value.MovePosition(lerpPosition);
                }
            }
        }
    }
}