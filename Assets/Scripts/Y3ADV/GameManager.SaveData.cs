using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Y3ADV
{
    public partial class GameManager
    {
        private List<string> finishedScenario;
        private static string saveDataPath => Path.Combine(Application.persistentDataPath, "FinishedScenario.saveDat");

        private void LoadFinishedScenario()
        {
            if (File.Exists(saveDataPath))
            {
                string saveDataContent = File.ReadAllText(saveDataPath);
                finishedScenario = saveDataContent.Split('\n').Select(s => s.Trim().Trim('\n', '\r')).ToList();
            }
            else
            {
                finishedScenario = new List<string>();
            }
        }

        public static void AddFinishedScenario(string scenarioName)
        {
            if (IsScenarioFinished(scenarioName)) return;
            Instance.finishedScenario.Add(scenarioName);
        }

        public static void SaveFinishedScenario()
        {
            string saveDataContent = string.Join("\n", Instance.finishedScenario);
            File.WriteAllText(saveDataPath, saveDataContent);
        }

        public static bool IsScenarioFinished(string scenarioName)
        {
            return Instance.finishedScenario.Contains(scenarioName);
        }
    }
}