/**********************************************************************************
 * FileName:    BundleMenuTools.cs
 * Description: 资源打包编辑器工具 目录
 * History: 2019-07-09
 *********************************************************************************/
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

namespace Engine.Asset
{
    public static class BundleMenuTools
    {
        #region 资源规范
        
        [MenuItem("Assets/AssetBundle工具/资源规范/查找引用的资源", false, 2)]
        public static void CheckOnAssetRef()
        {
            CheckAssetReferencesWin.CheckOnAssetRefs();
        }

        [MenuItem("Assets/AssetBundle工具/资源规范/查找依赖资源", false, 3)]
        public static void Test()
        {
            CheckAssetDependenciesWin.CheckOnAssetRefs();
            Resources.Load<Sprite>("");
        }
        
        #endregion
    }
}
#endif
