using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

public class FindProject
{
	static List<Object> resultList = new List<Object>(64);
	private static Action<List<Object>> onComplete = null;

	public static void Find(Action<List<Object>> onCompleteCallback)
	{
		onComplete = onCompleteCallback;
		Find();
	}
	public static void FindWithGuid(string guid, long fileId,Action<List<Object>> onCompleteCallback)
	{
		onComplete = onCompleteCallback;
		FindWithGuid(guid, fileId);
	}
	
    [MenuItem("Assets/Find References", false, 10)]
    public static void Find()
    {
	    EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!string.IsNullOrEmpty(path))
        {
	        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(Selection.activeObject, out string guid, out long fileId);
	        FindWithGuid(guid, fileId);
        }
    }
    private static string GUIDFormatWithFileIdStr = "fileID: {0}, guid: 0000000000000000f000000000000000, type: 0";
    public static void FindWithGuid(string guid, long fileId)
    {
	    resultList.Clear();
	    if (guid == "0000000000000000f000000000000000")
		    guid = string.Format(GUIDFormatWithFileIdStr, fileId);
	    List<string> withoutExtensions = new List<string>() {".prefab", ".unity", ".mat", ".asset"};
	    string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
		    .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
	    int startIndex = 0;

	    EditorApplication.update = delegate()
	    {
		    string file = files[startIndex];

		    bool isCancel =
			    EditorUtility.DisplayCancelableProgressBar("匹配资源中", file,
				    (float) startIndex / (float) files.Length);

		    if (Regex.IsMatch(File.ReadAllText(file), guid))
		    {
			    var uobj = AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file));
			    Debug.Log(file, uobj);
			    resultList.Add(uobj);
		    }

		    startIndex++;
		    if (isCancel || startIndex >= files.Length)
		    {
			    EditorUtility.ClearProgressBar();
			    EditorApplication.update = null;
			    startIndex = 0;
			    Debug.Log("匹配结束");
			    onComplete?.Invoke(resultList);
			    onComplete = null;
		    }
	    };
    }

    [MenuItem("Assets/Find References", true)]
    static private bool VFind()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return (!string.IsNullOrEmpty(path));
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }


#if UNITY_EDITOR_OSX
	
	[MenuItem("Assets/Find References In Project", false, 2000)]
	private static void FindProjectReferences()
	{
		string appDataPath = Application.dataPath;
		string output = "";
		string selectedAssetPath = AssetDatabase.GetAssetPath (Selection.activeObject);
		List<string> references = new List<string>();
		
		string guid = AssetDatabase.AssetPathToGUID (selectedAssetPath);
		
		var psi = new System.Diagnostics.ProcessStartInfo();
		psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
		psi.FileName = "/usr/bin/mdfind";
		psi.Arguments = "-onlyin " + Application.dataPath + " " + guid;
		psi.UseShellExecute = false;
		psi.RedirectStandardOutput = true;
		psi.RedirectStandardError = true;
		
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		process.StartInfo = psi;
		
		process.OutputDataReceived += (sender, e) => {
			if(string.IsNullOrEmpty(e.Data))
				return;
			
			string relativePath = "Assets" + e.Data.Replace(appDataPath, "");
			
			// skip the meta file of whatever we have selected
			if(relativePath == selectedAssetPath + ".meta")
				return;
			
			references.Add(relativePath);
			
		};
		process.ErrorDataReceived += (sender, e) => {
			if(string.IsNullOrEmpty(e.Data))
				return;
			
			output += "Error: " + e.Data + "\n";
		};
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		
		process.WaitForExit(2000);
		
		foreach(var file in references){
			output += file + "\n";
			Debug.Log(file, AssetDatabase.LoadMainAssetAtPath(file));
		}
		
		Debug.LogWarning(references.Count + " references found for object " + Selection.activeObject.name + "\n\n" + output);
	}
	
	#endif
}