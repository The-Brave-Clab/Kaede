using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Y3ADV
{
    public static class Utils
    {
        #region CoroutineHelpers

        public static void ForceImmediateExecution(this IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
            }
        }

        #endregion
        
        #region ExceptionHandling
        public static IEnumerator WithException(this IEnumerator enumerator)
        {
            while (true)
            {
                object current;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (Exception ex)
                {
                    SendBugNotification(ex.ToString());
                    yield break;
                }
                yield return current;
            }
        }

        public static void SendBugNotification(string what)
        {
            Debug.LogError($"{GameManager.ScriptName}: {what}\n\t{Y3ScriptModule.InstanceInScene.CurrentStatement}");
            Dictionary<string, string> postData = new Dictionary<string, string>
            {
                ["value1"] = GameManager.ScriptName,
                ["value2"] = $"{what} at {Y3ScriptModule.InstanceInScene.CurrentStatement}"
            };

            GameManager.Instance.StartCoroutine(SendBugWebRequest(postData));
        }

        private static IEnumerator SendBugWebRequest(Dictionary<string, string> postData)
        {
            string postString = JsonConvert.SerializeObject(postData);
            UnityWebRequest notification = UnityWebRequest.PostWwwForm(
                "https://maker.ifttt.com/trigger/Y3ADV_Exception/with/key/dNb3noBoVzZJljM7JIhoBf",
                postString);
            notification.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(postString));
            notification.uploadHandler.contentType = "application/json";
            yield return notification.SendWebRequest();
        }
        #endregion

        #region DOTween
        public static Ease GetEase(string easeName)
        {
            return Enum.TryParse<Ease>(easeName, true, out var value) ? value : Ease.Linear;
        }
        #endregion

        #region LevenshteinDistance

        public static string FindClosestMatch(string input, IEnumerable<string> dictionary, out int distance)
        {
            distance = int.MaxValue;
            var closestMatch = "";

            foreach (var word in dictionary.OrderBy(w => w))
            {
                var currentDistance = CalculateLevenshteinDistance(input, word);
                if (currentDistance >= distance) continue;
                distance = currentDistance;
                closestMatch = word;
            }

            return closestMatch;
        }

        private static int CalculateLevenshteinDistance(string source, string target)
        {
            var n = source.Length;
            var m = target.Length;
            var distance = new int[n + 1][];

            for (var i = 0; i <= n; ++i)
            {
                distance[i] = new int[m + 1];
                distance[i][0] = i;
            }

            for (var j = 0; j <= m; ++j)
                distance[0][j] = j;

            for (var i = 1; i <= n; ++i)
            {
                for (var j = 1; j <= m; ++j)
                {
                    var cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i][j] = Math.Min(Math.Min(distance[i - 1][j] + 1, distance[i][j - 1] + 1), distance[i - 1][j - 1] + cost);
                }
            }

            return distance[n][m];
        }

        #endregion
    }
}