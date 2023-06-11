using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace Y3ADV
{
    public partial class GameManager : SingletonMonoBehaviour<GameManager>
    {
        private string scriptName;

        public static string ScriptName
        {
            get => Instance.scriptName;
            set
            {
                Instance.hasPlayedOnce = Instance.hasPlayedOnce && Instance.scriptName == value;
                Instance.scriptName = value;
            }
        }

        public LoadingProgressBars loadingProgressBars;

        private Queue<IEnumerator> coroutineQueue;
        private List<bool> coroutineRunning;
        
        public static int JobCount =>
#if WEBGL_BUILD
            8;
#else
            Math.Min(8, SystemInfo.processorCount);
#endif

        private bool hasPlayedOnce;

        public static bool CanPlay =>
#if WEBGL_BUILD
            Instance.hasPlayedOnce;
#else
            true;
#endif

        public static string GetScenarioPath()
        {
            return $"adv/{ScriptName}";
        }

        public static string GetScenarioFile(string scenarioName, string scenarioFileType)
        {
            return $"adv/{scenarioName}/{scenarioName}_{scenarioFileType}.txt";
        }

        public static void RegisterLoading(string name, Func<float> getProgress)
        {
            if (Instance.loadingProgressBars == null) return;
            Instance.loadingProgressBars.Add(name, getProgress);
        }

        public static void UnregisterLoading(string name)
        {
            if (Instance.loadingProgressBars == null) return;
            Instance.loadingProgressBars.Delete(name);
        }

        protected override void Awake()
        {
            base.Awake();
            
            scriptName = "";
            loadingProgressBars = null;
            coroutineQueue = new Queue<IEnumerator>();

            hasPlayedOnce = false;

            LoadFinishedScenario();
            InitializeAWS();

            LocalizationSettings.SelectedLocaleChanged += locale => Debug.Log($"Changed locale to {locale.LocaleName}");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            SceneManager.sceneUnloaded += RemoveAll;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= RemoveAll;
        }

        private static void RemoveAll(Scene sceneToUnload)
        {
            if (sceneToUnload.name == "MainScene")
            {
                LocalResourceManager.RemoveScriptLoadedAsset(Instance.scriptName);

                Resources.UnloadUnusedAssets();
            }
        }

        private void Start()
        {
            Debug.Log($"Opening {JobCount} coroutine coordinators");
            coroutineRunning = new List<bool>(JobCount);
            for (int i = 0; i < JobCount; ++i)
            {
                coroutineRunning.Add(false);
                StartCoroutine(CoroutineCoordinator(i));
            }

            SetPresentationParams();
        }

        private void SetPresentationParams()
        {
            Application.targetFrameRate = -1;
#if UNITY_IOS
            UnityEngine.iOS.Device.hideHomeButton = false;
            UnityEngine.iOS.Device.deferSystemGesturesMode = UnityEngine.iOS.SystemGestureDeferMode.All;
#endif
        }

        IEnumerator CoroutineCoordinator(int id)
        {
            while (true)
            {
                coroutineRunning[id] = false;
                while (coroutineQueue.Count > 0)
                {
                    coroutineRunning[id] = true;
                    yield return StartCoroutine(coroutineQueue.Dequeue());
                }
                yield return null;
            }
        }

        public static void AddCoroutine(IEnumerator coroutine)
        {
            Instance.coroutineQueue.Enqueue(coroutine);
        }

        public static bool CoroutineQueueEmpty()
        {
            return Instance.coroutineQueue.Count == 0;
        }

        public static bool AllCoroutineFinished()
        {
            return CoroutineQueueEmpty() && Instance.coroutineRunning.All(f => !f);
        }

        public static void StartScenario(string scenarioId)
        {
            ScriptName = scenarioId;
            LoadSceneMode mode =
#if WEBGL_BUILD
                LoadSceneMode.Single;
#else
                LoadSceneMode.Additive;
#endif
            SceneManager.LoadScene("MainScene", mode);

#if UNITY_EDITOR
            ClearUnityConsole();
#endif
        }

#if UNITY_EDITOR
        private static System.Reflection.MethodInfo clearMethod = null;
        public static void ClearUnityConsole()
        {
            if (clearMethod == null)
            {
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.SceneView));
                var logEntriesType = assembly.GetType("UnityEditor.LogEntries");
                clearMethod = logEntriesType.GetMethod("Clear",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            }
            clearMethod?.Invoke(null, null);
        }
#endif

        public static void Play()
        {
            Instance.hasPlayedOnce = true;
        }

        public static void ResetPlay()
        {
            Instance.hasPlayedOnce = false;
        }
    }
}