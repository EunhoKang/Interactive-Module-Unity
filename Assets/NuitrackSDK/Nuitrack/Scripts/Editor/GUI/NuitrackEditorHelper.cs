using UnityEngine;
using UnityEditor;

using System.IO;


namespace NuitrackSDKEditor
{
    public static class NuitrackEditorHelper
    {
        /// <summary>
        /// Create a new asset (where T is ScriptableObject)
        /// </summary>
        /// <typeparam name="T">Your asset inherited from ScriptableObject</typeparam>
        /// <param name="assetName">Asset file name</param>
        /// <param name="assetPath">The path in the form of an enumeration of folders. If the subfolder does not exist, it will be created.</param>
        /// <returns>Created asset</returns>
        public static T CreateAsset<T>(string assetName, params string[] assetPath) where T : ScriptableObject
        {
            T newAsset = ScriptableObject.CreateInstance<T>();

            string assetFilename = string.Format("{0}.asset", assetName);

            string currentPath = "Assets";
            
            foreach(string aPath in assetPath)
            {
                string nPath = Path.Combine(currentPath, aPath);

                if (!AssetDatabase.IsValidFolder(nPath))
                {
                    string guid = AssetDatabase.CreateFolder(currentPath, aPath);
                    currentPath = AssetDatabase.GUIDToAssetPath(guid);
                }
                else
                    currentPath = nPath;
            }

            string newAssetFullPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(currentPath, assetFilename));

            AssetDatabase.CreateAsset(newAsset, newAssetFullPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(newAsset);

            return newAsset;
        }
    }
}