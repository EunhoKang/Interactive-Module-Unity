#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using UnityEngine.UI;
using NuitrackSDK.Calibration;


namespace NuitrackSDK.Tutorials.MotionCapture
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Motion Capture/Unity Animation Recorder")]
    public class UnityAnimationRecorder : MonoBehaviour
    {
        enum RecordMode { Generic, Humanoid };

        [Header("Generic")]
        [SerializeField] RecordMode recordMode = RecordMode.Generic;

        [Header("Save")]
        [SerializeField] string savePath = "Assets/NuitrackSDK/Tutorials/Motion Capture/Animations";
        [SerializeField] string fileName = "Example";

        [Header("Control")]
        [SerializeField] CalibrationHandler poseCalibration;
        [SerializeField] GameObject recordIcon;

        bool isRecording = false;
        IRecordable recordable = null;

        [Header("Generic Animations")]

        [SerializeField] Transform root;
        [SerializeField] Transform[] transforms;

        [Header("Humanoid Animations")]
        [SerializeField] AnimatorAvatar animatorAvatar;

        [Header("UI")]
        [SerializeField] Button startRecordButton;
        [SerializeField] Button stopRecordButton;

        void Start()
        {
            poseCalibration.onSuccess += PoseCalibration_onSuccess;

            switch (recordMode)
            {
                case RecordMode.Generic:
                    recordable = new GenericRecorder(transforms, root);
                    break;

                case RecordMode.Humanoid:
                    recordable = new HumanoidRecoder(animatorAvatar.GetAnimator, animatorAvatar.GetHumanBodyBones);
                    break;
            }
        }

        private void OnDestroy()
        {
            poseCalibration.onSuccess -= PoseCalibration_onSuccess;
        }

        public void StartRecord()
        {
            if (!isRecording)
            {
                Debug.Log("Start recording");
                isRecording = true;

                startRecordButton.interactable = false;
                stopRecordButton.interactable = true;

                recordIcon.SetActive(true);
            }
        }

        public void StopRecord()
        {
            if (isRecording)
            {
                Debug.Log("Stop recording");
                isRecording = false;

                startRecordButton.interactable = true;
                stopRecordButton.interactable = false;

                recordIcon.SetActive(false);

                SaveToFile(recordable.GetClip);
            }
        }

        private void PoseCalibration_onSuccess(Quaternion rotation)
        {
            if (!isRecording)
                StartRecord();
            else
                StopRecord();
        }

        void Update()
        {
            if (isRecording)
                recordable.TakeSnapshot(Time.deltaTime);
        }

        void SaveToFile(AnimationClip clip)
        {
            string path = savePath + "/" + fileName + ".anim";
            clip.name = fileName;

            AssetDatabase.CreateAsset(clip, path);
            Debug.Log("Save to: " + path);
        }
    }
}
#endif