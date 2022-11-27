using UnityEngine;


namespace NuitrackSDK.Calibration
{
    public class CameraRecenter : MonoBehaviour
    {
        [Header("This object doesn't rotated")]
        [SerializeField] Transform headBasisTransform;
        [Header("This object will be rotated after calibration")]
        [SerializeField] Transform yAxisCorrectionTransform;
        [Header("This object should rotate to follow the head (Cardboard plugin, etc)")]
        [SerializeField] Transform gazeDirectionTransform;

        CalibrationHandler calibrationHandler;

        static Quaternion correctionQ = Quaternion.identity;

        private void Start()
        {
            yAxisCorrectionTransform.localRotation = correctionQ;
            Debug.Log("Note: For rotating head on IOS and Android you can use Google Cardboard: https://developers.google.com/cardboard/develop/unity/quickstart");
        }

        void Recenter(Quaternion headRotation)
        {
            Vector3 gazeDirection = gazeDirectionTransform.forward;

            Vector3 gazeDirHead = headBasisTransform.InverseTransformVector(gazeDirection);
            Quaternion currentRotation = Quaternion.Euler(0f, Mathf.Atan2(gazeDirHead.x, gazeDirHead.z) * Mathf.Rad2Deg, 0f);

            Vector3 handsDirection = headRotation * Vector3.forward;
            Quaternion yPartRotation = Quaternion.Euler(0f, Mathf.Atan2(handsDirection.x, handsDirection.z) * Mathf.Rad2Deg, 0f);

            Quaternion correction = yPartRotation * Quaternion.Inverse(currentRotation);
            yAxisCorrectionTransform.localRotation = yAxisCorrectionTransform.localRotation * correction;
            yAxisCorrectionTransform.localEulerAngles += new Vector3(0, 180, 0);

            correctionQ = yAxisCorrectionTransform.localRotation;
        }

        private void OnEnable()
        {
            calibrationHandler = FindObjectOfType<CalibrationHandler>();
            calibrationHandler.onSuccess += Recenter;
        }

        void OnDisable()
        {
            calibrationHandler.onSuccess -= Recenter;
        }
    }
}