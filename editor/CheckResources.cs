using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.EditorApplication;

public class CheckResources
{
    [MenuItem("Tools/AssetCheck/StandardShader")]
    public static void CheckStandard()
    {

        var defaultMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtResources/DefaultMaterial.mat");
        string[] ids = AssetDatabase.FindAssets("t:material");
        Debuger.Log("Materials count: " + ids.Length);
        for (int i = 0; i < ids.Length; i++)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(ids[i]);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat.shader.name.ToLower().Equals("standard"))
            {
                Debuger.LogErrorFormat(nameof(CheckResources),"Standard Shader: " + mat.name + "   path: " + matPath, mat);
                mat.shader = Shader.Find("Standard-s");
            }
        }
        AssetDatabase.SaveAssets();

        ids = AssetDatabase.FindAssets("t:Model");
        Debuger.Log("Models count: " + ids.Length);
        for (int i = 0; i < ids.Length; i++)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(ids[i]);
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(matPath);
            var meshRenderers = fbx.GetComponentsInChildren<Renderer>(true);
            foreach (var meshRenderer in meshRenderers)
            {
                var mats = meshRenderer.sharedMaterials;
                for (int j = 0; j < mats.Length; j++)
                {
                    var material = mats[j];
                    if (material == null)
                    {
                        //Debuger.LogError("null material: " + rootGameObject.name + "   meshRenderer:" + meshRenderer.name);
                        continue;
                    }

                    if (material.shader.name.ToLower().Equals("standard"))
                    {
                        Debuger.LogErrorFormat(nameof(CheckResources),"Standard Shader: " + fbx.name + "   meshRenderer:" + meshRenderer.name, fbx);
                        meshRenderer.sharedMaterials[j] = defaultMat;
                    }
                }
            }

        }

        
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                var meshRenderers = rootGameObject.GetComponentsInChildren<Renderer>(true);
                foreach (var meshRenderer in meshRenderers)
                {
                    var mats = meshRenderer.sharedMaterials;
                    for (int j = 0; j < mats.Length; j++)
                    {
                        var material = mats[j];
                        if (material == null)
                        {
                            //Debuger.LogError("null material: " + rootGameObject.name + "   meshRenderer:" + meshRenderer.name);
                            continue;
                        }

                        if (material.shader.name.ToLower().Equals("standard"))
                        {
                            Debuger.LogError("Standard Shader: " + "Scene:" + scene.name + " " + rootGameObject.name + "   meshRenderer:" + meshRenderer.name);
                            meshRenderer.sharedMaterials[j] = defaultMat;
                            EditorUtility.SetDirty(meshRenderer.gameObject);
                        }
                    }
                    if (meshRenderer.sharedMaterial == null)
                    {
                        //Debuger.LogError("null material: " + rootGameObject.name + "   meshRenderer:" + meshRenderer.name);
                        continue;
                    }

                    if (meshRenderer.sharedMaterial.shader.name.ToLower().Equals("standard"))
                    {
                        Debuger.LogError("Standard Shader: " + "Scene:" + scene.name + " " + rootGameObject.name + "   meshRenderer:" + meshRenderer.name);
                        meshRenderer.sharedMaterial = defaultMat;
                        EditorUtility.SetDirty(meshRenderer.gameObject);
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                }
            }
           
        }
        AssetDatabase.SaveAssets();
        
    }

    [MenuItem("Tools/AssetCheck/Legacy Shaders_Bumped Specular")]
    public static void CheckLLLL()
    {

        string[] ids = AssetDatabase.FindAssets("t:material");
        Debuger.Log("Materials count: " + ids.Length);
        for (int i = 0; i < ids.Length; i++)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(ids[i]);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat.shader.name.ToLower().Contains("Legacy".ToLower()) && mat.shader.name.ToLower().Contains("Specular".ToLower()))
            {
                Debuger.LogErrorFormat(nameof(CheckResources),"Legacy Shaders_Bumped Specular Shader: " + mat.name + "   path: " + matPath, mat);
            }
        }
    }
    [MenuItem("Tools/AssetCheck/Legacy Shaders_Bumped Specular Replace")]
    public static void CheckLegacyShaderAndReplace()
    {
        string[] ids = AssetDatabase.FindAssets("t:material");
        Debuger.Log("Materials count: " + ids.Length);
        for (int i = 0; i < ids.Length; i++)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(ids[i]);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat.shader.name.ToLower().Contains("Legacy".ToLower()) && mat.shader.name.ToLower().Contains("Specular".ToLower()))
            {
                Debuger.LogErrorFormat(nameof(CheckResources),"Legacy Shaders_Bumped Specular Shader: " + mat.name + "   path: " + matPath, mat);
                mat.shader = Shader.Find("PK2/Bumped Specular");
            }
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/AssetCheck/Default-Material")]
    public static void CheckDefault_Material()
    {
        var defaultMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/ArtResources/DefaultMaterial.mat");
        string[] ids = AssetDatabase.FindAssets("t:prefab");
        Debuger.Log("Materials count: " + ids.Length);
        for (int i = 0; i < ids.Length; i++)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(ids[i]);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(matPath);
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0)
                continue;
            for (int j = 0; j < renderers.Length; j++)
            {
                if (renderers[j].sharedMaterial == null || renderers[j].sharedMaterial.name == "Default-Material")
                {
                    Debuger.LogErrorFormat(nameof(CheckResources),"Default-Material: " + go.name, go);
                    renderers[j].sharedMaterial = defaultMat;
                }
            }
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/AssetCheck/SpriteSizeGreater512")]
    public static void SpriteSizeGreater1024()
    {
        string[] ids = AssetDatabase.FindAssets("t:sprite");
        Debuger.Log("sprite count: " + ids.Length);
        for (int i = 0; i < ids.Length; i++)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(ids[i]);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite.rect.size.x >= 512 || sprite.rect.size.y >= 512)
            {
                Debuger.LogErrorFormat(nameof(CheckResources),"sprite size >= 512: " + sprite.name + "   path: " + spritePath, sprite);
            }
        }
    }

    [MenuItem("Tools/AssetCheck/Test Mode")]
    public static void TestMode()
    {


        {
            string[] ids = AssetDatabase.FindAssets("t:sprite");
            Debuger.Log("sprite count: " + ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                string spritePath = AssetDatabase.GUIDToAssetPath(ids[i]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                TextureImporter texIm = (TextureImporter)AssetImporter.GetAtPath(spritePath);

                {
                    texIm.maxTextureSize = 8;
                    AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
                }
            }
        }
        {
            string[] ids = AssetDatabase.FindAssets("t:texture");
            Debuger.Log("sprite count: " + ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                string spritePath = AssetDatabase.GUIDToAssetPath(ids[i]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                TextureImporter texIm = (TextureImporter)AssetImporter.GetAtPath(spritePath);

                {
                    texIm.maxTextureSize = 8;
                    AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
                }
            }
        }

    }

    [MenuItem("Tools/AssetCheck/SpriteMipmapOrRW")]
    public static void SpriteMipmapOrRW()
    {
        string[] ids = AssetDatabase.FindAssets("t:sprite");
        Debuger.Log("sprite count: " + ids.Length);

        for (int i = 0; i < ids.Length; i++)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(ids[i]);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite.texture.mipmapCount > 1)
            {
                Debuger.LogErrorFormat(nameof(CheckResources),"sprite mipmap: " + sprite.name + "   path: " + spritePath, sprite);
            }

            TextureImporter texIm = (TextureImporter)AssetImporter.GetAtPath(spritePath);
            if (texIm.isReadable)
            {
                texIm.isReadable = false;
                AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
                Debuger.LogErrorFormat(nameof(CheckResources),spritePath, sprite.texture);
            }
        }
    }
    [MenuItem("Tools/AssetCheck/PlayerTexMipmapEnable")]
    public static void PlayerTexMipmapEnable()
    {
        string[] ids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/CharacterCustomization/Player/Textures" });

        for (int i = 0; i < ids.Length; i++)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(ids[i]);
            var sprite = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);

            TextureImporter texIm = (TextureImporter)AssetImporter.GetAtPath(spritePath);
            if (texIm.isReadable || !texIm.mipmapEnabled)
            {
                texIm.isReadable = false;
                texIm.mipmapEnabled = true;
                AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
    //[MenuItem("Tools/AssetCheck/MeshRW")]
    //public static void MeshRW()
    //{
    //    string[] ids = AssetDatabase.FindAssets("t:mesh");
    //    Debuger.Log("mesh count: " + ids.Length);

    //    for (int i = 0; i < ids.Length; i++)
    //    {
    //        string meshPath = AssetDatabase.GUIDToAssetPath(ids[i]);
    //        //var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

    //        ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(meshPath);
    //        if (modelImporter.isReadable)
    //        {
    //            modelImporter.isReadable = false;
    //            AssetDatabase.ImportAsset(meshPath, ImportAssetOptions.ForceUpdate);
    //            Debuger.LogError(meshPath);
    //        }
    //    }
    //}

}
