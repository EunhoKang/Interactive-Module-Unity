using UnityEngine;
using System.Collections.Generic;


namespace NuitrackSDK.Tutorials.ARNuitrack.Extensions
{
    [AddComponentMenu("NuitrackSDK/Tutorials/AR Nuitrack/Extensions/Rigidbody Skeleton Manager")]
    public class RigidbodySkeletonManager : MonoBehaviour
    {
        [SerializeField] GameObject rigidBodySkeletonPrefab;
        [SerializeField] Transform space;

        Dictionary<int, RigidbodySkeletonController> skeletons = new Dictionary<int, RigidbodySkeletonController>();

        void Update()
        {
            foreach (UserData user in NuitrackManager.Users)
                if (!skeletons.ContainsKey(user.ID))
                {
                    RigidbodySkeletonController rigidbodySkeleton = Instantiate(rigidBodySkeletonPrefab, space).GetComponent<RigidbodySkeletonController>();
                    rigidbodySkeleton.UserID = user.ID;
                    rigidbodySkeleton.SetSpace(space);

                    skeletons.Add(user.ID, rigidbodySkeleton);
                }

            foreach (int skeletonID in new List<int>(skeletons.Keys))
                if (NuitrackManager.Users.GetUser(skeletonID) == null)
                {
                    Destroy(skeletons[skeletonID].gameObject);
                    skeletons.Remove(skeletonID);
                }
        }
    }
}