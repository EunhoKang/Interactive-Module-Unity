using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;


namespace NuitrackSDK.Poses
{
    [AddComponentMenu("NuitrackSDK/Poses/PoseViewer")]
    public class PoseViewer : TrackedUser
    {
        [Header("Pose viewer")]
        [SerializeField] PoseDetector poseDetector;
        [SerializeField] Text userInfo;

        [Header("Pose list")]
        [SerializeField] GameObject poseItemPrefab;
        [SerializeField] RectTransform posesContent;

        List<PoseItem> poseItems = null;

        PoseItem customPoseItem = null;
        NuitrackPose customPose = null;

        const string customPoseName = "Custom pose";

        void Awake()
        {
            poseItems = new List<PoseItem>();

            foreach (NuitrackPose pose in poseDetector)
            {
                PoseItem poseItem = CreatePoseItem(pose);
                poseDetector.AddListener(pose, poseItem.PoseProcess);
            }
        }

        PoseItem CreatePoseItem(NuitrackPose pose)
        {
            float height = poseItemPrefab.GetComponent<RectTransform>().rect.height;

            PoseItem poseItem = Instantiate(poseItemPrefab, posesContent).GetComponent<PoseItem>();
            poseItem.RectTransform.anchoredPosition = new Vector2(0, -height * poseItems.Count);
            poseItem.UseCurrentUserTracker = UseCurrentUserTracker;

            if (!UseCurrentUserTracker)
                poseItem.UserID = UserID;

            poseItem.Init(pose.name);
            poseItems.Add(poseItem);

            return poseItem;
        }

        void DeletePoseItem(PoseItem poseItem)
        {
            poseItems.Remove(poseItem);
            Destroy(poseItem.gameObject);
        }

        void Update()
        {
            userInfo.text = ControllerUser != null ? string.Format("User ID: {0}", ControllerUser.ID) : "User not found";
        }

        public void AddCustomPose()
        {
            if (ControllerUser != null && ControllerUser.Skeleton != null)
            {
                if (customPose != null)
                {
                    poseDetector.RemoveListener(customPose, customPoseItem.PoseProcess);
                    poseDetector.RemovePose(customPose);

                    DeletePoseItem(customPoseItem);
                }

                customPose = poseDetector.AddPose(ControllerUser.Skeleton, customPoseName);
                customPoseItem = CreatePoseItem(customPose);

                poseDetector.AddListener(customPose, customPoseItem.PoseProcess);

                Debug.Log("Pose has been added successfully, you can view it in the PoaseDetector.");
            }
            else
                Debug.LogError("User not found. Make sure that the user is in the frame.");
        }
    }
}