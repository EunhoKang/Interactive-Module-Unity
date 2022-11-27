using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using nuitrack;


namespace NuitrackSDK.Tutorials.NuitrackAI
{
    [AddComponentMenu("NuitrackSDK/Tutorials/Nuitrack AI/Object Detector")]
    public class ObjectDetector : MonoBehaviour
    {
        [SerializeField] GameObject warningScreen;
        [SerializeField] AudioSource warningAS;
        [SerializeField] float detectTimeTarget = 0.3f;
        [SerializeField] Canvas detectorCanvas;
        [SerializeField] RectTransform frame;
        [SerializeField] int frameCount = 10;

        List<RectTransform> frames = new List<RectTransform>();

        Instances[] objects;
        JsonInfo objectInfo;

        float detectTime = 0;

        bool cigaretteDetected = false;

        private void Start()
        {
            for (int i = 0; i < frameCount; i++)
            {
                frames.Add(Instantiate(frame, detectorCanvas.transform));
            }
        }

        void Update()
        {
            objectInfo = NuitrackManager.NuitrackJson;

            if (objectInfo == null)
                return;

            objects = objectInfo.Instances;
            cigaretteDetected = false;

            for (int i = 0; i < frames.Count; i++)
            {
                if (objects != null && i < objects.Length && objects[i].@class != "human")
                {
                    frames[i].gameObject.SetActive(true);
                    print(objects[i].@class + objects[i].bbox.top);

                    float frameWidth = objects[i].bbox.width * Screen.width;
                    float frameHeight = objects[i].bbox.height * Screen.height;

                    float posx = objects[i].bbox.left * Screen.width;
                    float posy = objects[i].bbox.top * Screen.height;

                    frames[i].anchoredPosition = new Vector2(posx - Screen.width / 2 + frameWidth / 2, -posy + Screen.height / 2 - frameHeight / 2);
                    frames[i].sizeDelta = new Vector2(frameWidth, frameHeight);
                    frames[i].GetComponentInChildren<Text>().text = objects[i].@class;

                    if (objects[i].@class == "cigarette")
                        cigaretteDetected = true;
                }
                else
                {
                    frames[i].gameObject.SetActive(false);
                }
            }

            if (cigaretteDetected)
                detectTime += Time.deltaTime;
            else
                detectTime = 0;

            if (detectTime >= detectTimeTarget)
            {
                if (!warningAS.isPlaying)
                    warningAS.Play();

                warningScreen.SetActive(true);
            }
            else
            {
                warningAS.Stop();
                warningScreen.SetActive(false);
            }
        }
    }
}