using UnityEngine;
using UnityEngine.UI;


namespace NuitrackSDK.Poses
{
    [AddComponentMenu("NuitrackSDK/Poses/PoseItem")]
    public class PoseItem : TrackedUser
    {
        [Header("Visual")]
        [SerializeField] RectTransform rectTransform;
        [SerializeField] Text poseNameText;
        [SerializeField] Slider poseCompilance;

        [SerializeField] Graphic fillSlider;
        [SerializeField] Color succesPoseColor = Color.green;

        Color defaultFillColor;

        public RectTransform RectTransform
        {
            get
            {
                return rectTransform;
            }
        }

        void Awake()
        {
            poseCompilance.value = 0;
            defaultFillColor = fillSlider.color;
        }

        public void Init(string poseName)
        {
            poseNameText.text = poseName;
        }

        public void PoseProcess(NuitrackPose pose, int userID, float compilance)
        {
            if (userID == UserID)
            {
                poseNameText.text = pose.name;
                poseCompilance.value = compilance;
                fillSlider.color = Mathf.Approximately(compilance, 1) ? succesPoseColor : defaultFillColor;
            }
        }
    }
}
