using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class SpriteAtlasTool
{
    [MenuItem("Assets/**设置UIAtlas--每个文件夹或子文件夹一个Atlas", priority = 1)]
    static void SetAtlasEveryFolderMapOneAtlas()
    {
        string[] guidArray = Selection.assetGUIDs;
        foreach (var item in guidArray)
        {
            string selectFloder = AssetDatabase.GUIDToAssetPath(item);
            DirectoryInfo root = new DirectoryInfo(selectFloder);
            HandleFolder(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
//        if (!isWaiting)
//        {
//            func(atlasPathList);
//        }
    }

    static string assetsPath = Application.dataPath.Replace("/", "\\");

    static string GetAssetsPath(DirectoryInfo path)
    {
        var dir = "Assets" + path.FullName.Replace(assetsPath, "");
        return dir;
    }

    static string GetAssetsPath(FileInfo path)
    {
        var dir = "Assets" + path.FullName.Replace(assetsPath, "");
        return dir;
    }

    static List<Sprite> _sprites = new List<Sprite>(128);

    static void HandleFolder(DirectoryInfo root)
    {
        var files = root.GetFiles().Where(o => o.Extension.ToLower() == ".jpg" || o.Extension.ToLower() == ".png")
            .ToArray();
        var assetPath = GetAssetsPath(root);

        if (files.Length != 0)
        {
            _sprites.Clear();
            foreach (FileInfo file in files)
            {
                var path = $"{GetAssetsPath(file)}";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                
                if (sprite != null)
                {
                    _sprites.Add(sprite);
                    // 
//                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
//                    importer.spritePackingTag = "";
//                    importer.SaveAndReimport();
                }
                else
                {
                    // todo
                    Debug.LogError($"暂不支持把非sprite的texture加入atlas",
                        AssetDatabase.LoadAssetAtPath<Texture>($"{GetAssetsPath(file)}"));
                }
            }

            if (_sprites.Count != 0)
            {
                string atlasPath = assetPath + "/" + root.Name + ".spriteAtlas";

                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                if (atlas == null)
                {
                    atlas = CreateSpriteAtlas(root.Name, atlasPath);
                }

                ResetAtlas(atlas, _sprites.ToArray());
            }
        }

        //查找子文件夹
        DirectoryInfo[] array = root.GetDirectories();
        //Debug.Log(root);
        foreach (DirectoryInfo item in array)
        {
            HandleFolder(item);
        }
    }

    private static SpriteAtlas CreateSpriteAtlas(string name, string atlasPath)
    {
        SpriteAtlas atlas;
        atlas = new SpriteAtlas();
        atlas.name = name;

        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
        platformSettings.name = "iPhone";
        platformSettings.overridden = true;
        platformSettings.format = TextureImporterFormat.ASTC_RGBA_6x6;

        atlas.SetPlatformSettings(platformSettings);

        AssetDatabase.CreateAsset(atlas, atlasPath);
        // todo unity crash

        SetPackingSetting(atlasPath);

        return atlas;
    }

    static void ResetAtlas(SpriteAtlas atlas, Object[] objects)
    {
        var oldSprites = atlas.GetPackables();
        atlas.Remove(oldSprites);

        atlas.Add(objects);
    }

    [MenuItem("Assets/**设置UIAtlas--每个文件夹(包括子文件夹)一个Atlas", priority = 1)]
    static void SetAtlasWholeFolderMapOneAtlas()
    {
        string[] guidArray = Selection.assetGUIDs;
        foreach (var item in guidArray)
        {
            string selectFloder = AssetDatabase.GUIDToAssetPath(item);
            DirectoryInfo root = new DirectoryInfo(selectFloder);
            var assetPath = GetAssetsPath(root);
            string atlasPath = assetPath + "/" + root.Name + ".spriteAtlas";

            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null)
            {
                atlas = CreateSpriteAtlas(root.Name, atlasPath);
            }

            Object folder = AssetDatabase.LoadAssetAtPath(selectFloder, typeof(Object));
            ResetAtlas(atlas, new[] {folder});
        }

        if (EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOn ||
            EditorSettings.spritePackerMode == SpritePackerMode.BuildTimeOnly)
        {
            
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
//        if (!isWaiting)
//        {
//            func();
//        }
    }

//    private static bool isWaiting = false;
//    
//    private static string rootPath = Application.dataPath+"/";
//    
//    private static Func<Task> func = async () =>
//    {
//        isWaiting = true;
//        await Task.Delay(System.TimeSpan.FromSeconds(0.3f));
//        isWaiting = false;
//
//        string[] ids = AssetDatabase.FindAssets("t:SpriteAtlas");
//        Debug.LogError($"{ids.Length}   {rootPath}");
//        for (int i = 0; i < ids.Length; i++)
//        {
//            SetPackingSetting(ids[i]);
//        }
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//    };

    private static void SetPackingSetting(string path)
    {
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
        var packingSetting = atlas.GetPackingSettings();
        packingSetting.padding = 4;
        packingSetting.enableRotation = false;
        packingSetting.enableTightPacking = false;
        atlas.SetPackingSettings(packingSetting);
    }


    private static string TTpath = "Assets/TempCopy";
    private static string prefabPath = "Assets/GameAssets/Common/UI/Common/Prefabs/UIPanels/ui_view_fight.prefab";

    [MenuItem("Tools/CopyPrefab", priority = 1)]
    static void TT()
    {
        ReplaceSprite(prefabPath, TTpath);
    }


    static void ReplaceSprite(string prefabPath, string targetPath)
    {
        var prefabName = Path.GetFileName(prefabPath);
        var originrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
//        AssetDatabase.CopyAsset(prefabPath, targetPath + "/" + prefabName);
//        //AssetDatabase.CreateAsset(originrefab, targetPath + "/" + prefabName);
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath + "/" + prefabName);

        var images = prefab.GetComponentsInChildren<Image>(true);
//        for (int i = 0; i < images.Length; i++)
//        {
//            var image = images[i];
//            if (image.sprite == null)
//                continue;
//            var spritePath = AssetDatabase.GetAssetPath(image.sprite.texture);
//            if (spritePath.ToLower().Contains("builtin"))
//                continue;
//            AssetDatabase.CopyAsset(spritePath,
//                targetPath + "/fight_" + image.sprite.name + Path.GetExtension(spritePath));
//        }
//
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//        
        func(() =>
        {
            for (int i = 0; i < images.Length; i++)
            {
                var image = images[i];
                if (image.sprite == null)
                    continue;
                var spritePath = AssetDatabase.GetAssetPath(image.sprite.texture);
                if (spritePath.ToLower().Contains("builtin"))
                    continue;
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(targetPath + "/fight_" + image.sprite.name +
                                                                     Path.GetExtension(spritePath));
            }
        });
        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }

    private static bool isWaiting = false;

    private static Func<Action , Task> func = async (action) =>
    {
        isWaiting = true;
        await Task.Delay(System.TimeSpan.FromSeconds(1f));
        isWaiting = false;
        action?.Invoke();
        
    };
}


//public class SpritePostProcessor : AssetPostprocessor
//{
//    private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
//    {
//        if (isWaiting)
//            return;
//        else
//        {
//            func();
//        }
//    }
//
//    private static bool isWaiting = false;
//
//    private static Func<Task> func = async () =>
//    {
//        isWaiting = true;
//        await Task.Delay(System.TimeSpan.FromSeconds(0.3f));
//        isWaiting = false;
//        //SpriteAtlasTool.CollectSpriteReferencesToResourcesFolder();
//        // todo 根据文件夹命名规则设置 spriteatlas
//    };
//}