using UnityEditor;

using System;
using System.Linq;
using Reflection = System.Reflection;

using NuitrackSDK;


namespace NuitrackSDKEditor
{
    public abstract class NuitrackSDKEditor : Editor
    {
        Reflection.FieldInfo[] GetFieldInfo(Type typeObject)
        {
            Reflection.FieldInfo[] fields = typeObject.GetFields(
                Reflection.BindingFlags.Instance |
                Reflection.BindingFlags.NonPublic |
                Reflection.BindingFlags.Public);

            if (typeObject.BaseType != typeof(UnityEngine.Object))
            {
                Reflection.FieldInfo[] baseTypeFields = GetFieldInfo(typeObject.BaseType);
                fields = fields.Concat(baseTypeFields).ToArray();
            }

            return fields;
        }

        /// <summary>
        /// Get the names of fields marked with <see cref="NuitrackSDKInspector"/> attribute
        /// </summary>
        /// <returns>Array of strings</returns>
        protected string[] GetNuitrackSDKInspectorFieldNames()
        {
            Reflection.FieldInfo[] fields = GetFieldInfo(target.GetType());

            string[] excludeFieldsNames = fields.Where(f => f.IsDefined(typeof(NuitrackSDKInspector), true)).Select(f => f.Name).ToArray();
            excludeFieldsNames = excludeFieldsNames.Append("m_Script").ToArray();

            return excludeFieldsNames;
        }

        /// <summary>
        /// Draw the inspector. 
        /// Fields marked with the attribute <see cref="NuitrackSDKInspector"/> 
        /// will not be drawn in the inheritors of <see cref="NuitrackSDKEditor"/> . 
        /// </summary>
        new protected void DrawDefaultInspector()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.UpdateIfRequiredOrScript();

            string[] excludeFieldsNames = GetNuitrackSDKInspectorFieldNames();
            DrawPropertiesExcluding(serializedObject, excludeFieldsNames);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndChangeCheck();
        }

        [Obsolete("Use this method only if the GUI elements are displayed incorrectly (it may cause duplication of some elements). Use DrawDefaultInspector()", false)]
        protected void DrawDefaultUnityInspector()
        {
            base.DrawDefaultInspector();
        }
    }
}