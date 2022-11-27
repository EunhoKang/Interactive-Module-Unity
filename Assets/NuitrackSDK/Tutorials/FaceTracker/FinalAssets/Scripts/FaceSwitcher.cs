using UnityEngine;


namespace NuitrackSDK.Tutorials.FaceTracker
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Face Tracker/Face Switcher")]
    public class FaceSwitcher : MonoBehaviour
    {
        [SerializeField] nuitrack.Face.GenderType gender;
        [SerializeField] nuitrack.Age.Type ageType;
        [SerializeField] nuitrack.Emotions.Type emotions;
        [SerializeField] GameObject enabledObject;
        [SerializeField] GameObject disabledObject;

        FaceController faceController;
        bool display = false;

        void Start()
        {
            faceController = GetComponentInParent<FaceController>();
        }

        void Update()
        {
            display = (gender == nuitrack.Face.GenderType.any || gender == faceController.genderType) &&
                        (ageType == nuitrack.Age.Type.any || ageType == faceController.ageType) &&
                        (emotions == nuitrack.Emotions.Type.any || emotions == faceController.emotionType);

            SwitchObjects();
        }

        void SwitchObjects()
        {
            if (enabledObject)
                enabledObject.SetActive(display);

            if (disabledObject)
                disabledObject.SetActive(!display);
        }
    }
}