using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using live2d;
using live2d.framework;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Y3ADV
{
    public class Y3Live2DManager : SingletonMonoBehaviour<Y3Live2DManager>
    {
        [SerializeField]
        private Transform live2DWrapper = null;
        [SerializeField]
        private Transform live2DLoadedWrapper = null;
        [SerializeField]
        private List<Y3Live2DModelController> allControllers = null;
        [SerializeField]
        private List<Y3Live2DModelController> loadedControllers = null;
        [SerializeField] 
        private GameObject rawImagePrefab = null;

        public bool autoDeleteActor = false;

        public static void AddController(Y3Live2DModelController controller)
        {
            if (Instance.allControllers == null)
                Instance.allControllers = new List<Y3Live2DModelController>();
            
            Instance.allControllers.Add(controller);
        }

        public static void RemoveController(Y3Live2DModelController controller)
        {
            if (Instance == null) return;
            Instance.allControllers?.Remove(controller);
        }

        void Start()
        {
            if (!StartupSettings.BatchMode) Live2D.init();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!StartupSettings.BatchMode) Live2D.dispose();
        }

        private void Update()
        {
            UtSystem.setUserTimeMSec((long)(Time.timeAsDouble * 1000.0f));
        }

        protected override void Awake()
        {
            base.Awake();

            loadedControllers = new List<Y3Live2DModelController>();
        }
        
        public static void Render()
        {
            if (Instance.allControllers == null) return;
            foreach (var controller in Instance.allControllers)
            {
                controller.Render();
            }
        }

        public static void ReorderModels()
        {
            if (Instance == null) return;
            if (Instance.allControllers == null) return;

            Instance.allControllers.Sort((c1, c2) => c1.Layer.CompareTo(c2.Layer));
            foreach (var controller in Instance.allControllers)
            {
                controller.transform.SetSiblingIndex(Instance.allControllers.FindIndex(c => c == controller));
            }
        }

        private static bool IsTargetController(Y3Live2DModelController controller, string modelName)
        {
            return controller != null && controller.modelName == modelName;
        }

        public static Y3Live2DModelController FindController(string modelName)
        {
            if (Instance == null) return null;
            if (Instance.allControllers == null) return null;
            return Instance.allControllers.FirstOrDefault(c => IsTargetController(c, modelName));
        }

        public delegate void ModelLoadedCallback(Y3Live2DModelController controller);
        
        public static IEnumerator ActorSetup(ModelInfo modelInfo, ModelLoadedCallback cb = null)
        {
            void CloneLoadedModel(Y3Live2DModelController controller)
            {
                GameObject loadedObject = controller.gameObject;
                GameObject clonedObject = Instantiate(loadedObject, Instance.live2DWrapper);
                clonedObject.transform.localPosition = Vector3.zero;
                clonedObject.name = controller.modelName;
                clonedObject.SetActive(true);

                Y3Live2DModelController clonedController = clonedObject.GetComponent<Y3Live2DModelController>();
                clonedController.modelName = controller.modelName;
                
                cb?.Invoke(clonedObject.GetComponent<Y3Live2DModelController>());
            }
            yield return LoadModel(modelInfo, CloneLoadedModel);
        }
        
        public static IEnumerator LoadModel(ModelInfo modelInfo, ModelLoadedCallback cb = null)
        {
            Instance.loadedControllers = Instance.loadedControllers.Where(c => c != null).ToList();
            
            if (Instance.loadedControllers.Any(c => IsTargetController(c, modelInfo.Name)))
            {
                cb?.Invoke(Instance.loadedControllers.First(c => IsTargetController(c, modelInfo.Name)));
                yield break;
            }
            
            // Download the corresponding asset bundle
            string platformName = "";
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    platformName = "iOS";
                    break;
                case RuntimePlatform.Android:
                    platformName = "Android";
                    break;
                case RuntimePlatform.WebGLPlayer:
                    platformName = "WebGL";
                    break;
                default:
                    platformName = "StandaloneWindows64";
                    break;
            }

            AssetBundle live2dAssetBundle = null;

            yield return LocalResourceManager.LoadAssetBundleFromFile($"{platformName}/{modelInfo.Path}",
                bundle =>
                {
                    live2dAssetBundle = bundle;
                });

            if (live2dAssetBundle == null)
            {
                Debug.LogError($"Failed to load AssetBundle {modelInfo.Path}!");
                yield break;
            }

            // string jsonText = "";
            // yield return LocalResourceManager.LoadTextFromFile(
            //     new[] {modelInfo.JsonFile}, 
            //     textAsset =>
            //     {
            //         jsonText = textAsset.text;
            //     });
            TextAsset jsonAsset = live2dAssetBundle.LoadAsset<TextAsset>($"assets/bundles/{modelInfo.JsonFile}");
            string jsonText = jsonAsset.text;

            Model jsonObj = JsonConvert.DeserializeObject<Model>(jsonText);
            Debug.Assert(jsonObj != null, "Live2D json file deserialization failed!");

            GameObject model = Instantiate(Instance.rawImagePrefab, Instance.live2DLoadedWrapper);
            model.name = modelInfo.Name + "_loaded";
            model.transform.localPosition = Vector3.zero;
            
            model.SetActive(false);
            
            Y3Live2DModelController modelController = model.AddComponent<Y3Live2DModelController>();
            modelController.modelName = modelInfo.Name;
            
            TextAsset mocAsset = live2dAssetBundle.LoadAsset<TextAsset>($"assets/bundles/{modelInfo.Path}/{jsonObj.model}.bytes");
            modelController.mocFile = mocAsset.bytes;
            // yield return LocalResourceManager.LoadBytesFromFile(
            //     new[] {$"{modelInfo.Path}/{jsonObj.model}"},
            //     bytes =>
            //     {
            //         modelController.mocFile = bytes;
            //     });

            modelController.textureFiles = new Texture2D[jsonObj.textures.Length];

            for (int i = 0; i < jsonObj.textures.Length; ++i)
            {
                // int index = i;
                // yield return LocalResourceManager.LoadTexture2DFromFile(
                //     new[] {$"{modelInfo.Path}/{jsonObj.textures[index]}"},
                //     texture2D =>
                //     {
                //         modelController.textureFiles[index] = texture2D;
                //     });
                modelController.textureFiles[i] = live2dAssetBundle.LoadAsset<Texture2D>($"assets/bundles/{modelInfo.Path}/{jsonObj.textures[i]}");
            }

            List<string> motionFiles = new List<string>(jsonObj.motions.Count);
            List<string> motionNames = new List<string>(jsonObj.motions.Count);
            foreach (var motion in jsonObj.motions)
            {
                foreach (var motionFile in motion.Value)
                {
                    motionFiles.Add(motionFile.file);
                    motionNames.Add(motion.Key);
                }
            }
            modelController.motionFiles = new Y3Live2DModelController.Motion[motionFiles.Count];
            for (int i = 0; i < motionFiles.Count; ++i)
            {
                modelController.motionFiles[i] =
                    new Y3Live2DModelController.Motion
                    {
                        Name = motionNames[i]
                    };
                // int index = i;
                // yield return LocalResourceManager.LoadBytesFromFile(
                //     new[] {$"{modelInfo.Path}/{motionFiles[index]}"},
                //     bytes =>
                //     {
                //         modelController.motionFiles[index].Asset = bytes;
                //     });
                TextAsset mtnAsset = live2dAssetBundle.LoadAsset<TextAsset>($"assets/bundles/{modelInfo.Path}/{motionFiles[i]}.bytes");
                modelController.motionFiles[i].Asset = mtnAsset.bytes;
            }
            if (jsonObj.pose != null && !string.IsNullOrEmpty(jsonObj.pose))
            {
                TextAsset poseAsset = live2dAssetBundle.LoadAsset<TextAsset>($"assets/bundles/{modelInfo.Path}/{jsonObj.pose}");
                modelController.poseFile = poseAsset.text;
            }

            yield return live2dAssetBundle.UnloadAsync(false);

            Instance.loadedControllers.Add(modelController);
            
            cb?.Invoke(modelController);

            yield return null;
        }

        public class ModelInfo
        {
            public string Name = "";
            public string JsonFile = "";
            public string Path = "";
        }
        
        // Keep this the same as the Model json file, don't modify
        private class Model
        {
            public string version { get; set; } = "";
            public string model { get; set; } = "";
            public string[] textures { get; set; } = null;

            public class MotionFile
            {
                public string file { get; set; } = "";
            }

            public Dictionary<string, List<MotionFile>> motions { get; set; } = null;
            public string pose { get; set; } = "";
        }
    }
}
