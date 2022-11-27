using UnityEngine;
using NuitrackSDK.Calibration;


namespace NuitrackSDK.Tutorials.ZombieVR
{
    [AddComponentMenu("NuitrackSDK/Tutorials/ZombieVR/Nuitrack Legs")]
    public class NuitrackLegs : TrackedUser
    {
        [SerializeField] Transform VRCamera;
        [SerializeField] Transform headBase;
        [SerializeField] Rigidbody leftLeg, rightLeg, body;
        Vector3 offset;
        Quaternion q180 = Quaternion.Euler(0f, 180f, 0f); // mirror the joint position

        void OnEnable()
        {
            if (CalibrationHandler.Instance != null)
                CalibrationHandler.Instance.onSuccess += OnSuccessCalib;
        }

        void OnSuccessCalib(Quaternion rotation)
        {
            headBase.eulerAngles = new Vector3(0, headBase.eulerAngles.y - VRCamera.eulerAngles.y, 0);
            Vector3 newPos = headBase.position - VRCamera.position;
            headBase.position = new Vector3(newPos.x, headBase.position.y, newPos.z);
        }

        void OnDisable()
        {
            if (CalibrationHandler.Instance != null)
                CalibrationHandler.Instance.onSuccess -= OnSuccessCalib;
        }

        Vector3 GetPos(nuitrack.JointType jointType)
        {
            return q180 * (Vector3.up * CalibrationInfo.FloorHeight + CalibrationInfo.SensorOrientation * ControllerUser.Skeleton.GetJoint(jointType).Position);
        }

        void FixedUpdate()
        {
            if (ControllerUser != null && ControllerUser.Skeleton != null)
            {
                offset = GetPos(nuitrack.JointType.Head) - VRCamera.position;
                leftLeg.MovePosition(GetPos(nuitrack.JointType.LeftAnkle) - offset);
                rightLeg.MovePosition(GetPos(nuitrack.JointType.RightAnkle) - offset);
                body.MovePosition(GetPos(nuitrack.JointType.Waist) - offset);
            }
        }
    }
}