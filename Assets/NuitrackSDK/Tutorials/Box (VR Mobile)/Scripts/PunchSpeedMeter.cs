using UnityEngine.UI;
using UnityEngine;
using NuitrackSDK.Calibration;


namespace NuitrackSDK.Tutorials.BoxVR
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Box (VR Mobile)/Punch Speed Meter")]
    public class PunchSpeedMeter : MonoBehaviour
    {
        [SerializeField] Text speedMeterText;
        [SerializeField] GameObject dummy;
        [SerializeField] Transform transformTarget;

        float maximumPunchSpeed = 0;

        void Awake()
        {
            dummy.SetActive(false);
        }

        void OnEnable()
        {
            CalibrationHandler.Instance.onSuccess += OnSuccessCalibration;
        }

        void OnSuccessCalibration(Quaternion rotation)
        {
            dummy.SetActive(true);
            transform.position = transformTarget.position + new Vector3(0, -1, 1);
        }

        public void CalculateMaxPunchSpeed(float speed)
        {
            if (maximumPunchSpeed < speed)
                maximumPunchSpeed = speed;
            speedMeterText.text = maximumPunchSpeed.ToString("f2") + " m/s";
        }

        void OnDisable()
        {
            if (CalibrationHandler.Instance)
                CalibrationHandler.Instance.onSuccess -= OnSuccessCalibration;
        }
    }
}