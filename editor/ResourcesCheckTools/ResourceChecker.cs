/**********************************************************************************
 * FileName:    ResourceChecker.cs
 * Description: 资源管理框架---资源导入检查工具
 * 说明：该文件为非打包框架内容，是根据当前项目的需要，自定义的资源格式、目录结构及
 *       AB打包策略。针对新项目可以删除和修改
 * History: 2019-07-09
 *********************************************************************************/


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Engine.Asset
{
    public static class ResourceChecker
    {
        private static Material _modelDefaultMaterial;

        public static Material ModelDefaultMaterial
        {
            get
            {
                if (_modelDefaultMaterial == null)
                {
                    _modelDefaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/modelDefaultMaterial.mat");
                    if (_modelDefaultMaterial == null)
                    {
                        _modelDefaultMaterial = new Material(Shader.Find("Unlit/Color"));
                        AssetDatabase.CreateAsset(_modelDefaultMaterial, "Assets/modelDefaultMaterial.mat");
                        _modelDefaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("modelDefaultMaterial");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }

                return _modelDefaultMaterial;
            }
        }

        public static Material OnAssignMaterialModel(Material previousMaterial, Renderer renderer)
        {
            if (previousMaterial.shader.name.ToLower() == "standard")
            {
                return ModelDefaultMaterial;
            }

            Debug.Log(previousMaterial.name, renderer.gameObject);

            return previousMaterial;
        }

        // 用于暂时处理 一些使用import materials 的fbx 
        static List<string> disableImportMaterialWhiteList = new List<string>()
        {
//            "changjing_3v3",
//            "TreeLog_A-03",
//            "TreePine-01",
//            "TreePine-02",
//            "TreeStump_A-01",
//            "TreeStump_A-03",
//            "1v1JC",
//            "fuwenshi4",
//            "zhanshen_show",
        };

        // 用于暂时处理 一些需要读取mesh数据的fbx 
        static List<string> disableRWMeshWhiteList = new List<string>()
        {
            "603700_1",
            "603710_1",
        };

        //模型导入之前调用  
        public static void OnPreprocessModel(string assetPath, AssetImporter assetImporter)
        {
            //Debug.Log("OnPreprocessModel=" + assetPath);
        }

        //模型导入之前调用  
        public static void OnPostprocessModel(string assetPath, AssetImporter assetImporter, GameObject go)
        {
            var modelImporter = assetImporter as ModelImporter;
            if (modelImporter != null && modelImporter.useFileScale == false)
            {
                modelImporter.useFileScale = true;
            }

            //Debug.Log("OnPostprocessModel=" + go.name);
            if (-1 == disableImportMaterialWhiteList.FindIndex(o => o == go.name))
            {
                if (modelImporter != null && modelImporter.importMaterials)
                {
                    modelImporter.importMaterials = false;
                    //go.GetComponent<Renderer>().s
                    Debug.LogError($"[ResourceChecker] 已关闭 {go.name} Model import materials", go);
                }
            }
            else
            {
                Debug.Log($"{go.name} 处于白名单之中， 不做处理");
            }

            if (-1 == disableRWMeshWhiteList.FindIndex(o => o == go.name))
            {
                if (modelImporter != null && modelImporter.isReadable)
                {
                    modelImporter.isReadable = false;
                }
            }
            else
            {
                Debug.Log($"{go.name} 处于白名单之中， 不做处理");
            }
        }

        //纹理导入之前调用，针对入到的纹理进行设置  
        public static void OnPreprocessTexture(string assetPath, AssetImporter assetImporter)
        {
            Debug.Log("OnPreProcessTexture=" + assetPath);
        }

        //文理导入之后
        public static void OnPostprocessTexture(string assetPath, AssetImporter assetImporter, Texture2D tex)
        {
            Debug.Log("OnPostProcessTexture=" + assetPath);
            /*TextureImporter impor = assetImporter as TextureImporter;
            impor.textureCompression = TextureImporterCompression.Compressed;
            impor.maxTextureSize = 512;
            impor.textureType = TextureImporterType.Sprite;
            //impor.textureFormat = TextureImporterFormat.ETC2_RGB4;
            impor.mipmapEnabled = false;*/
        }

        //音频导入之前
        public static void OnPreprocessAudio(string assetPath, AssetImporter assetImporter)
        {
            AudioImporter audio = assetImporter as AudioImporter;
            audio.forceToMono = true;
        }

        //音频导入之后
        public static void OnPostprocessAudio(string assetPath, AssetImporter assetImporter, AudioClip clip)
        {
        }

        public static void OnPostprocessAllAssets(List<PostAssetInfo> fileList, List<PostAssetInfo> floderList)
        {
        }
    }
}

#endif