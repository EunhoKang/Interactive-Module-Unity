using UnityEngine;

using NuitrackSDK.Poses;
using UnityEngine.Events;


namespace NuitrackSDK.Calibration
{
    public class CalibrationHandler : MonoBehaviour
    {
        [SerializeField] NuitrackPose callibrationPose;
        [SerializeField] float calibrationTime;

        #region delegates and events
        public delegate void OnStartHandler();
        public delegate void OnProgressHandler(float progress);
        public delegate void OnFailHandler();
        public delegate void OnSuccessHandler(Quaternion headRotation);

        public event OnStartHandler onStart;
        public event OnProgressHandler onProgress;
        public event OnFailHandler onFail;
        public event OnSuccessHandler onSuccess;

        [SerializeField, NuitrackSDKInspector] UnityEvent onStartEvent;
        [SerializeField, NuitrackSDKInspector] UnityEvent<float> onProgressEvent;
        [SerializeField, NuitrackSDKInspector] UnityEvent onFailEvent;
        [SerializeField, NuitrackSDKInspector] UnityEvent<Quaternion> onSuccessEvent;

        #endregion

        public float CalibrationTime { get { return calibrationTime; } }

        float timer;
        float cooldown;

        static CalibrationHandler instance = null;

        public static CalibrationHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<CalibrationHandler>();
                    if (instance)
                        DontDestroyOnLoad(instance);
                }

                return instance;
            }
        }

        static Quaternion sensorOrientation = Quaternion.identity;
        static public Quaternion SensorOrientation { get { return sensorOrientation; } }

        bool calibrationStarted;

        void Awake()
        {
            if (Instance != this)
            {
                Debug.Log("Destroy CalibrationHandler on " + gameObject.name);
                Destroy(this);
            }
        }

        void Start()
        {
            DontDestroyOnLoad(this);

            timer = 0f;
            cooldown = 0f;
            calibrationStarted = false;
        }

        void Update()
        {
            if (cooldown > 0f)
            {
                cooldown -= Time.unscaledDeltaTime;
            }
            else
            {
                if (NuitrackManager.Users.Current != null && NuitrackManager.Users.Current.Skeleton != null)
                {
                    UserData.SkeletonData skeleton = NuitrackManager.Users.Current.Skeleton;
                    
                    if (!calibrationStarted)
                        StartCalibration(skeleton);
                    else
                    {
                        if (timer > calibrationTime)
                        {
                            calibrationStarted = false;
                            timer = 0f;
                            cooldown = calibrationTime;

                            Quaternion headAngles = GetHeadAngles(skeleton);

                            onSuccess?.Invoke(headAngles);
                            onSuccessEvent.Invoke(headAngles);
                        }
                        else
                        {
                            ProcessCalibration(skeleton);
                            if (!calibrationStarted)
                            {
                                timer = 0f;
                                onFail?.Invoke();
                                onFailEvent.Invoke();
                            }
                            else
                            {
                                onProgress?.Invoke(timer / calibrationTime);
                                onProgressEvent.Invoke(timer / calibrationTime);

                                timer += Time.unscaledDeltaTime;
                            }
                        }
                    }
                }
            }
        }

        void StartCalibration(UserData.SkeletonData skeleton)
        {
            float poseMath = callibrationPose.Match(skeleton);

            if (Mathf.Approximately((float)System.Math.Round(poseMath, 3), 1))
            {
                calibrationStarted = true;

                onStart?.Invoke();
                onStartEvent.Invoke();
            }
        }

        void ProcessCalibration(UserData.SkeletonData skeleton)
        {
            float poseMath = callibrationPose.Match(skeleton);

            if (!Mathf.Approximately((float)System.Math.Round(poseMath, 3), 1))
                calibrationStarted = false;
        }

        Quaternion GetHeadAngles(UserData.SkeletonData skeleton)
        {
            Vector3 deltaWrist = skeleton.GetJoint(nuitrack.JointType.LeftWrist).Position - skeleton.GetJoint(nuitrack.JointType.RightWrist).Position;

            float angleY = -Mathf.Rad2Deg * Mathf.Atan2(deltaWrist.z, deltaWrist.x);
            float angleX = -Mathf.Rad2Deg * Mathf.Atan2(Input.gyro.gravity.z, -Input.gyro.gravity.y);

            Vector3 torso = NuitrackManager.Users.Current.Skeleton.GetJoint(nuitrack.JointType.Torso).Position;
            Vector3 neck = NuitrackManager.Users.Current.Skeleton.GetJoint(nuitrack.JointType.Neck).Position;
            Vector3 diff = neck - torso;

            sensorOrientation = Quaternion.Euler(Mathf.Atan2(diff.z, diff.y) * Mathf.Rad2Deg, 0f, 0f);

            //Debug.Log("Gravity vector: " + Input.gyro.gravity.ToString("0.000") + "; AngleX: " + angleX.ToString("0") + "; AngleY: " + angleY.ToString("0"));

            return Quaternion.Euler(angleX, angleY, 0f);
        }
    }
}