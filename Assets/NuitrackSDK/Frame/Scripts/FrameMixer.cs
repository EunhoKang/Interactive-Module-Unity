using UnityEngine;
using UnityEngine.Events;


namespace NuitrackSDK.Frame
{
    [AddComponentMenu("NuitrackSDK/Frame/Frame Mixer")]
    public class FrameMixer : MonoBehaviour
    {
        public enum Mode
        {
            Cut,
            ReverseCut,
            Mul,
            Mix
        }

        [SerializeField, NuitrackSDKInspector] bool useStaticMainTexture;
        [SerializeField, NuitrackSDKInspector] Texture staticMainTexture;

        [SerializeField, NuitrackSDKInspector] bool useStaticMaskTexture;
        [SerializeField, NuitrackSDKInspector] Texture staticMaskTexture;

        [SerializeField] Mode mode = Mode.Cut;
        [SerializeField] UnityEvent<Texture> onFrameUpdate;

        RenderTexture renderTexture;

        Texture overrideMainTexture = null;
        Texture overrideMaskTexture = null;


        void Update()
        {
            Texture mainTexture = useStaticMainTexture ? staticMainTexture : overrideMainTexture;
            Texture maskTexture = useStaticMaskTexture ? staticMaskTexture : overrideMaskTexture;

            if (mainTexture == null || maskTexture == null)
                return;

            switch (mode)
            {
                case Mode.Cut:
                    FrameUtils.TextureUtils.Cut(mainTexture, maskTexture, ref renderTexture);
                    break;
                case Mode.ReverseCut:
                    FrameUtils.TextureUtils.ReverseCut(mainTexture, maskTexture, ref renderTexture);
                    break;
                case Mode.Mul:
                    FrameUtils.TextureUtils.Mul(mainTexture, maskTexture, ref renderTexture);
                    break;
                case Mode.Mix:
                    FrameUtils.TextureUtils.MixMask(mainTexture, maskTexture, ref renderTexture);
                    break;
            }

            onFrameUpdate.Invoke(renderTexture);
        }

        public void MainTexture(Texture texture)
        {
            overrideMainTexture = texture;
        }

        public void MaskTexture(Texture texture)
        {
            overrideMaskTexture = texture;
        }
    }
}