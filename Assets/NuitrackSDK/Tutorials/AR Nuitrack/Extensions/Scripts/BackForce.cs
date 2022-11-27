using UnityEngine;


namespace NuitrackSDK.Tutorials.ARNuitrack.Extensions
{
    [AddComponentMenu("NuitrackSDK/Tutorials/AR Nuitrack/Extensions/Back Force")]
    public class BackForce : MonoBehaviour
    {
        [SerializeField] Rigidbody ball;
        [SerializeField] Transform targePoint;
        [SerializeField] float backForce = 4f;

        void FixedUpdate()
        {
            Vector3 force = (targePoint.position - ball.transform.position).normalized * backForce;
            ball.AddForce(force, ForceMode.Force);
        }
    }
}
