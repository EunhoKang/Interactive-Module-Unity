using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using System.Collections.Generic;


namespace NuitrackSDKEditor.Poses
{
    public class NuitrackPreviewStage : PreviewSceneStage
    {
        Object selectedObject = null;
        List<GameObject> sceneObjects = null;

        public void SceneSetup(Object selectedObject)
        {
            this.selectedObject = selectedObject;

            sceneObjects = new List<GameObject>();

            GameObject lightingObject = new GameObject("Lighting");
            lightingObject.hideFlags = HideFlags.HideAndDontSave;

            lightingObject.AddComponent<Light>().type = LightType.Directional;
            lightingObject.transform.eulerAngles = new Vector3(45, 175, 0);
            sceneObjects.Add(lightingObject);

            StageUtility.PlaceGameObjectInCurrentStage(lightingObject);
        }

        protected override void OnCloseStage()
        {
            base.OnCloseStage();

            foreach(GameObject gameObject in sceneObjects)
                DestroyImmediate(gameObject);

            sceneObjects.Clear();
        }

        protected override GUIContent CreateHeaderContent()
        {
            GUIContent headerContent = new GUIContent
            {
                text = selectedObject.name,
                image = EditorGUIUtility.IconContent("AvatarSelector").image
            };

            return headerContent;
        }
    }
}