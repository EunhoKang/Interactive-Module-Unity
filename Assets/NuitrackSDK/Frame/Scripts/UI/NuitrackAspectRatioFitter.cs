using UnityEngine;
using UnityEngine.UI;


namespace NuitrackSDK.Frame
{
    [AddComponentMenu("NuitrackSDK/Frame/UI/Nuitrack Aspect Ratio Fitter")]
    public class NuitrackAspectRatioFitter : AspectRatioFitter
    {
        public enum FrameMode
        {
            Color = 0,
            Depth = 1,
            Segment = 2
        }

        [SerializeField] FrameMode frameMode = FrameMode.Color;

        RectTransform m_Rect;

        public RectTransform RectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            switch (frameMode)
            {
                case FrameMode.Color:
                    NuitrackManager.onColorUpdate += NuitrackManager_onFrameUpdate;
                    break;
                case FrameMode.Depth:
                    NuitrackManager.onDepthUpdate += NuitrackManager_onFrameUpdate;
                    break;
                case FrameMode.Segment:
                    NuitrackManager.onUserTrackerUpdate += NuitrackManager_onFrameUpdate;
                    break;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            switch (frameMode)
            {
                case FrameMode.Color:
                    NuitrackManager.onColorUpdate -= NuitrackManager_onFrameUpdate;
                    break;
                case FrameMode.Depth:
                    NuitrackManager.onDepthUpdate -= NuitrackManager_onFrameUpdate;
                    break;
                case FrameMode.Segment:
                    NuitrackManager.onUserTrackerUpdate -= NuitrackManager_onFrameUpdate;
                    break;
            }
        }

        void NuitrackManager_onFrameUpdate<T>(nuitrack.Frame<T> frame) where T : struct
        {
            float frameAspectRatio = (float)frame.Cols / frame.Rows;
            aspectRatio = frameAspectRatio;
        }
    }
}