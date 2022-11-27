using UnityEngine;

using System.Collections.Generic;


namespace NuitrackSDK.Face
{
    public class UIFacesManager : MonoBehaviour
    {
        [SerializeField] RectTransform spawnRectTransform;
        [SerializeField] UIFaceInfo faceFrame;

        List<UIFaceInfo> uiFaces = new List<UIFaceInfo>();

        void Start()
        {
            for (int i = Users.MinID; i <= Users.MaxID; i++)
            {
                GameObject newFrame = Instantiate(faceFrame.gameObject, spawnRectTransform);
                newFrame.SetActive(false);

                UIFaceInfo faceInfo = newFrame.GetComponent<UIFaceInfo>();
                faceInfo.Initialize(spawnRectTransform);

                faceInfo.UseCurrentUserTracker = false;
                faceInfo.UserID = i;

                uiFaces.Add(faceInfo);
            }
        }

        void Update()
        {
            foreach(UIFaceInfo faceInfo in uiFaces)
            {
                bool isActive = faceInfo.ControllerUser != null;
                faceInfo.gameObject.SetActive(isActive);
            }
        }
    }
}