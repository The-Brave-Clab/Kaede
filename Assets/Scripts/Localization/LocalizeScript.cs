using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Y3ADV
{
    public static class LocalizeScript
    {
        public static string ApplyTranslation(string script, string translationJson)
        {
            TranslationJson translation = JsonUtility.FromJson<TranslationJson>(translationJson);

            var scriptText = script.Replace("\r\n", "\n").Replace("\n\r", "\n");
            var scriptLines = scriptText.Split("\n");
            var scriptName = translation.name;

            var translatedScript = $"// Translated language: {translation.language}\n";

            int captionIndex = 0;
            int messageIndex = 0;
            
            foreach (var line in scriptLines)
            {
                string newLine = line;
                var args = line.Split("\t");
                if (args[0] == "caption")
                {
                    var lineId = $"{scriptName}_c_{captionIndex:00}";
                    ++captionIndex;
                    var captionLine = translation.lines.FirstOrDefault(l => l.id == lineId);
                    if (captionLine != null)
                    {
                        var translatedCaption = captionLine.text[0];

                        var captionArg = args[1];
                        var captionArgSplit = captionArg.Split(':');
                        captionArgSplit[1] = translatedCaption;
                        args[1] = string.Join(':', captionArgSplit);
                        newLine = string.Join('\t', args);
                    }
                }
                else if (args[0] == "mes" || args[0] == "mes_auto")
                {
                    var voiceName = args[2];

                    var lineId = $"{scriptName}_m_{voiceName}_{messageIndex:000}";
                    ++messageIndex;
                    var messageLine = translation.lines.FirstOrDefault(l => l.id == lineId);
                    if (messageLine != null)
                    {
                        var translatedName = messageLine.text[0];
                        var translatedMessage = messageLine.text[1].Replace("\n", "\\n");

                        var nameArg = args[1];
                        var nameArgSplit = nameArg.Split(':');
                        var nameIndex = nameArgSplit.Length > 1 ? 1 : 0;
                        nameArgSplit[nameIndex] = translatedName;
                        args[1] = string.Join(':', nameArgSplit);
                        args[3] = translatedMessage;
                        newLine = string.Join('\t', args);
                    }
                }

                translatedScript += newLine + "\n";
            }
            
            return translatedScript;
        }

        [Serializable]
        private class TranslationJson
        {
            public string name;
            public string language;
            public List<Line> lines;

            [Serializable]
            public class Line
            {
                public string type;
                public string id;
                public int index;
                public List<string> text;
            }
        }
    }
}