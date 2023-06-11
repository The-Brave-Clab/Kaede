using System;
using System.IO;
using UnityEngine;

namespace Y3ADV
{
    [Serializable]
    public class GameSettings
    {
        private static readonly GameSettings instance;
        
        [SerializeField]
        private bool fixed16By9 = true;
        public static bool Fixed16By9
        {
            get => instance.fixed16By9;
            set
            {
                instance.fixed16By9 = value;
                SaveGameSettings();
            }
        }

#if !WEBGL_BUILD
        private static readonly string savePath = Application.persistentDataPath + "/settings.json";
#endif

        static GameSettings()
        {
            try
            {
#if !WEBGL_BUILD
                if (File.Exists(savePath))
                    instance = JsonUtility.FromJson<GameSettings>(File.ReadAllText(savePath));
                else
#endif
                    instance = new GameSettings();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game settings: {e}");
                instance = new GameSettings();
            }
        }

        private GameSettings() { }
        
        public static void SaveGameSettings()
        {
#if !WEBGL_BUILD
            try
            {
                File.WriteAllText(savePath, JsonUtility.ToJson(instance));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game settings: {e}");
            }
#endif
        }
    }
}