using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Y3ADV.Editor.Build
{
    public class BuildAssetBundles
    {
        private const string assetBaseFolder = "Assets/Bundles/live2d";

        private static List<FileInfo> CollectAssets(FileSystemInfo fileSystemInfo)
        {
            List<FileInfo> result = new List<FileInfo>();
            DirectoryInfo directoryInfoObj = fileSystemInfo as DirectoryInfo;
            FileSystemInfo[] fileSystemInfos = directoryInfoObj.GetFileSystemInfos();

            foreach (var fileInfo in fileSystemInfos)
            {
                if (fileInfo is FileInfo fileInfoObj)
                {
                    result.Add(fileInfoObj);
                }
                else
                {
                    result = result.Concat(CollectAssets(fileInfo)).ToList();
                }
            }

            return result;
        }

        private static void SetFileABLabel(FileInfo fileInfoObj, string name)
        {
            string strAssetFilePath = "";

            if (fileInfoObj.Extension == ".meta") return;

            string filePath = fileInfoObj.FullName.Substring(fileInfoObj.FullName.IndexOf("Assets"));
            strAssetFilePath = filePath;

            AssetImporter importer = AssetImporter.GetAtPath(strAssetFilePath);
            importer.SetAssetBundleNameAndVariant(name, "");
        }

        [MenuItem("Y3ADV/Asset Bundles/Tag Bundles")]
        static void TagBundles()
        {
            DirectoryInfo[] live2dPaths = null;

            AssetDatabase.RemoveUnusedAssetBundleNames();

            DirectoryInfo assetBaseInfo = new DirectoryInfo(assetBaseFolder);
            live2dPaths = assetBaseInfo.GetDirectories();

            foreach (var live2dPath in live2dPaths)
            {
                string modelName = live2dPath.Name;
                var assets = CollectAssets(live2dPath);
                foreach (var asset in assets)
                {
                    SetFileABLabel(asset, $"live2d/{modelName}");
                }
            }

            AssetDatabase.Refresh();
        }

        static void BuildAllAssetBundlesToDirectory(BuildTarget buildTarget, string assetBundleDirectory)
        {
            string directory = $"{assetBundleDirectory}/{buildTarget:G}";
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            BuildPipeline.BuildAssetBundles(directory, 
                BuildAssetBundleOptions.None, 
                buildTarget);
        }

        [MenuItem("Y3ADV/Asset Bundles/Build Bundles/Android")]
        static void BuildAllAssetBundlesAndroid()
        {
            BuildAllAssetBundlesToDirectory(BuildTarget.Android, "AssetBundles");
        }

        [MenuItem("Y3ADV/Asset Bundles/Build Bundles/iOS")]
        static void BuildAllAssetBundlesIos()
        {
            BuildAllAssetBundlesToDirectory(BuildTarget.iOS, "AssetBundles");
        }

        [MenuItem("Y3ADV/Asset Bundles/Build Bundles/PC")]
        static void BuildAllAssetBundlesPC()
        {
            BuildAllAssetBundlesToDirectory(BuildTarget.StandaloneWindows64, "AssetBundles");
        }

        [MenuItem("Y3ADV/Asset Bundles/Build Bundles/WebGL")]
        static void BuildAllAssetBundlesWebGL()
        {
            BuildAllAssetBundlesToDirectory(BuildTarget.WebGL, "AssetBundles");
        }
        
        [MenuItem("Y3ADV/Asset Bundles/Tag Bundles", true)]
        [MenuItem("Y3ADV/Asset Bundles/Build Bundles/Android", true)]
        [MenuItem("Y3ADV/Asset Bundles/Build Bundles/iOS", true)]
        [MenuItem("Y3ADV/Asset Bundles/Build Bundles/PC", true)]
        [MenuItem("Y3ADV/Asset Bundles/Build Bundles/WebGL", true)]
        public static bool CanAccessAssetBundles()
        {
            return Directory.Exists(assetBaseFolder);
        }
    }
}