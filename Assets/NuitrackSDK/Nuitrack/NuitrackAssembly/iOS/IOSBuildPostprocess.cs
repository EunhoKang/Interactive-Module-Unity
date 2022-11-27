#if UNITY_EDITOR
#if UNITY_IOS

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public static class IOSBuildPostprocess
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild( BuildTarget buildTarget, string path)
    {
        if(buildTarget == BuildTarget.iOS)
        {
            // Update configuration
            string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            string target = pbxProject.GetUnityMainTargetGuid();
            string targetFramework = pbxProject.GetUnityFrameworkTargetGuid();

            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            pbxProject.SetBuildProperty(targetFramework, "ENABLE_BITCODE", "NO");

            pbxProject.WriteToFile (projectPath);

            // Update plist
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            PlistElementDict rootDict = plist.root;

            PlistElementArray bgModes = rootDict.CreateArray("UISupportedExternalAccessoryProtocols");
            bgModes.AddString("io.structure.depth");
            bgModes.AddString("io.structure.infrared");
            bgModes.AddString("io.structure.control");

            File.WriteAllText(plistPath, plist.WriteToString());

        }
    }
}

#endif
#endif
