using UnityEngine;
using UnityEngine.Events;

using System.Collections;
using System.Collections.Generic;


namespace NuitrackSDK.Poses
{
    [AddComponentMenu("NuitrackSDK/Poses/Pose Detector")]
    public class PoseDetector : MonoBehaviour, IEnumerable<NuitrackPose>
    {
        /// <summary>
        /// Args event (NuitrackPose pose, int userID, float match)
        /// </summary>
        [System.Serializable]
        public class PoseProcessEvent : UnityEvent<NuitrackPose, int, float> { }

        [System.Serializable]
        public class NuitrackPoseItem
        {
            public NuitrackPose pose;
            public PoseProcessEvent poseProcessEvent;
        }


        [Tooltip("Use a manual process if you want to compare poses in your own scripts.")]
        [SerializeField] bool manualProcess = false;

        [SerializeField, NuitrackSDKInspector] List<NuitrackPoseItem> posesCollection;

        Dictionary<NuitrackPose, PoseProcessEvent> poseEvents = null;

        /// <summary>
        /// Pose matches
        ///
        /// <para>
        /// The nested dictionary contains user IDs and the matching value.
        /// </para>
        /// </summary>
        public Dictionary<NuitrackPose, Dictionary<int, float>> Matches
        {
            get;
            private set;
        }

        /// <summary>
        /// Add a listener for a pose
        /// </summary>
        /// <param name="pose">Target pose</param>
        /// <param name="listener">Target listener</param>
        public void AddListener(NuitrackPose pose, UnityAction<NuitrackPose, int, float> listener)
        {
            if (poseEvents.ContainsKey(pose))
                poseEvents[pose].AddListener(listener);
            else
                Debug.LogError(string.Format("The collection of poses does not contain the pose \"{0}\"", pose.name));
        }

        /// <summary>
        /// Remove a listener for a pose
        /// </summary>
        /// <param name="pose">Target pose</param>
        /// <param name="listener">Target listener</param>
        public void RemoveListener(NuitrackPose pose, UnityAction<NuitrackPose, int, float> listener)
        {
            if (poseEvents.ContainsKey(pose))
                poseEvents[pose].RemoveListener(listener);
            else
                Debug.LogError(string.Format("The collection of poses does not contain the pose \"{0}\"", pose.name));
        }

        void Awake()
        {
            NuitrackManager.Users.OnUserExit += Users_OnUserExit;

            poseEvents = new Dictionary<NuitrackPose, PoseProcessEvent>();
            Matches = new Dictionary<NuitrackPose, Dictionary<int, float>>();

            foreach (NuitrackPoseItem poseItem in posesCollection)
            {
                poseEvents.Add(poseItem.pose, poseItem.poseProcessEvent);
                Matches.Add(poseItem.pose, new Dictionary<int, float>());
            }
        }

        void OnDestroy()
        {
            NuitrackManager.Users.OnUserExit -= Users_OnUserExit;
        }

        void Users_OnUserExit(UserData user)
        {
            foreach (NuitrackPose pose in this)
            {
                Matches[pose].Remove(user.ID);
                poseEvents[pose].Invoke(pose, user.ID, 0);
            }
        }

        /// <summary>
        /// Count of poses
        /// </summary>
        public int CountPoses
        {
            get
            {
                return poseEvents.Count;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<NuitrackPose> GetEnumerator()
        {
            return poseEvents.Keys.GetEnumerator();
        }

        /// <summary>
        /// Add a new pose
        /// </summary>
        /// <param name="skeleton">Source skeleton</param>
        /// <param name="name">Name of the new pose</param>
        /// <returns>ID of the added pose</returns>
        public NuitrackPose AddPose(UserData.SkeletonData skeleton, string name = null)
        {
            name ??= string.Format("Pose {0}", poseEvents.Count + 1);

            NuitrackPose pose = new NuitrackPose(name, skeleton);

            poseEvents.Add(pose, new PoseProcessEvent());
            Matches.Add(pose, new Dictionary<int, float>());

            return pose;
        }

        /// <summary>
        ///  Add a new pose
        /// </summary>
        /// <param name="pose">Source pose</param>
        /// <param name="name">Name of the new pose</param>
        /// <returns>ID of the added pose</returns>
        public NuitrackPose AddPose(NuitrackPose pose, string name = null)
        {
            name = name == null ? string.Format("Pose {0}", poseEvents.Count + 1) : pose.name;
            NuitrackPose poseDuplicate = new NuitrackPose(name, pose);

            poseEvents.Add(pose, new PoseProcessEvent());
            Matches.Add(pose, new Dictionary<int, float>());

            return poseDuplicate;
        }

        /// <summary>
        /// Remove pose from PoseDetector
        /// </summary>
        /// <param name="pose">Target pose</param>
        /// <returns>true - if the pose is removed from the Postdetector, otherwise - false</returns>
        public bool RemovePose(NuitrackPose pose)
        {
            if (Matches.ContainsKey(pose))
            {
                poseEvents.Remove(pose);
                Matches.Remove(pose);
                return true;
            }
            else
                return false;
        }

        void Update()
        {
            if (manualProcess)
                return;

            foreach (NuitrackPose pose in this)
            {
                Matches[pose].Clear();

                foreach (UserData user in NuitrackManager.Users)
                {
                    float match = 0;

                    if (user.Skeleton != null)
                    {
                        match = pose.Match(user.Skeleton);
                        poseEvents[pose].Invoke(pose, user.ID, match);
                    }

                    Matches[pose].Add(user.ID, match);
                }
            }
        }
    }
}