using UnityEditor;
using UnityEngine;

using System.Collections.Generic;


namespace NuitrackSDKEditor.Documentation
{
    [HelpURL("https://github.com/3DiVi/nuitrack-sdk/tree/master/doc")]
    public class NuitrackTutorials : ScriptableObject
    {
        [System.Serializable]
        public class TutorialItem
        {
            [SerializeField] string label;
            [SerializeField] Texture previewImage;

            [SerializeField] string textURL;
            [SerializeField] string videoURL;
            [SerializeField, TextArea(1, 10)] string description;

            [SerializeField] SceneAsset scene;

            [SerializeField] List<string> tags;

            public string Label
            {
                get
                {
                    return label;
                }
            }

            public Texture PreviewImage
            {
                get
                {
                    return previewImage;
                }
            }

            public string TextURL
            {
                get
                {
                    return textURL;
                }
            } 
            
            public string VideoURL
            {
                get
                {
                    return videoURL;
                }
            }

            public string Description
            {
                get
                {
                    return description;
                }
            }  

            public SceneAsset Scene
            {
                get
                {
                    return scene;
                }
            }

            public List<string> Tags
            {
                get
                {
                    return tags;
                }
            }
        }

        [SerializeField] List<TutorialItem> tutorialItems;

        public List<TutorialItem> TutorialItems
        {
            get
            {
                return tutorialItems;
            }
        }
    }
}