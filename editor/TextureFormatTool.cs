using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureAutoSet : EditorWindow {

    [MenuItem("Assets/*****设置文件夹以及子文件夹下面的图片压缩格式为ASTC", priority = 0)]
    static void AutoSetASTC()
    {
        string[] guidArray = Selection.assetGUIDs;
        foreach (var item in guidArray)
        {
            string selectFloder = AssetDatabase.GUIDToAssetPath(item);
            DirectoryInfo root = new DirectoryInfo(selectFloder);

            string[] ids = AssetDatabase.FindAssets("t:Texture", new[] {selectFloder});

            for (int i = 0; i < ids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(ids[i]);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                
                if (importer == null)
                {
                    Debug.LogWarning(assetPath);
                    continue;
                }
                
                if(importer.textureType != TextureImporterType.Sprite)
                    continue;
                bool changed = false;
                if (importer.mipmapEnabled == true)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }
                //判断图片大小
//                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
//                int textureSize = Mathf.Max(texture.height, texture.width);
//                //Debug.Log(textureSize);
//                int SizeType = FitSize(textureSize);
//        
                int size = importer.GetDefaultPlatformTextureSettings().maxTextureSize;
                //Android   iPhone
                if(importer.DoesSourceTextureHaveAlpha())
                {
                    //ios版本
                    TextureImporterPlatformSettings setting = importer.GetPlatformTextureSettings("iPhone");
                    if (setting.format != TextureImporterFormat.ASTC_RGBA_8x8 || setting.maxTextureSize != size)
                    {
                        changed = true;
                        importer.SetPlatformTextureSettings("iPhone", size, TextureImporterFormat.ASTC_RGBA_8x8);
                    }
                    //安卓版本
                    importer.SetPlatformTextureSettings("Android", SizeType, TextureImporterFormat.ETC2_RGBA8);
                }
                else
                {
                    //ios版本
                    TextureImporterPlatformSettings setting = importer.GetPlatformTextureSettings("iPhone");
                    if (setting.format != TextureImporterFormat.ASTC_RGB_8x8 || setting.maxTextureSize != size)
                    {
                        changed = true;
                        importer.SetPlatformTextureSettings("iPhone", size, TextureImporterFormat.ASTC_RGB_8x8);
                    }
                    //安卓版本
                    importer.SetPlatformTextureSettings("Android", SizeType, TextureImporterFormat.ETC2_RGB4);
                }
                if(changed)
                    importer.SaveAndReimport();
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void GetFloder(DirectoryInfo root)
    {
        GetFile(root);
        //查找子文件夹
        DirectoryInfo[] array = root.GetDirectories();
        //Debug.Log(root);
        foreach (DirectoryInfo item in array)
        {
            GetFloder(item);
        }
    }

    static void GetFile(DirectoryInfo root)
    {
        //DirectoryInfo root = new DirectoryInfo(path);
        FileInfo[] fileDic = root.GetFiles();
        foreach (var file in fileDic)
        {
            //sDebug.Log(file);
            if (file.FullName.EndsWith(".png") || file.FullName.EndsWith(".jpg") || file.FullName.EndsWith(".tga") ||
                file.FullName.EndsWith(".psd") || file.FullName.EndsWith(".PSD") || file.FullName.EndsWith(".exr") ||
                file.FullName.EndsWith(".tif"))
            {
                //Debug.Log("-------------" + file.FullName);
                //Debug.Log(Application.dataPath);
                SetPicFormat(file.FullName.Replace(Application.dataPath.Replace("Assets",""),""));
            }
        }
    }

    static void SetPicFormat(string path)
    {
        Debug.Log(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer.mipmapEnabled == true)
        {
            importer.mipmapEnabled = false;
        }
        
        //判断图片大小
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        int textureSize = Mathf.Max(texture.height, texture.width);
        //Debug.Log(textureSize);
        int SizeType = FitSize(textureSize);
        
        //Android   iPhone
        if(importer.DoesSourceTextureHaveAlpha())
        {
            //ios版本
            importer.SetPlatformTextureSettings("iPhone", SizeType, TextureImporterFormat.ASTC_RGBA_6x6);
            //安卓版本
            //importer.SetPlatformTextureSettings("Android", SizeType, TextureImporterFormat.ETC2_RGBA8);
        }
        else
        {
            //ios版本
            importer.SetPlatformTextureSettings("iPhone", SizeType, TextureImporterFormat.ASTC_RGB_6x6);
            //安卓版本
            //importer.SetPlatformTextureSettings("Android", SizeType, TextureImporterFormat.ETC2_RGB4);
        }
    }
    
    static int[] formatSize = new int[]{32,64,128,256,512,1024,2048,4096};
    static int FitSize(int picValue)
    {
        foreach (var one in formatSize)
        {
            if (picValue <= one)
            {
                return one;
            }
        }

        return 1024;
    }
}