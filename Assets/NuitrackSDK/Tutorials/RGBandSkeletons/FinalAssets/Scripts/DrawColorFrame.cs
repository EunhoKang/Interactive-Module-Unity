using UnityEngine;
using UnityEngine.UI;

using NuitrackSDK.Frame;


namespace NuitrackSDK.Tutorials.RGBandSkeletons
{
    [AddComponentMenu("NuitrackSDK/Tutorials/RGB and Skeletons/Draw Color Frame")]
    public class DrawColorFrame : MonoBehaviour
    {
        [SerializeField] RawImage background;

        void Start()
        {
            NuitrackManager.onColorUpdate += DrawColor;
        }

        void DrawColor(nuitrack.ColorFrame frame)
        {
            background.texture = frame.ToTexture2D();
        }

        void OnDestroy()
        {
            NuitrackManager.onColorUpdate -= DrawColor;
        }
    }
}