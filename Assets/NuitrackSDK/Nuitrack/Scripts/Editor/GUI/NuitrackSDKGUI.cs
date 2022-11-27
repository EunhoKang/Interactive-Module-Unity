using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

using System;
using System.IO;
using System.Collections.Generic;


namespace NuitrackSDKEditor
{
    /// <summary>
    /// Put the <see cref="GUI"/> block code in the using statement to color the <see cref="GUI"/> elements in the specified color
    /// After the using block, the <see cref="GUI"/> color will return to the previous one
    ///
    /// <para>
    /// <example>
    /// This shows how to change the GUI color
    /// <code>
    /// using (new GUIColor(Color.green))
    /// {
    ///     // Your GUI code ...
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public class GUIColor : IDisposable
    {
        Color oldColor;
        readonly bool backgroundMode = false;

        public GUIColor(Color newColor, bool backgroundMode = false)
        {
            this.backgroundMode = backgroundMode;

            if (backgroundMode)
            {
                oldColor = GUI.backgroundColor;
                GUI.backgroundColor = newColor;
            }
            else
            {
                oldColor = GUI.color;
                GUI.color = newColor;
            }
        }

        public void Dispose()
        {
            if (backgroundMode)
                GUI.backgroundColor = oldColor;
            else
                GUI.color = oldColor;
        }
    }

    /// <summary>
    /// Put the <see cref="Handles"/> block code in the using statement to color the <see cref="Handles"/> elements in the specified color
    /// After the using block, the <see cref="Handles"/> color will return to the previous one
    ///
    /// <para>
    /// <example>
    /// This shows how to change the Handles color
    /// <code>
    /// using (new HandlesColor(Color.green))
    /// {
    ///     // Your Handles code ...
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public class HandlesColor : IDisposable
    {
        Color oldColor;

        public HandlesColor(Color newColor)
        {
            oldColor = Handles.color;
            Handles.color = newColor;
        }

        public void Dispose()
        {
            Handles.color = oldColor;
        }
    }

    /// <summary>
    /// Place GUI elements in a horizontal group <seealso cref="EditorGUILayout.BeginHorizontal(GUIStyle, GUILayoutOption[])"/>
    /// 
    /// <para>
    /// <example>
    /// This shows how to put GUI elements in a horizontal group via using
    /// <code>
    /// using (new HorizontalGroup())
    /// {
    ///     // Your GUI code ...
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public class HorizontalGroup : IDisposable
    {
        /// <summary>
        /// Create a horizontal group
        /// </summary>
        /// <param name="guiStyle">GUI style (default is GUIStyle.none)</param>
        /// <param name="options">GUI layout option (GUILayout.Width, GUILayout.MinWidth and others)</param>
        /// <param name="backgroundColor">Override background color</param>
        public HorizontalGroup(GUIStyle guiStyle = null, GUILayoutOption[] options = null, Color backgroundColor = default)
        {
            guiStyle ??= GUIStyle.none;
            backgroundColor = backgroundColor.Equals(default) ? GUI.color : backgroundColor;

            using (new GUIColor(backgroundColor))
                EditorGUILayout.BeginHorizontal(guiStyle, options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Place GUI elements in a vertical group <seealso cref="EditorGUILayout.BeginVertical(GUIStyle, GUILayoutOption[])"/>
    /// 
    /// <para>
    /// <example>
    /// This shows how to put GUI elements in a vertical group via using
    /// <code>
    /// using (new VerticalGroup())
    /// {
    ///     // Your GUI code ...
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    public class VerticalGroup : IDisposable
    {
        /// <summary>
        /// Create a vertical group
        /// </summary>
        /// <param name="guiStyle">GUI style (default is GUIStyle.none)</param>
        /// <param name="options">GUI layout option (GUILayout.Width, GUILayout.MinWidth and others)</param>
        /// <param name="backgroundColor">Override background color</param>
        public VerticalGroup(GUIStyle guiStyle = null, GUILayoutOption[] options = null, Color backgroundColor = default)
        {
            guiStyle ??= GUIStyle.none;
            backgroundColor = backgroundColor.Equals(default) ? GUI.color : backgroundColor;

            using (new GUIColor(backgroundColor))
                EditorGUILayout.BeginVertical(guiStyle, options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// GUI draw helper class
    /// </summary>
    public static class NuitrackSDKGUI
    {
        /// <summary>
        ///  Draw an additional button to the right of the GUI element (for example, the clear or help button)
        ///  Return a rectangle to draw your GUI element with an indent for the button.
        /// </summary>
        /// <param name="previousRect">Previous Rect (if you want to add several buttons in a row)</param>
        /// <param name="buttonAction">Action when clicking on an button</param>
        /// <param name="iconName">Name of the icon for the button</param>
        /// <param name="tooltip">(optional) ToolTip displayed when hovering over the button</param>
        /// <returns>Rectangle to draw your GUI element with an indent for the button.</returns>
        public static Rect WithRightButton(Rect previousRect, UnityAction buttonAction, string iconName, string tooltip = "")
        {
            Rect main = previousRect;

            GUIContent buttonContent = new GUIContent("", EditorGUIUtility.IconContent(iconName).image, tooltip);
            main.xMax -= buttonContent.image.width;

            Rect addButtonRect = new Rect(main.x + main.width, main.y, buttonContent.image.width, main.height);

            if (GUI.Button(addButtonRect, buttonContent, "RL FooterButton"))
                buttonAction.Invoke();

            main.xMax -= 4f;

            return main;
        }

        /// <summary>
        ///  Draw an additional button to the right of the GUI element (for example, the clear or help button)
        ///  Return a rectangle to draw your GUI element with an indent for the button.
        /// </summary>
        /// <param name="buttonAction">Action when clicking on an button</param>
        /// <param name="iconName">Name of the icon for the button</param>
        /// <param name="tooltip">(optional) ToolTip displayed when hovering over the button</param>
        /// <param name="previousRect">Previous Rect (if you want to add several buttons in a row)</param>
        /// <returns>Rectangle to draw your GUI element with an indent for the button.</returns>
        public static Rect WithRightButton(UnityAction buttonAction, string iconName, string tooltip = "")
        {
            Rect rect = EditorGUILayout.GetControlRect();

            return WithRightButton(rect, buttonAction, iconName, tooltip);
        }

        /// <summary>
        /// Draw property with "Help" button.
        /// </summary>
        /// <param name="serializedObject">Target serialized object</param>
        /// <param name="propertyName">Name of object property</param>
        /// <param name="url">Click-through link</param>
        /// <param name="toolTip">(optional) ToolTip displayed when hovering over the button</param>
        public static void PropertyWithHelpButton(SerializedObject serializedObject, string propertyName, string url, string toolTip = "")
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            UnityAction helpClick = delegate { Application.OpenURL(url); };

            Rect propertyRect = WithRightButton(helpClick, "_Help", toolTip);

            EditorGUI.PropertyField(propertyRect, property);
            serializedObject.ApplyModifiedProperties();
        }

        #region Open file & folder fields

        /// <summary>
        /// Draw a GUI block of the path to the file with the "Browse" and "Clear" buttons.
        /// This element also provides a file selection dialog box.
        /// </summary>
        /// <param name="path">Current path to file</param>
        /// <param name="filterLabel">Filter label</param>
        /// <param name="extension">Filterable file extensions</param>
        /// <returns>Path to file</returns>
        public static string OpenFileField(string path, string filterLabel, params string[] extension)
        {
            GUIContent browseButtonContent = new GUIContent("Browse", EditorGUIUtility.IconContent("Project").image);
            GUIContent clearButtonContent = new GUIContent("Clear", EditorGUIUtility.IconContent("TreeEditor.Trash").image);
            GUIContent errorMessage = new GUIContent("Specified file was not found, check the correctness of the path", GetMessageIcon(LogType.Exception));

            GUIContent warningMessage = new GUIContent("Path is not specified", GetMessageIcon(LogType.Warning));

            bool pathIsCorrect = File.Exists(path);

            Color color;

            if (path == string.Empty)
                color = Color.yellow;
            else if (!pathIsCorrect)
                color = Color.red;
            else
                color = Color.green;

            using (new VerticalGroup(EditorStyles.helpBox, null, color))
            {
                if (!pathIsCorrect || path == string.Empty)
                {
                    GUIContent message = path == string.Empty ? warningMessage : errorMessage;
                    GUILayout.Label(message, EditorStyles.wordWrappedLabel);
                }

                path = EditorGUILayout.TextField("Path to file", path);

                using (new HorizontalGroup())
                {
                    if (GUILayout.Button(browseButtonContent))
                    {
                        string windowLabel = string.Format("Open {0} file", string.Join(", ", extension));
                        string[] fileFilter = new string[]
                        {
                            filterLabel,
                            string.Join(",", extension)
                        };

                        string newFilePath = EditorUtility.OpenFilePanelWithFilters(windowLabel, Application.dataPath, fileFilter);

                        if (newFilePath != null && newFilePath != string.Empty)
                            path = newFilePath;
                    }

                    EditorGUI.BeginDisabledGroup(path == string.Empty);

                    if (GUILayout.Button(clearButtonContent))
                    {
                        path = string.Empty;
                        GUIUtility.keyboardControl = 0;
                    }

                    EditorGUI.EndDisabledGroup();
                }
            }

            return path;
        }

        /// <summary>
        /// Draw a GUI block of the path to the folder with the "Browse" and "Clear" buttons.
        /// This element also provides a file selection dialog box.
        /// </summary>
        /// <param name="path">Current path to folder</param>
        /// <param name="title">Title text</param>
        /// <param name="projectPathReuest">Require the folder to be located inside the project</param>
        /// <param name="defaultPath">Default path for the reset option (if not specified, the reset button will not be showed)</param>
        /// <returns>Path to folder</returns>
        public static string OpenFolderField(string path, string title, bool projectPathReuest = true, string defaultPath = null)
        {
            GUIContent browseButtonContent = new GUIContent("Browse", EditorGUIUtility.IconContent("Project").image);
            GUIContent clearButtonContent = new GUIContent("Clear", EditorGUIUtility.IconContent("TreeEditor.Trash").image);

            bool pathIsCorrect = Directory.Exists(path);
            bool pathInProject = projectPathReuest && path.Contains(Application.dataPath) || !projectPathReuest;

            Color color;

            if (path == string.Empty)
                color = messageColors[LogType.Warning];
            else if (!pathIsCorrect || !pathInProject)
                color = messageColors[LogType.Exception];
            else
                color = Color.green;

            using (new VerticalGroup(EditorStyles.helpBox, null, color))
            {
                if (!pathIsCorrect)
                {
                    GUIContent errorMessage = new GUIContent("Specified path was not found", GetMessageIcon(LogType.Exception));
                    GUIContent warningMessage = new GUIContent("Path is not specified", GetMessageIcon(LogType.Warning));

                    GUILayout.Label(path == string.Empty ? warningMessage : errorMessage, EditorStyles.wordWrappedLabel);
                }
                else if(!pathInProject)
                {
                    GUIContent pathNotInProjectMessage = new GUIContent(
                        "Path to the folder is outside the project, change the path",
                        GetMessageIcon(LogType.Exception));

                    GUILayout.Label(pathNotInProjectMessage, EditorStyles.wordWrappedLabel);
                }

                if(defaultPath != null)
                {
                    bool toDefault = false;
                    UnityAction resetAction = delegate { toDefault = true; };
                    Rect pathField = WithRightButton(resetAction, "preAudioLoopOff", "Reset to default path");

                    path = EditorGUI.TextField(pathField, title, toDefault ? defaultPath : path);
                }
                else
                    path = EditorGUILayout.TextField("Path to folder", path);

                using (new HorizontalGroup())
                {
                    if (GUILayout.Button(browseButtonContent))
                    {
                        string newFilePath = EditorUtility.OpenFolderPanel(title, Application.dataPath, "");

                        if (newFilePath != null && newFilePath != string.Empty)
                            path = newFilePath;
                    }

                    EditorGUI.BeginDisabledGroup(path == string.Empty);

                    if (GUILayout.Button(clearButtonContent))
                    {
                        path = string.Empty;
                        GUIUtility.keyboardControl = 0;
                    }

                    EditorGUI.EndDisabledGroup();
                }
            }

            return path;
        }

        #endregion

        /// <summary>
        /// Draw a texture while maintaining the aspect ratio by aligning it to the width.
        /// </summary>
        /// <param name="texture">Target texture</param>
        /// <param name="title">Title text</param>
        public static Rect DrawFrame(Texture texture, string title)
        {
            using (new VerticalGroup(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

                float height = 0;

                if (texture)
                    height = texture.height * (EditorGUIUtility.currentViewWidth / texture.width);

                Rect rect = EditorGUILayout.GetControlRect(false, height);

                GUI.DrawTexture(rect, texture);

                return rect;
            }
        }

        #region Message

        static readonly Dictionary<LogType, Color> messageColors = new Dictionary<LogType, Color>()
        {
            { LogType.Warning, Color.yellow },
            { LogType.Log, Color.white },
            { LogType.Error, Color.red },
            { LogType.Assert, Color.red },
            { LogType.Exception, Color.red }
        };

        static readonly Dictionary<LogType, string> messageIcons = new Dictionary<LogType, string>()
        {
            { LogType.Warning, "console.warnicon.sml" },
            { LogType.Log, "console.infoicon.sml" },
            { LogType.Error, "console.erroricon.sml" },
            { LogType.Assert, "console.erroricon.sml" },
            { LogType.Exception, "console.erroricon.sml" }
        };

        /// <summary>
        /// Get an icon for the specified type of message in the form of a Texture
        /// </summary>
        /// <param name="logType">Type of message</param>
        /// <returns>Icon Texture</returns>
        public static Texture GetMessageIcon(LogType logType)
        {
            return EditorGUIUtility.IconContent(messageIcons[logType]).image;
        }

        /// <summary>
        /// Draw a default message with a warning that there should be a NuitrackManager component on the scene
        /// </summary>
        public static void MessageIfNuitrackNotExist()
        {
            if (UnityEngine.Object.FindObjectOfType<NuitrackManager>() == null)
            {
                DrawMessage("Make sure that when the script is running, the NuitrackScripts prefab will be on the scene.", LogType.Warning);
                EditorGUILayout.Space();
            }
        }

        /// <summary>
        /// Draw a message in a frame with a backlight and an icon (with an optional action button)
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="messageType">Message type (will affect the backlight color and icon)</param>
        /// <param name="fixAction">Optional action (if set, a button will be drawn, when pressed, this action will be called)</param>
        /// <param name="fixButtonLabel">Label on the optional button</param>
        public static void DrawMessage(string message, LogType messageType, UnityAction fixAction = null, string fixButtonLabel = null)
        {
            GUIContent messageWithIcon = new GUIContent(message, GetMessageIcon(messageType));
            DrawMessage(messageWithIcon, messageColors[messageType], fixAction, new GUIContent(fixButtonLabel));
        }

        /// <summary>
        /// Draw a message in a frame with a backlight and an icon (with an optional action button)
        /// </summary>
        /// <param name="message">Message GUIContent</param>
        /// <param name="backgroundColor">Color for the message background</param>
        /// <param name="fixAction">Optional action (if set, a button will be drawn, when pressed, this action will be called)</param>
        /// <param name="fixButtonGuiContent">Label on the optional button</param>
        public static void DrawMessage(GUIContent message, Color backgroundColor, UnityAction fixAction = null, GUIContent fixButtonGuiContent = null)
        {
            using (new VerticalGroup(EditorStyles.helpBox, null, backgroundColor))
            {
                EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);

                if (fixAction != null)
                    if (GUILayout.Button(fixButtonGuiContent))
                        fixAction.Invoke();
            }
        }

        #endregion


        #region PropertyDrawer

        public static SerializedProperty DrawPropertyField(this SerializedObject serializedObject, string propertyName, string label = null, string toolTip = null, string iconName = null)
        {
            serializedObject.Update();

            label = label ?? ObjectNames.NicifyVariableName(propertyName);
            toolTip = toolTip ?? string.Empty;
            Texture icon = iconName != null ? EditorGUIUtility.IconContent(iconName).image : null;

            GUIContent propertyContent = label != null || toolTip != null || icon != null ? new GUIContent(label, icon, toolTip) : null;

            SerializedProperty property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property, propertyContent);
            serializedObject.ApplyModifiedProperties();

            return property;
        }

        public static SerializedProperty DrawPropertyField(this SerializedProperty serializedProperty, string relativePropertyName, string label = null, string toolTip = null, string iconName = null)
        {
            serializedProperty.serializedObject.Update();

            label = label ?? string.Empty;
            toolTip = toolTip ?? string.Empty;
            Texture icon = iconName != null ? EditorGUIUtility.IconContent(iconName).image : null;

            GUIContent propertyContent = label != null || toolTip != null || icon != null ? new GUIContent(label, icon, toolTip) : null;

            SerializedProperty property = serializedProperty.FindPropertyRelative(relativePropertyName);
            EditorGUILayout.PropertyField(property, propertyContent);
            serializedProperty.serializedObject.ApplyModifiedProperties();

            return property;
        }

        #endregion


        #region Text

        /// <summary>
        /// Convert the name of the bone to a human-readable form
        /// </summary>
        /// <param name="humanBone">Bone Type</param>
        /// <param name="bodyPart">Body part of bone</param>
        /// <returns>Name of the bone to a human-readable form</returns>
        public static string GetUnityDisplayBoneName(HumanBodyBones humanBone, AvatarMaskBodyPart bodyPart = AvatarMaskBodyPart.Root)
        {
            string displayName = humanBone.ToString();

            if (bodyPart == AvatarMaskBodyPart.LeftArm || bodyPart == AvatarMaskBodyPart.LeftLeg)
                displayName = displayName.Replace("Left", "");
            else if (bodyPart == AvatarMaskBodyPart.RightArm || bodyPart == AvatarMaskBodyPart.RightLeg)
                displayName = displayName.Replace("Right", "");

            return ObjectNames.NicifyVariableName(displayName);
        }

        #endregion
    }
}