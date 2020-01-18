/**********************************************************************************
 * FileName:    AssetBundleUtility.cs
 * Description: 资源管理框架---资源目录工具类
 * History: 2019-07-09
 *********************************************************************************/
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Asset
{
    public static class AssetBundleUtility
    {

        public const string AssetBundleVariant = "";

        public const string AssetBundleManifest = "AssetBundleManifest";

        public const string GameAssets = "GameAssets/Common";

        public const string VersionSettings = "hotfix_ver.txt";

        public const string AssetGameAssets = "Assets/";

        public const string TempGameAssets = "Assets/GameAssets/Temp";

        public const string AssetPackageSettings = "AssetPackageSettings.asset";

        public const string AssetToBundleMap = "-AssetToBundleMapping";
        

        private static StringBuilder __StringBuilder = new StringBuilder();

        public static string GameAssetInputPath()
        {
            return Path.Combine(Application.dataPath, GameAssets);
        }

        public static string GetAssetBundleOutputPath()
        {
            return (Application.dataPath + "/../BundleBuilds/" + GetPlatformName()).Replace("\\", "/");
        }

        public static string GetAssetVersionStorePath()
        {
            return Application.dataPath + "/../AssetVersionStore";
        }

        public static string GetAssetPackageSettingsPath()
        {
            return Path.Combine(AssetGameAssets+"/",AssetPackageSettings);
        }

        public static string GetAssetToBundleMapAssetPath(string root)
        {
            return Path.Combine(AssetGameAssets, root+ "/" + root + AssetToBundleMap + ".asset");
        }

        public static string GetAssetToBundleMappingPath(string root)
        {
            return Path.Combine(GameAssetInputPath(), root + "/" + root + "_AssetToBundle.asset");
        }

        public static string GetBundleFullPath(string path, string bundleName, bool withExt)
        {
            path = Path.Combine(path, bundleName);
            return path;
        }

        public static string GetAssetVersionControlPath()
        {
            return Application.dataPath + "/../BundleBuilds/VersionControll";
        }

        private static string s_BundleDataPath = string.Empty;
        
        private static string s_BundleApplicationPath = string.Empty;

        public static string GetStreamingAssetDataPath()
        {
            if (string.IsNullOrEmpty(s_BundleApplicationPath))
            {
                s_BundleApplicationPath = Path.Combine(Application.streamingAssetsPath, GameAssets);
            }
            return s_BundleApplicationPath;
        }

        public static string GetStreamingAssetDataPathURL(string fileName)
        {
            __StringBuilder.Clear();

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    __StringBuilder.Append("jar:file://");
                    break;

                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    __StringBuilder.Append("file://");
                    break;
            }
            __StringBuilder.Append(GetStreamingAssetDataPath()).Append("/.").Append(fileName);
            return __StringBuilder.ToString();
        }

        public static string GetAssetDatabasePath(string fullPath)
        {
            int index = fullPath.IndexOf("Assets");
            return fullPath.Substring(index);
        }

        public static string GetPlatformName()
        {
            string platformName;

#if UNITY_ANDROID
            platformName = "Android";
#elif UNITY_IOS
            platformName = "IPhone";
#else
            platformName = "Windows";
#endif
            return platformName;
        }

        public static string GetAssetBundleURL(string rootPath, string bundleName)
        {
            __StringBuilder.Clear();

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    __StringBuilder.Append("jar:file://");
                    break;

                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    __StringBuilder.Append("file://");
                    break;
            }

            __StringBuilder.Append(rootPath).Append("/")
            .Append(bundleName).Append(".").Append(AssetBundleVariant);
            return __StringBuilder.ToString();
        }

        public static string GetRelativePath(string fullPath, string root)
        {
            fullPath = fullPath.Replace("\\", "/");
            int index = fullPath.IndexOf(root) + root.Length;
            if (fullPath.Length > index + 1)
            {
                index += 1;
            }
            return fullPath.Substring(index);
        }

        public static int VersionCompare(string greate, string less)
        {
            long nX = 0, nY = 0;
            long.TryParse(greate.Replace(".", ""), out nX);
            long.TryParse(less.Replace(".", ""), out nY);
            if (nX == nY) return 0;
            return nX > nY ? -1 : 1;
        }

        public static string ConvertToWWWPath(string path)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    if(!path.StartsWith("jar:file://"))
                        path = "jar:file://" + path;
                    break;

                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    if (!path.StartsWith("file://"))
                        path = "file://" + path;
                    break;
            }
            return path;
        }
    }
}
