using UnityEngine;

namespace NuitrackSDK.NuitrackDemos
{
    public class CameraFullscreen : MonoBehaviour
    {
        Camera cam;

        [SerializeField] GameObject[] hideObjects;

        bool fullscreen;
        Rect defaultRect;

        private void Start()
        {
            cam = GetComponent<Camera>();
            defaultRect = cam.rect;
        }

        public void SwitchFullscreen()
        {
            fullscreen = !fullscreen;

            if (fullscreen)
            {
                cam.rect = new Rect(0, 0, 1, 1);
            }
            else
            {
                cam.rect = defaultRect;
            }

            for (int i = 0; i < hideObjects.Length; i++)
            {
                hideObjects[i].SetActive(!fullscreen);
            }
        }
    }
}