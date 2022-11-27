using System.Collections.Generic;
using UnityEngine;

using NuitrackSDK.Tutorials.RGBandSkeletons;


namespace NuitrackSDK.Tutorials.FaceTracker
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Face Tracker/Face Manager")]
    public class FaceManager : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] GameObject faceController;
        [SerializeField] SkeletonController skeletonController;
        List<FaceController> faceControllers = new List<FaceController>();

        void Start()
        {
            for (int i = 0; i < skeletonController.skeletonCount; i++)
            {
                faceControllers.Add(Instantiate(faceController, canvas.transform).GetComponent<FaceController>());
            }
        }

        void Update()
        {
            for (int i = 0; i < faceControllers.Count; i++)
            {
                int id = i + 1;
                UserData user = NuitrackManager.Users.GetUser(id);

                if (user != null && user.Skeleton != null && user.Face != null)
                {
                    // Pass the face to FaceController
                    faceControllers[i].SetFace(user.Face);
                    faceControllers[i].gameObject.SetActive(true);

                    UserData.SkeletonData.Joint head = user.Skeleton.GetJoint(nuitrack.JointType.Head);

                    faceControllers[i].transform.position = head.RelativePosition(Screen.width, Screen.height);
                    //stretch the face to fit the rectangle

                    faceControllers[i].transform.localScale = user.Face.ScreenRect(Screen.width, Screen.height).size;
                }
                else
                {
                    faceControllers[i].gameObject.SetActive(false);
                }
            }
        }
    }
}