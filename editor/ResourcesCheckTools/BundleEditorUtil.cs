/**********************************************************************************
 * FileName:    BundleEditorUtil.cs
 * Description: 资源管理框架---编辑器 工具接口
 * History: 2019-07-09
 *********************************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using System;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Engine.Asset
{
    public static class BundleEditorUtil
    {
        /// <summary>
        /// 获取当前选中的Asset文件夹
        /// </summary>
        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        /// <summary>
        /// 获取当前选中的Asset资源
        /// </summary>
        public static string GetSelectedAssetPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
            }
            return path;
        }


    }
}
#endif