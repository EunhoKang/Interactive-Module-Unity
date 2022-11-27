using UnityEditor;

using NuitrackSDK.Frame;


namespace NuitrackSDKEditor.Frame
{
    [CustomEditor(typeof(Cropper), true)]
    public class CroperEditor : TrackedUserEditor
    {
        public override void DrawDefaultInspector()
        {
            serializedObject.DrawPropertyField("noUserImage", "Empty image", "The image that will be returned if the user is not detected");
            serializedObject.DrawPropertyField("margin", "Margin", "Adds an indentation proportional to the size of the face");

            SerializedProperty useSmoothMove = serializedObject.DrawPropertyField("smoothMove", "Use motion smoothing");

            if (useSmoothMove.boolValue)
                serializedObject.DrawPropertyField("smoothSpeed", "Smooth speed");

            serializedObject.DrawPropertyField("onFrameUpdate", "On frame update action");
            serializedObject.DrawPropertyField("aspectRatioUpdate", "On aspect ratio update action");

            base.DrawDefaultInspector();
        }
    }
}
