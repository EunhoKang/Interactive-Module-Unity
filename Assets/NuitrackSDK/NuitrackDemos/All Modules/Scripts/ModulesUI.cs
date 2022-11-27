using UnityEngine;
using UnityEngine.UI;

namespace NuitrackSDK.NuitrackDemos
{
    public class ModulesUI : MonoBehaviour
    {
        [SerializeField] bool depthOn = true;
        [SerializeField] bool colorOn = true;
        [SerializeField] bool userOn = true;
        [SerializeField] bool skeletonOn = true;
        [SerializeField] bool handsOn = true;
        [SerializeField] bool gesturesOn = true;
        [SerializeField] bool bordersOn = true;

        [Header ("Game object for settings")]
        [SerializeField] GameObject settingsContainer;
        [SerializeField] GameObject nuitrackAiButton;

        [SerializeField] GameObject userInFrameInfo;
        bool firstUserDetection = false;

        [Header("Toggles")]
        [SerializeField] Toggle tDepth = null;
        [SerializeField] Toggle tColor = null;
        [SerializeField] Toggle tUser = null;
        [SerializeField] Toggle tSkeleton = null;
        [SerializeField] Toggle tHands = null;
        [SerializeField] Toggle tGestures = null;
        [SerializeField] Toggle tDepthMesh = null;
        [SerializeField] Toggle tBackground = null;

        NuitrackModules nuitrackModules;

        public void ToggleSettings()
        {
            settingsContainer.SetActive(!settingsContainer.activeSelf);

            userInFrameInfo.SetActive(!settingsContainer.activeSelf && !firstUserDetection);
        }

        void Start()
        {
            nuitrackAiButton.SetActive(Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer || Application.isEditor);

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            settingsContainer.SetActive(false);
            nuitrackModules = FindObjectOfType<NuitrackModules>();

            depthOn = tDepth.isOn;
            colorOn = tColor.isOn;
            userOn = tUser.isOn;
            skeletonOn = tSkeleton.isOn;
            handsOn = tHands.isOn;
            gesturesOn = tGestures.isOn;

            nuitrackModules.InitModules();
            nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);

            SwitchDepthVisualisation(tDepthMesh.isOn);
            SwitchBackground(tBackground.isOn);

            userInFrameInfo.SetActive(true);
        }

        void Update()
        {
            if (!firstUserDetection && NuitrackManager.Users.Count > 0)
            {
                firstUserDetection = true;
                userInFrameInfo.SetActive(false);
            }
        }

        bool showBackground = true;

        public void SwitchDepthVisualisation(bool meshEnabled)
        {
            UserTrackerVisualization utv = FindObjectOfType<UserTrackerVisualization>();
            if (utv != null) utv.SetActive(!meshEnabled);

            UserTrackerVisMesh utvm = FindObjectOfType<UserTrackerVisMesh>();
            if (utvm != null) utvm.SetActive(meshEnabled);

            SwitchBackground(tBackground.isOn);
        }

        public void SwitchBackground(bool bgEnabled)
        {
            showBackground = bgEnabled;
            //currentBGColor = (currentBGColor + 1) % backgroundColors.Length;
            UserTrackerVisualization utv = FindObjectOfType<UserTrackerVisualization>();
            if (utv != null) utv.SetShaderProperties(showBackground, bordersOn);

            UserTrackerVisMesh utvm = FindObjectOfType<UserTrackerVisMesh>();
            if (utvm != null) utvm.SetShaderProperties(showBackground, bordersOn);
        }

        public void SwitchBorders()
        {
            bordersOn = !bordersOn;
            UserTrackerVisualization utv = FindObjectOfType<UserTrackerVisualization>();
            if (utv != null) utv.SetShaderProperties(showBackground, bordersOn);

            UserTrackerVisMesh utvm = FindObjectOfType<UserTrackerVisMesh>();
            if (utvm != null) utvm.SetShaderProperties(showBackground, bordersOn);

        }

        public void DepthToggle()
        {
            depthOn = tDepth.isOn;
            nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
        }

        public void ColorToggle()
        {
            colorOn = tColor.isOn;
            nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
        }

        public void UserToggle()
        {
            userOn = tUser.isOn;
            nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
        }

        public void SkeletonToggle()
        {
            skeletonOn = tSkeleton.isOn;
            nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
        }

        public void HandsToggle()
        {
            handsOn = tHands.isOn;
            nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
        }

        public void GesturesToggle()
        {
            gesturesOn = tGestures.isOn;
            nuitrackModules.ChangeModules(depthOn, colorOn, userOn, skeletonOn, handsOn, gesturesOn);
        }
    }
}