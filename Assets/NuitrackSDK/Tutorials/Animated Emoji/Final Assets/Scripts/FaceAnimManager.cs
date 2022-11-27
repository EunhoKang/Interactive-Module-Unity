using UnityEngine;
using System.Collections.Generic;
using nuitrack;

namespace NuitrackSDK.Tutorials.AnimatedEmoji
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Animated Emoji/Face Anim Manager")]
    public class FaceAnimManager : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] FaceAnimController facePrefab;

        [Range(0, 6)]
        [SerializeField] int faceCount = 6;         //Max number of skeletons tracked by Nuitrack

        List<FaceAnimController> faceAnimControllers = new List<FaceAnimController>();
        float headsDistance = 100;

        void Start()
        {
            for (int i = 0; i < faceCount; i++)
            {
                GameObject newFace = Instantiate(facePrefab.gameObject, new UnityEngine.Vector3(i * headsDistance, 0, 0), Quaternion.identity);
                newFace.SetActive(false);
                FaceAnimController faceAnimController = newFace.GetComponent<FaceAnimController>();
                faceAnimController.Init(canvas);
                faceAnimControllers.Add(faceAnimController);
            }

            NuitrackManager.SkeletonTracker.SetNumActiveUsers(faceCount);
        }

        void Update()
        {
            for (int i = 0; i < faceAnimControllers.Count; i++)
            {
                int id = i + 1;
                UserData user = NuitrackManager.Users.GetUser(id);

                if (user != null && user.Skeleton != null && user.Face != null)
                {
                    UserData.SkeletonData.Joint headJoint = user.Skeleton.GetJoint(JointType.Head);

                    faceAnimControllers[i].gameObject.SetActive(headJoint.Confidence > 0.5f);
                    faceAnimControllers[i].UpdateFace(user.Face, headJoint);
                }
                else
                {
                    faceAnimControllers[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
