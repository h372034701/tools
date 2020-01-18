/**********************************************************************************
 * FileName:    AssetProcessTrigger.cs
 * Description: 资源管理框架---资源变更触发器
 *              1 编辑器资源导入监听器
 *              2 游戏有且只有一个 资源导入处理脚本，便于统一处理
 * History: 2019-07-09
 *********************************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using System;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Engine.Asset
{
    public class AssetProcessTrigger : AssetPostprocessor
    {
        protected virtual Material OnAssignMaterialModel(Material previousMaterial, Renderer renderer)
        {
            return ResourceChecker.OnAssignMaterialModel(previousMaterial, renderer);
        }

        //模型导入之前调用  
        public void OnPreprocessModel()
        {
            ResourceChecker.OnPreprocessModel(assetPath, assetImporter);
        }

        //模型导入之前调用  
        public void OnPostprocessModel(GameObject go)
        {
            ResourceChecker.OnPostprocessModel(assetPath, assetImporter, go);
        }

        //纹理导入之前调用，针对入到的纹理进行设置  
        public void OnPreprocessTexture()
        {
            ResourceChecker.OnPreprocessTexture(assetPath, assetImporter);
        }

        //文理导入之后
        public void OnPostprocessTexture(Texture2D tex)
        {
            ResourceChecker.OnPostprocessTexture(assetPath, assetImporter, tex);
        }

        //音频导入之前
        public void OnPreprocessAudio()
        {
            ResourceChecker.OnPreprocessAudio(assetPath, assetImporter);
        }

        //音频导入之后
        public void OnPostprocessAudio(AudioClip clip)
        {
            ResourceChecker.OnPostprocessAudio(assetPath, assetImporter, clip);
        }
        

        #region 监听所有资源变更 并做预处理

        static List<PostAssetInfo> fileAssets = new List<PostAssetInfo>();
        static List<PostAssetInfo> floderAssets = new List<PostAssetInfo>();

        //所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的  
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < importedAsset.Length; i++)
            {
                Debug.Log($"{importedAsset[i]}");
            }

            fileAssets.Clear();
            floderAssets.Clear();

            CheckAssetPathArray(importedAsset, "importedAsset", true);
            CheckAssetPathArray(deletedAssets, "deletedAssets", false);
            CheckAssetPathArray(movedAssets, "movedAssets", true);
            CheckAssetPathArray(movedFromAssetPaths, "movedFromAssetPaths", false);
            
            //通知资源检查工具。 
            //1 检查资源格式
            //2 检查资源目录结构
            //3 自定更新文件打包策略
            ResourceChecker.OnPostprocessAllAssets(fileAssets, floderAssets);
            
        }

        private static void CheckAssetPathArray(string[] assetPathArray, string options, bool isNewAsset)
        {
            foreach (var assetPath in assetPathArray)
            {
                var assetInfo = new PostAssetInfo();
                assetInfo.assetPath = assetPath;
                assetInfo.isNewAsset = isNewAsset;

                int indexOf = assetPath.IndexOf('.');
                if (indexOf > 0)
                {
                    fileAssets.Add(assetInfo);
                    //Debug.LogFormat(" {0} a asset = {1} !", options, assetPath);
                }
                else
                {
                    floderAssets.Add(assetInfo);
                    //Debug.LogFormat(" {0} a floder = {1} !", options, assetPath);
                }
            }
        }


        #endregion
    }

    public struct PostAssetInfo : IComparable<PostAssetInfo>
    {
        public string assetPath;
        public bool isNewAsset;

        public int CompareTo(PostAssetInfo other)
        {
            if (isNewAsset == other.isNewAsset)
            {
                int length = assetPath.Length;
                int otherLength = other.assetPath.Length;
                if (length > otherLength) return 1;
                else if (length < otherLength) return -1;
                else return 0;
            }
            return isNewAsset ? 1 : -1;
        }
    }
}

#endif