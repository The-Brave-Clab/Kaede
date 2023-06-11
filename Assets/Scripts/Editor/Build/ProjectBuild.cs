using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.Build;
using Unity.EditorCoroutines.Editor;

namespace Y3ADV.Editor.Build
{
    public static partial class ProjectBuild
    {
        [MenuItem("Y3ADV/Build/WebGL")]
        public static void BuildWebGL()
        {
            var buildConfig = GetBuildConfigurationFromAssetPath("WebGL/WebGLRelease.buildconfiguration");
            PrepareAndBuild(buildConfig);
        }

        [MenuItem("Y3ADV/Build/Windows")]
        public static void BuildWindows()
        {
            var buildConfig = GetBuildConfigurationFromAssetPath("Windows/WindowsReleaseIL2CPP.buildconfiguration");
            PrepareAndBuild(buildConfig);
        }

        private static void PrepareAndBuild(BuildConfiguration buildConfig)
        {
            if (buildConfig == null) return;
            if (!buildConfig.CanBuild())
            {
                var prepareResult = buildConfig.Prepare();
                if (prepareResult.Failed)
                {
                    Debug.LogError($"Failed to prepare build configuration: {prepareResult.Message}");
                }
                else
                {
                    BuildSingleton.BuildConfig = buildConfig;
                }

                return;
            }
            
            Build(buildConfig);
        }

        public static void Build(BuildConfiguration buildConfig)
        {
            var buildResult = buildConfig.Build();
            if (buildResult.Failed)
            {
                Debug.LogError($"Failed to build project: {buildResult.Message}");
                return;
            }

            Debug.Log($"Build {buildConfig.name} completed successfully.\nOutput path: {buildConfig.GetBuildPipeline().GetOutputBuildDirectory(buildConfig).ToHyperLink()}");

            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
                EditorApplication.Exit(0);
        }
        
        private static BuildConfiguration GetBuildConfigurationFromAssetPath(string buildConfigPath)
        {
            var assetPath = $"Assets/BuildConfigs/{buildConfigPath}";
            var buildConfiguration = BuildConfiguration.LoadAsset(assetPath);
            if (buildConfiguration != null) return buildConfiguration;
            Debug.LogError($"Failed to load build configuration from asset path '{assetPath}'.");
            return null;
        }
        
        private static string ToHyperLink(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null || !directoryInfo.Exists)
            {
                return string.Empty;
            }

            return $"<a directory=\"{directoryInfo.FullName}\">{directoryInfo.FullName}</a>";
        }
    }
    
    [FilePath("Temp/Y3ADVBuild.yaml", FilePathAttribute.Location.ProjectFolder)]
    public class BuildSingleton : ScriptableSingleton<BuildSingleton>
    {
        [SerializeField]
        private BuildConfiguration buildConfig = null;

        public static BuildConfiguration BuildConfig
        {
            get => instance.buildConfig;
            set
            {
                instance.buildConfig = value;
                instance.Save(true);
            }
        }

        [InitializeOnLoadMethod]
        public static void Build()
        {
            if (instance == null) return;
            if (BuildConfig == null) return;

            Debug.Log("Prepared build environment. Build starting after domain reload.");
            EditorCoroutineUtility.StartCoroutineOwnerless(StartBuild());
        }
        
        private static IEnumerator StartBuild()
        {
            yield return null;
            ProjectBuild.Build(BuildConfig);
            BuildConfig = null;
        }
    }
}