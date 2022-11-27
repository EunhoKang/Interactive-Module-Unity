using UnityEngine;


namespace NuitrackSDK
{
    [System.Serializable]
    class PressureBone
    {
        public Transform bone = null;
        public Vector3 minAngle = Vector3.zero, maxAngle = Vector3.one;
    }

    public class HandPressure : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 1)]
        float pressure;
        float pressSpeed = 20;
        [SerializeField] PressureBone[] bones;
        [SerializeField] bool rightHand = true;

        float minPressure = .5f, maxPressure = 1.0f;

        void Update()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                UserData user = NuitrackManager.Users.Current;

                if (user != null)
                {
                    if (rightHand)
                    {
                        if (user.RightHand != null)
                            pressure = Mathf.Lerp(pressure, user.RightHand.Pressure / 100.0f, pressSpeed * Time.deltaTime);
                    }
                    else
                    {
                        if (user.LeftHand != null)
                            pressure = Mathf.Lerp(pressure, user.LeftHand.Pressure / 100.0f, pressSpeed * Time.deltaTime);
                    }
                }
            }

            //pressure = Mathf.InverseLerp(minPressure, maxPressure, pressure);

            if (pressure > maxPressure)
                maxPressure = pressure;

            if (pressure < minPressure)
                minPressure = pressure;

            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].bone.localEulerAngles = Vector3.Lerp(bones[i].minAngle, bones[i].maxAngle, Mathf.InverseLerp(minPressure, maxPressure, pressure));
            }
        }
    }
}