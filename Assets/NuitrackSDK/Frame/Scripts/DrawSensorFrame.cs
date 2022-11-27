using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections.Generic;


namespace NuitrackSDK.Frame
{
    public class DrawSensorFrame : MonoBehaviour
    {
        public enum FrameType
        {
            Color = 0,
            Depth = 1,
            User = 2,
        }

        [SerializeField] FrameType defaultFrameType = FrameType.Color;
        [SerializeField] RectTransform panel;

        [Header ("Frame objects")]
        [SerializeField] GameObject colorImage;
        [SerializeField] GameObject depthImage;
        [SerializeField] GameObject userImage;
        [SerializeField] GameObject segmentOverlay;

        [Header ("UI elements")]
        [SerializeField] Toggle segmentToggle;
        [SerializeField] GameObject skeletonsOverlay;
        [SerializeField] Toggle skeletonToggle;

        [SerializeField] GameObject facesOverlay;
        [SerializeField] Toggle facesToggle;

        [SerializeField] Dropdown frameDropDown;

        [Header("Options")]
        [SerializeField, Range(1, 100)] int windowPercent = 20;
        [SerializeField] bool fullscreenDefault = true;
        [SerializeField] bool showSegmentOverlay = false;
        [SerializeField] bool showSkeletonsOverlay = false;
        [SerializeField] bool showFacesOverlay = false;

        bool isFullscreen;

        public void SwitchByIndex(int frameIndex)
        {
            SelectFrame((FrameType)frameIndex);
        }

        void Start()
        {
            SelectFrame(defaultFrameType);

            isFullscreen = fullscreenDefault;
            SwitchFullscreen();

            segmentToggle.isOn = showSegmentOverlay;
            segmentOverlay.SetActive(showSegmentOverlay);

            skeletonToggle.isOn = showSkeletonsOverlay;
            skeletonsOverlay.SetActive(showSkeletonsOverlay);

            facesToggle.isOn = showFacesOverlay;
            facesOverlay.SetActive(showFacesOverlay);

            if (FindObjectOfType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            List<string> frameOptions = new List<string>()
            {
                FrameType.Color.ToString(),
                FrameType.Depth.ToString(),
                FrameType.User.ToString()
            };

            frameDropDown.AddOptions(frameOptions);
        }

        bool ActiveFrameType(FrameType frameType)
        {
            return frameType switch
            {
                FrameType.Color => NuitrackManager.Instance.UseColorModule,
                FrameType.Depth => NuitrackManager.Instance.UseDepthModule,
                FrameType.User => NuitrackManager.Instance.UseUserTrackerModule,
                _ => false,
            };
        }

        void CheckToggle(Toggle toggle, bool isActive)
        {
            toggle.interactable = isActive;
            toggle.isOn = toggle.isOn && toggle.interactable;
        }

        void Update()
        {
            CheckToggle(segmentToggle, NuitrackManager.Instance.UseUserTrackerModule);
            CheckToggle(skeletonToggle, NuitrackManager.Instance.UseSkeletonTracking);
            CheckToggle(facesToggle, NuitrackManager.Instance.UseFaceTracking);

            foreach (Toggle toggle in gameObject.GetComponentsInChildren<Toggle>())
            {
                foreach(FrameType frameType in System.Enum.GetValues(typeof(FrameType)))
                {
                    string name = string.Format("Item {0}: {1}", (int)frameType, frameType.ToString());

                    if (toggle.name == name)
                        toggle.interactable = ActiveFrameType(frameType);
                }
            }
        }

        void SelectFrame(FrameType frameType)
        {
            colorImage.SetActive(frameType == FrameType.Color);
            depthImage.SetActive(frameType == FrameType.Depth);
            userImage.SetActive(frameType == FrameType.User);
        }

        public void SwitchSegmentOverlay(bool value)
        {
            segmentOverlay.SetActive(value);
        }

        public void SwitchSkeletonsOverlay(bool value)
        {
            skeletonsOverlay.SetActive(value);
        }

        public void SwitchFacesOverlay(bool value)
        {
            facesOverlay.SetActive(value);
        }

        public void SwitchFullscreen()
        {
            isFullscreen = !isFullscreen;

            if (isFullscreen)
                panel.localScale = new Vector3(1.0f / 100 * windowPercent, 1.0f / 100 * windowPercent, 1.0f);
            else
                panel.localScale = new Vector3(1, 1, 1);
        }
    }
}