using UnityEngine;

namespace NuitrackSDK.NuitrackDemos
{
    public class FovVisualSetter : MonoBehaviour
    {
        [SerializeField] RectTransform firstLine;
        [SerializeField] RectTransform secondLine;

        [SerializeField] bool needVFov;

        void Start()
        {
            nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();
            if (needVFov)
            {
                float fov = 2 * Mathf.Atan(Mathf.Tan(mode.HFOV / 2) * (float)mode.YRes / (float)mode.XRes);
                firstLine.localEulerAngles = new Vector3(0, 0, fov * Mathf.Rad2Deg / 2);
                secondLine.localEulerAngles = new Vector3(0, 0, -fov * Mathf.Rad2Deg / 2);
            }
            else
            {
                firstLine.localEulerAngles = new Vector3(0, 0, mode.HFOV * Mathf.Rad2Deg / 2 - 90);
                secondLine.localEulerAngles = new Vector3(0, 0, -mode.HFOV * Mathf.Rad2Deg / 2 - 90);
            }
        }
    }
}