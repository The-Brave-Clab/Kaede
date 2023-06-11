using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace Y3ADV
{
    public class WebGLInterops : MonoBehaviour
    {
#if WEBGL_BUILD
        [DllImport("__Internal")]
        public static extern void RegisterWebGLInteropGameObject(string gameObjectName);

        [DllImport("__Internal")]
        public static extern void RegisterSoundManagerGameObject(string gameObjectName);

        [DllImport("__Internal")]
        public static extern void RegisterUIManagerGameObject(string gameObjectName);

        [DllImport("__Internal")]
        public static extern void RegisterFullscreenManagerGameObject(string gameObjectName);

        [DllImport("__Internal")]
        public static extern void RegisterInterops();

        [DllImport("__Internal")]
        public static extern void OnScriptLoaded(string script);

        [DllImport("__Internal")]
        public static extern void OnMessageCommand(string speaker, string voiceId, string message);

        [DllImport("__Internal")]
        public static extern void OnScenarioStarted();

        [DllImport("__Internal")]
        public static extern void OnScenarioFinished();

        [DllImport("__Internal")]
        public static extern void OnExitFullscreen();

        [DllImport("__Internal")]
        public static extern void OnToggleAutoMode(int on);

        [DllImport("__Internal")]
        public static extern void OnToggleDramaMode(int on);

        private IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;

            RegisterWebGLInteropGameObject(gameObject.name);
            DontDestroyOnLoad(gameObject);

            RegisterInterops();
        }

        public void ResetPlayer(string unifiedName)
        {
            var split = unifiedName.Split(':');
            string scriptName = split[0];
            string languageCode = split[1];

            GameManager.ScriptName = scriptName;
            if (GameManager.ScriptName == "") return;
            LocalizationSettings.Instance.SetSelectedLocale(LocalizationSettings.AvailableLocales.Locales
                .Find(l => l.Identifier.CultureInfo.TwoLetterISOLanguageName == languageCode));
            GameManager.ResetPlay();
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }

        public void DisableInput()
        {
            WebGLInput.captureAllKeyboardInput = false;
        }

        public void EnableInput()
        {
            WebGLInput.captureAllKeyboardInput = true;
        }
#endif
    }
}