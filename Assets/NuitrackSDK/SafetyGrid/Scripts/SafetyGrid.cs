using System.Collections.Generic;
using UnityEngine;

namespace NuitrackSDK.SafetyGrid
{
    public class SafetyGrid : MonoBehaviour
    {
        [SerializeField] SpriteRenderer frontGrid, leftGrid, rightGrid;

        [SerializeField] float warningDistance = 1.5f;
        [SerializeField] float sideWidth = 5;
        [SerializeField] float fov = 60;
        [SerializeField] bool autoAdjustingFOV = true;

        [Range(0, 1)]
        [SerializeField] float sensitivity = 0.15f;

        void Start()
        {
            SetTransform();

            ChangeAlpha(frontGrid, 0);
            ChangeAlpha(leftGrid, 0);
            ChangeAlpha(rightGrid, 0);
        }

        void SetTransform()
        {
            float angle = fov / 2;

            if (autoAdjustingFOV)
                angle = NuitrackManager.DepthSensor.GetOutputMode().HFOV * Mathf.Rad2Deg / 2;

            //Set front transforms
            frontGrid.transform.localPosition = new Vector3(frontGrid.transform.localPosition.x, frontGrid.transform.localPosition.y, warningDistance);
            float sideDistance = warningDistance / Mathf.Cos(angle * Mathf.Deg2Rad);
            float frontWidth = Mathf.Sqrt(-(warningDistance * warningDistance) + sideDistance * sideDistance);
            frontGrid.size = new Vector2(frontWidth * 2 / frontGrid.transform.localScale.x, frontGrid.size.y);

            //Set side transforms
            float x = frontWidth + Mathf.Sin(angle * Mathf.Deg2Rad) * sideWidth / 2;
            float z = warningDistance + Mathf.Cos(angle * Mathf.Deg2Rad) * sideWidth / 2;
            leftGrid.transform.localPosition = new Vector3(x, leftGrid.transform.localPosition.y, z);
            rightGrid.transform.localPosition = new Vector3(-x, rightGrid.transform.localPosition.y, z);
            leftGrid.transform.localEulerAngles = new Vector3(0, angle + 90, 0);
            rightGrid.transform.localEulerAngles = new Vector3(0, -angle + 90, 0);
            leftGrid.size = new Vector2(sideWidth / leftGrid.transform.localScale.x, leftGrid.size.y);
            rightGrid.size = new Vector2(sideWidth / rightGrid.transform.localScale.x, rightGrid.size.y);
        }

        void CheckSkeletonPositions(UserData.SkeletonData skeleton)
        {
            List<UserData.SkeletonData.Joint> joints = new List<UserData.SkeletonData.Joint>(10);
            joints.Add(skeleton.GetJoint(nuitrack.JointType.Head));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.Torso));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.LeftElbow));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.LeftWrist));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.RightElbow));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.RightWrist));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.LeftKnee));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.RightKnee));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.LeftAnkle));
            joints.Add(skeleton.GetJoint(nuitrack.JointType.RightAnkle));

            float minZ = float.MaxValue;
            float proximityLeft = 0, proximityRight = 0;
            foreach (UserData.SkeletonData.Joint joint in joints)
            {
                float posX = joint.Position.x;
                float posZ = joint.Position.z;

                float angle = fov / 2;
                float sideDistance = posZ / Mathf.Cos(angle * Mathf.Deg2Rad);
                float frontWidth = Mathf.Sqrt(-(posZ * posZ) + sideDistance * sideDistance);
                float distToSide = posX / frontWidth;

                if (proximityLeft < distToSide)
                    proximityLeft = distToSide;

                if (proximityRight > distToSide)
                    proximityRight = distToSide;

                if (minZ > posZ)
                    minZ = posZ;
            }

            ChangeAlpha(frontGrid, 1.0f + (warningDistance - minZ) / (warningDistance * sensitivity));
            ChangeAlpha(leftGrid, 1.0f - (1.0f - proximityLeft) / sensitivity);
            ChangeAlpha(rightGrid, 1.0f - (1.0f + proximityRight) / sensitivity);
        }

        void ChangeAlpha(SpriteRenderer spriteRenderer, float alpha)
        {
            Color gridColor = spriteRenderer.color;
            gridColor.a = alpha;
            spriteRenderer.color = gridColor;
        }

        private void Update()
        {
            UserData userData = NuitrackManager.Users.Current;
            if (userData != null && userData.Skeleton != null)
                CheckSkeletonPositions(userData.Skeleton);
        }
    }
}
