using UnityEngine;
using System.Collections.Generic;


namespace NuitrackSDK.Tutorials.RGBandSkeletons
{
    [AddComponentMenu("NuitrackSDK/Tutorials/RGB and Skeletons/Skeleton Controller")]
    public class SkeletonController : MonoBehaviour
    {
        [Range(0, 6)]
        public int skeletonCount = 6;         //Max number of skeletons tracked by Nuitrack
        [SerializeField] SimpleSkeletonAvatar skeletonAvatar;

        List<SimpleSkeletonAvatar> avatars = new List<SimpleSkeletonAvatar>();

        void Start()
        {
            for (int i = 0; i < skeletonCount; i++)
            {
                GameObject newAvatar = Instantiate(skeletonAvatar.gameObject, transform);
                SimpleSkeletonAvatar simpleSkeleton = newAvatar.GetComponent<SimpleSkeletonAvatar>();
                simpleSkeleton.autoProcessing = false;
                avatars.Add(simpleSkeleton);
            }

            NuitrackManager.SkeletonTracker.SetNumActiveUsers(skeletonCount);
        }

        void Update()
        {
            for (int i = 0; i < avatars.Count; i++)
            {
                int id = i + 1;
                UserData user = NuitrackManager.Users.GetUser(id);

                if (user != null && user.Skeleton != null)
                {
                    avatars[i].gameObject.SetActive(true);
                    avatars[i].ProcessSkeleton(user);
                }
                else
                {
                    avatars[i].gameObject.SetActive(false);
                }
            }
        }
    }
}