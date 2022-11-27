using UnityEngine;

namespace NuitrackSDK.Pointer
{
    public class PointerRotation : MonoBehaviour
    {
        public int hand;
        public Transform target;

        public float speed = 0.1F;

        // Update is called once per frame
        void LateUpdate()
        {
            if (NuitrackManager.Users.Current == null && NuitrackManager.Users.Current.Skeleton == null)
                return;

            hand = PointerPassing.hand;

            UserData.SkeletonData skeleton = NuitrackManager.Users.Current.Skeleton;

            if (hand % 2 == 0)
            {
                Quaternion targetRot = skeleton.GetJoint(nuitrack.JointType.RightHand).Rotation;
                transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(targetRot.x * -1, targetRot.y * 1, targetRot.z * -1, targetRot.w * 1), speed * Time.deltaTime);
            }
            else
            {
                Quaternion targetRot = skeleton.GetJoint(nuitrack.JointType.LeftHand).Rotation;
                transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(targetRot.x * -1, targetRot.y * 1, targetRot.z * -1, targetRot.w * 1), speed * Time.deltaTime);
            }
        }
    }
}
