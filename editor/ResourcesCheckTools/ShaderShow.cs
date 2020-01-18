using System;
using Engine.Asset;
using UnityEditor;
#if UNITY_EDITOR
    
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UObj = UnityEngine.Object;

#if UNITY_EDITOR
/// <summary>
/// 检查为被使用的资源
/// </summary>
public class ReplaceShaderEditor : ScriptableWizard
{
    public Shader selectShader;

    [Header("未使用的资源--依赖加载")] public UObj[] unusedStaticRes;

    [Header("未使用的动态资源--代码加载")] public UObj[] dynamicObjects;

    private void OnWizardUpdate()
    {
        //isValid = false;// (unusedNoMapping != null && unusedNoMapping.Length > 0 );
    }

    private void OnWizardCreate()
    {
        if (selectShader == null)
            return;
    }

    private void Find()
    {
        if (selectShader == null)
        {
            unusedStaticRes = null;
            return;
        }

        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(selectShader, out string guid, out long fileId);
        string path = AssetDatabase.GetAssetPath(selectShader);
        Debug.LogError($@"GUID: {guid} 
                        localId: {fileId} 
                        path: {path}", selectShader);
        if (string.IsNullOrEmpty(guid))
        {
            unusedStaticRes = null;
            return;
        }

        if (guid == "0000000000000000f000000000000000" || path.ToLower().Contains("unity_builtin_extra"))
        {
            unusedStaticRes = null;

            return;
        }

        FindProject.FindWithGuid(guid, fileId, list => { unusedStaticRes = list.ToArray(); });
    }

    private void OnWizardOtherButton()
    {
        Find();
    }

    [MenuItem("Assets/Tools/SelectShader")]
    public static void Create()
    {
        ScriptableWizard.DisplayWizard<ReplaceShaderEditor>("SelectShader", "确定", "查找");
    }
}


public class ReplaceMaterialEditor : ScriptableWizard
{
    public Material selectMaterial;

    [Header("未使用的资源--依赖加载")] public UObj[] unusedStaticRes;

    [Header("未使用的动态资源--代码加载")] public UObj[] dynamicObjects;

    private void OnWizardUpdate()
    {
        //isValid = false;// (unusedNoMapping != null && unusedNoMapping.Length > 0 );
    }

    private void OnWizardCreate()
    {
        
    }
    
    private void Find()
    {
        if (selectMaterial == null)
        {
            unusedStaticRes = null;
            return;
        }

        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(selectMaterial, out string guid, out long localId);
        Debug.LogError($"GUID: {guid} \n localId: {localId}", selectMaterial);
        if (string.IsNullOrEmpty(guid))
        {
            unusedStaticRes = null;
            return;
        }

        FindProject.FindWithGuid(guid, localId, (o) => { unusedStaticRes = o.ToArray(); });
    }

    public static void CheckDefault_Material(
        Material selectMaterial,
        Action<string, Renderer> onPrefabCallback,
        Action<string, Renderer> onModelCallback,
        Action<string, Renderer, Scene> onSceneCallback
    )
    {
        {
            string[] ids = AssetDatabase.FindAssets("t:prefab");
            for (int i = 0; i < ids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var renderers = go.GetComponentsInChildren<Renderer>(true);
                if (renderers == null || renderers.Length == 0)
                    continue;
                for (int j = 0; j < renderers.Length; j++)
                {
                    if (IsDefaultMaterial(renderers[j].sharedMaterial, selectMaterial))
                    {
                        onPrefabCallback?.Invoke(path, renderers[j]);
                    }
                }
            }
        }

        {
            string[] ids = AssetDatabase.FindAssets("t:Model");
            for (int i = 0; i < ids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var renderers = go.GetComponentsInChildren<Renderer>(true);
                if (renderers == null || renderers.Length == 0)
                    continue;
                bool dirty = false;
                for (int j = 0; j < renderers.Length; j++)
                {
                    if (IsDefaultMaterial(renderers[j].sharedMaterial, selectMaterial))
                    {
                        onModelCallback?.Invoke(path, renderers[j]);
                    }
                }
            }
        }

//        {
//            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
//            {
//                Scene scene = EditorSceneManager.GetSceneAt(i);
//                foreach (var rootGameObject in scene.GetRootGameObjects())
//                {
//                    var meshRenderers = rootGameObject.GetComponentsInChildren<Renderer>(true);
//                    foreach (var meshRenderer in meshRenderers)
//                    {
//                        var mats = meshRenderer.sharedMaterials;
//                        bool dirty = false;
//                        for (int j = 0; j < mats.Length; j++)
//                        {
//                            var material = mats[j];
//                            if (IsDefaultMaterial(material, selectMaterial))
//                            {
//                                dirty = true;
//                            }
//                        }
//
//                        if (IsDefaultMaterial(meshRenderer.sharedMaterial, selectMaterial))
//                        {
//                            dirty = true;
//                        }
//                        if (dirty)
//                        {
//                            onSceneCallback?.Invoke(scene.path, meshRenderer, scene);
//                            EditorUtility.SetDirty(meshRenderer.gameObject);
//                            EditorSceneManager.MarkSceneDirty(scene);
//                            EditorSceneManager.SaveScene(scene);
//                        }
//                    }
//                }
//            }
//
//        }

        AssetDatabase.SaveAssets();
    }

    public static bool IsDefaultMaterial(Material material, Material selectMaterial)
    {
        if (material != null)
        {
            return material.Equals(selectMaterial);
            var p = AssetDatabase.GetAssetPath(material);
            if (p.ToLower().Contains("unity_builtin_extra"))
                return true;
        }
        else
        {
            return false;
        }

        return false;
    }

    private void OnWizardOtherButton()
    {
        Find();
    }

    [MenuItem("Assets/Tools/SelectMaterial")]
    public static void Create()
    {
        ScriptableWizard.DisplayWizard<ReplaceMaterialEditor>("SelectMaterial", "确定", "查找");
    }
}
#endif
