using UnityEngine;
using UnityEditor;

using UnityEditor.SceneManagement;

using System.Collections.Generic;

// Menu Item Template
// +-------------+-----------------------------+
// |             | Label                       |
// |    Image    |                             |
// |             | Description                 |
// |             |                             |
// +-------------+--------------+--------------+
// | Text button | Video button | Scene button |
// +-------------+--------------+--------------+


namespace NuitrackSDKEditor.Documentation
{
    public class NuitrackTutorialsEditorWindow : EditorWindow
    {
        const float itemHeight = 82;
        const int maxDescriptionCharCount = 200;

        static Vector2 scrollPos = Vector2.zero;
        static string selectTags = string.Empty;

        static Color filterEnableColor = Color.green;

        public static void Open()
        {
            NuitrackTutorialsEditorWindow window = GetWindow<NuitrackTutorialsEditorWindow>();
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        void OnGUI()
        {
            DrawTutorials(true);
        }

        static GUIStyle HeaderLabelStyle
        {
            get
            {
                GUIStyle headerLabelStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    fontSize = 16
                };

                return headerLabelStyle;
            }
        }

        static GUIStyle LabelStyle
        {
            get
            {
                GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
                {
                    alignment = TextAnchor.UpperLeft,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    wordWrap = true
                };

                return labelStyle;
            }
        }

        static void DrawButton(string url, string label, string icon, GUIStyle style)
        {
            GUIContent videoButtonContent = new GUIContent(label, EditorGUIUtility.IconContent(icon).image);

            EditorGUI.BeginDisabledGroup(url == null || url == string.Empty);

            if (GUILayout.Button(videoButtonContent, style))
                Application.OpenURL(url);

            EditorGUI.EndDisabledGroup();
        }

        static bool DrawToSceneButton(SceneAsset scene, string label, string icon, GUIStyle style)
        {
            GUIContent videoButtonContent = new GUIContent(label, EditorGUIUtility.IconContent(icon).image);

            EditorGUI.BeginDisabledGroup(scene == null);

            if (GUILayout.Button(videoButtonContent, style))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                string scenePath = AssetDatabase.GetAssetPath(scene);
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                Object obj = AssetDatabase.LoadAssetAtPath(scenePath, typeof(Object));
                Selection.activeObject = obj;

                return true;
            }

            EditorGUI.EndDisabledGroup();

            return false;
        }

        static bool ContainsTag(List<string> tags, List<string> filterTags)
        {
            foreach (string filterTag in filterTags)
            {
                bool haveTags = false;
                
                foreach (string tutorialTag in tags)
                {
                    if (tutorialTag.ToLower().Contains(filterTag))
                    {
                        haveTags = true;
                        break;
                    }
                }

                if (!haveTags)
                    return false;
            }

            return true;
        }

        public static void DrawTutorials(bool inspectorMode)
        {
            EditorGUILayout.LabelField("Nuitrack tutorials", HeaderLabelStyle);
            EditorGUILayout.Space();

            NuitrackTutorials tutorials = (NuitrackTutorials)AssetDatabase.LoadAssetAtPath("Assets/NuitrackSDK/TUTORIALS.asset", typeof(NuitrackTutorials));

            static void addButtonAction()
            {
                selectTags = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            Rect fieldRect = NuitrackSDKGUI.WithRightButton(addButtonAction, "winbtn_win_close", "Clear");
            Color tagSetColor = selectTags == string.Empty ? GUI.color : Color.Lerp(GUI.color, filterEnableColor, 0.5f);

            using (new GUIColor(tagSetColor))
                selectTags = EditorGUI.TextField(fieldRect, "Filter by tags", selectTags);

            GUILayout.Space(10);

            string lowerSelectTags = selectTags.ToLower();
            lowerSelectTags = lowerSelectTags.Replace(", ", ",");
            lowerSelectTags = lowerSelectTags.Replace("; ", ";");

            List<string> filterTags = new List<string>(lowerSelectTags.Split(new char[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries));

            if (inspectorMode)
                scrollPos = GUILayout.BeginScrollView(scrollPos);

            foreach (NuitrackTutorials.TutorialItem tutorialItem in tutorials.TutorialItems)
            {
                if(filterTags.Count > 0 && !ContainsTag(tutorialItem.Tags, filterTags))
                    continue;

                // Content item
                using (new VerticalGroup(EditorStyles.helpBox))
                {
                    // Content & button plane
                    using (new VerticalGroup())
                    {
                        // Content
                        using (new HorizontalGroup())
                        {
                            Texture previewImage = tutorialItem.PreviewImage != null ?
                                tutorialItem.PreviewImage :
                                EditorGUIUtility.IconContent("SceneViewVisibility@2x").image;

                            float maxWidth = (itemHeight / previewImage.height) * previewImage.width;
                            GUILayout.Box(previewImage, GUILayout.Height(itemHeight), GUILayout.Width(maxWidth));

                            // Label & description
                            using (new VerticalGroup())
                            {
                                EditorGUILayout.LabelField(tutorialItem.Label, LabelStyle);

                                string description = tutorialItem.Description;

                                if (description.Length > maxDescriptionCharCount)
                                    description = description.Substring(0, maxDescriptionCharCount) + "...";

                                EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
                            }
                        }

                        using (new HorizontalGroup())
                        {
                            foreach (string tag in tutorialItem.Tags)
                            {
                                GUIContent tagButton = new GUIContent(string.Format("#{0}", tag), "Click to show tutorials with a similar tag");

                                bool tagClicked = GUILayout.Button(tagButton, EditorStyles.linkLabel);
                                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                                if (tagClicked)
                                {
                                    selectTags += selectTags == string.Empty ? tag : string.Format(", {0}", tag);
                                    GUIUtility.keyboardControl = 0;
                                }
                            }
                        }
                        
                        GUILayout.Space(10);

                        // Button plane
                        using (new HorizontalGroup())
                        {
                            DrawButton(tutorialItem.TextURL, "Text", "UnityEditor.ConsoleWindow", EditorStyles.miniButtonLeft);
                            DrawButton(tutorialItem.VideoURL, "Video", "UnityEditor.Timeline.TimelineWindow", EditorStyles.miniButtonMid);

                            if (DrawToSceneButton(tutorialItem.Scene, "Scene", "animationvisibilitytoggleon", EditorStyles.miniButtonRight))
                                break;
                        }

                        GUILayout.Space(5);
                    }
                }
            }

            if (inspectorMode)
                GUILayout.EndScrollView();

            GUILayout.Space(10);

            if (GUILayout.Button("See more on GitHub page"))
                Application.OpenURL("https://github.com/3DiVi/nuitrack-sdk/tree/master/doc");
        }
    }
}
