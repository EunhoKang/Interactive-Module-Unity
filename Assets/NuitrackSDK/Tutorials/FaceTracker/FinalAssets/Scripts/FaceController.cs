using UnityEngine;


namespace NuitrackSDK.Tutorials.FaceTracker
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Face Tracker/Face Controller")]
    public class FaceController : MonoBehaviour
    {
        public nuitrack.Face.GenderType genderType;
        public nuitrack.Emotions.Type emotionType;
        public nuitrack.Age.Type ageType;

        public void SetFace(nuitrack.Face newFace)
        {
            //Gender
            genderType = newFace.Gender;

            //Age
            ageType = newFace.AgeType;

            //Emotion
            emotionType = newFace.PrevailingEmotion;
        }
    }
}