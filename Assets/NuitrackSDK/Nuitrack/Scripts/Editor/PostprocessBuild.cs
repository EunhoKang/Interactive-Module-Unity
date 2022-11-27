using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class NuitrackBuildPostprocessor 
{
	public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
	{
		if (!Directory.Exists(target.FullName))
        {
            Directory.CreateDirectory(target.FullName);
        }

		foreach (FileInfo fi in source.GetFiles())
        {
			if (Path.GetExtension(fi.Name) != ".meta")
			{
				fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
			}
        }

		foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
	}

	[PostProcessBuildAttribute(1)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) 
	{
#if UNITY_STANDALONE_WIN
        var editorPluginsPath = Path.Combine(Application.dataPath, "NuitrackSDK/Plugins/x86_64");
        if (!Directory.Exists(editorPluginsPath))
            return;

		var buildPluginsPath = $"{Directory.GetParent(pathToBuiltProject)}/{Path.GetFileNameWithoutExtension(pathToBuiltProject)}_Data/Plugins";
		if (Directory.Exists(Path.Combine(buildPluginsPath, "x86_64")))
		{
			buildPluginsPath = Path.Combine(buildPluginsPath, "x86_64");
		}

		CopyAll(new DirectoryInfo(editorPluginsPath), new DirectoryInfo(buildPluginsPath));
#endif
	}
}
