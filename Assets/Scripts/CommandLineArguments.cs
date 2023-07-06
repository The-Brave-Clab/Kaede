using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Y3ADV
{
    public static class StartupSettings
    {
        private static readonly bool specifiedScenario = false;
        public static bool SpecifiedScenario => specifiedScenario;

        private static readonly string specifiedScenarioName = "";
        public static string SpecifiedScenarioName => specifiedScenarioName;

        private static readonly bool overrideTranslation = false;
        public static bool OverrideTranslation => overrideTranslation;

        private static readonly string overrideTranslationFile = "";
        public static string OverrideTranslationFile => overrideTranslationFile;

        static StartupSettings()
        {
            specifiedScenario = CommandLineArguments.HasArg("scenario");
            if (specifiedScenario)
                specifiedScenarioName = CommandLineArguments.GetArgParam("scenario");

            overrideTranslation = CommandLineArguments.HasArg("override-translation");
            if (overrideTranslation)
                overrideTranslationFile = CommandLineArguments.GetArgParam("override-translation");
        }

        static class CommandLineArguments
        {
            private static string[] args;
            private static Dictionary<string, List<string>> argMap;
            private static readonly char[] prefixes = {'-', '+', '/'};

            private static bool IsArg(string arg)
            {
                return prefixes.Any(arg.StartsWith);
            }

            static CommandLineArguments()
            {
                args = Environment.GetCommandLineArgs();

                argMap = new Dictionary<string, List<string>>();

                int i = 0;
                
                while (i < args.Length)
                {
                    if (IsArg(args[i]))
                    {
                        string currentArg = args[i].TrimStart(prefixes);
                        argMap[currentArg] = new List<string>();
                        while (i < args.Length - 1)
                        {
                            ++i;
                            if (IsArg(args[i]))
                            {
                                --i;
                                break;
                            }
                            argMap[currentArg].Add(args[i]);
                        }
                    }
                    ++i;
                }
            }

            public static bool HasArg(string arg)
            {
                return argMap.ContainsKey(arg);
            }

            public static string GetArgParam(string arg)
            {
                if (!HasArg(arg))
                    return null;
                return argMap[arg].Count == 0 ? null : argMap[arg][0];
            }

            public static string[] GetArgParams(string arg)
            {
                return HasArg(arg) ? argMap[arg].ToArray() : Array.Empty<string>();
            }

            public static void LogParams()
            {
                string output = "";
                foreach (var pair in argMap)
                {
                    output += $"{pair.Key}\n";
                    foreach (var param in pair.Value)
                    {
                        output += $"\t{param}\n";
                    }
                }
                
                Debug.Log(output);
            }
        }
    }
}
