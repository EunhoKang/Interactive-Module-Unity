using UnityEditor;

using nuitrack;
using NuitrackSDK.Avatar;


namespace NuitrackSDKEditor.Avatar
{
    [CustomEditor(typeof(BaseAvatar), true)]
    public class BaseAvatarEditor : TrackedUserEditor
    {
        protected virtual JointType SelectJoint { get; set; } = JointType.None;

        public override void DrawDefaultInspector()
        {
            DrawSkeletonSettings();
            base.DrawDefaultInspector();
            DrawAvatarGUI();
        }

        /// <summary>
        /// Draw basic avatar settings
        /// </summary>
        protected void DrawSkeletonSettings()
        {
            serializedObject.DrawPropertyField("jointConfidence", "Joint confidence");
        }

        /// <summary>
        /// Override this method to add your own settings and parameters in the Inspector.
        /// </summary>
        protected virtual void DrawAvatarGUI() { }      
    }
}