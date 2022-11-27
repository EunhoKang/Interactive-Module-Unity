using UnityEngine;


namespace NuitrackSDK.Tutorials.BoxVR
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Box (VR Mobile)/Punch Speed Sender")]
    public class PunchSpeedSender : MonoBehaviour
    {
        [SerializeField] PunchSpeedMeter punchSpeedMeter;

        private void OnCollisionEnter(Collision collision)
        {
            punchSpeedMeter.CalculateMaxPunchSpeed(collision.relativeVelocity.magnitude);
        }
    }
}