using UnityEditor;

using NuitrackSDK;


namespace NuitrackSDKEditor
{
    [CustomEditor(typeof(TrackedUser), true)]
    public class TrackedUserEditor : NuitrackSDKEditor
    {
        public override void OnInspectorGUI()
        {
            DrawUserTrackintSettings();
            DrawDefaultInspector();
        }

        public virtual new void DrawDefaultInspector()
        {
            base.DrawDefaultInspector();
        }

        /// <summary>
        /// Draw basic avatar settings
        /// </summary>
        protected void DrawUserTrackintSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("User tracking options", EditorStyles.boldLabel);

            SerializedProperty useCurrentUserTracker = serializedObject.DrawPropertyField("useCurrentUserTracker", "Use current user tracker");

            if (!useCurrentUserTracker.boolValue)
            {
                SerializedProperty userID = serializedObject.FindProperty("userID");
                userID.intValue = EditorGUILayout.IntSlider("User ID", userID.intValue, Users.MinID, Users.MaxID);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}