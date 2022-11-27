using UnityEngine;
using UnityEngine.UI;

using NuitrackSDK.Tutorials.FaceTracker;


namespace NuitrackSDK.Tutorials.AnimatedEmoji
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Animated Emoji/Face Anim Controller")]
    public class FaceAnimController : MonoBehaviour
    {
        [SerializeField] Transform headRoot;

        [SerializeField] RawImage rawImage;
        [SerializeField] Camera faceCamera;

        [SerializeField] SkinnedMeshRenderer faceMeshRenderer;
        [SerializeField] RenderTexture renderTextureSample;
        [SerializeField] float smoothHeadRotation = 5;

        //Face Animation
        [Header("BlendShapesIds")]
        [SerializeField] int jawOpen = 6;
        [SerializeField] int eyeBlinkLeft = 0;
        [SerializeField] int eyeBlinkRight = 2;
        [SerializeField] int mouthLeft = 10;
        [SerializeField] int mouthRight = 11;
        [SerializeField] int browUpLeft = 17;
        [SerializeField] int browUpRight = 18;

        RenderTexture renderTexture;

        Quaternion baseRotation;
        BlendshapeWeights blendshapeWeights = new BlendshapeWeights();
        Quaternion newRotation;
        RawImage faceRaw;

        public void Init(Canvas canvas)
        {
            baseRotation = headRoot.rotation;
            faceRaw = Instantiate(rawImage, canvas.transform).GetComponent<RawImage>();
            faceRaw.transform.localScale = Vector2.one * Screen.height;

            renderTexture = new RenderTexture(renderTextureSample);
            faceCamera.targetTexture = renderTexture;
            faceRaw.texture = renderTexture;
            faceRaw.gameObject.SetActive(false);
        }

        public void UpdateFace(nuitrack.Face face, UserData.SkeletonData.Joint headJoint)
        {
            faceRaw.transform.position = headJoint.RelativePosition(Screen.width, Screen.height);

            headRoot.localPosition = new Vector3(0, 0, -headJoint.Position.z);

            if (face.landmark == null)
                return;

            //Mouth
            faceMeshRenderer.SetBlendShapeWeight(jawOpen, blendshapeWeights.GetJawOpen(face));

            //Eyes
            faceMeshRenderer.SetBlendShapeWeight(eyeBlinkLeft, blendshapeWeights.GetEyeBlinkLeft(face));
            faceMeshRenderer.SetBlendShapeWeight(eyeBlinkRight, blendshapeWeights.GetEyeBlinkRight(face));

            //Smile
            faceMeshRenderer.SetBlendShapeWeight(mouthLeft, blendshapeWeights.GetSmile(face));
            faceMeshRenderer.SetBlendShapeWeight(mouthRight, blendshapeWeights.GetSmile(face));

            //Brows
            faceMeshRenderer.SetBlendShapeWeight(browUpLeft, blendshapeWeights.GetBrowUpLeft(face));
            faceMeshRenderer.SetBlendShapeWeight(browUpRight, blendshapeWeights.GetBrowUpRight(face));

            //Head rotation
            newRotation = baseRotation * face.Rotation;
        }

        void OnDisable()
        {
            if (faceRaw != null)
                faceRaw.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            if (faceRaw != null)
                faceRaw.gameObject.SetActive(true);
        }

        void Update()
        {
            headRoot.rotation = Quaternion.Slerp(headRoot.rotation, newRotation, smoothHeadRotation * Time.deltaTime);
        }
    }
}