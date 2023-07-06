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

        private const string SCENARIO_ARG = "scenario";
        private const string OVERRIDE_TRANSLATION_ARG = "override-translation";

        static StartupSettings()
        {
            #if !WEBGL_BUILD

            InitializeCommandLineArguments();

            specifiedScenario = HasArg(SCENARIO_ARG);
            if (specifiedScenario)
                specifiedScenarioName = GetArgParam(SCENARIO_ARG);

            overrideTranslation = HasArg(OVERRIDE_TRANSLATION_ARG);
            if (overrideTranslation)
                overrideTranslationFile = GetArgParam(OVERRIDE_TRANSLATION_ARG);

            #endif
        }


        #region CommandLineArguments
        
        #if !WEBGL_BUILD

        private static string[] args;
        private static Dictionary<string, List<string>> argMap;
        private static readonly char[] prefixes = {'-', '+', '/'};

        static bool IsArg(string arg)
        {
            return prefixes.Any(arg.StartsWith);
        }

        static void InitializeCommandLineArguments()
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

        static bool HasArg(string arg)
        {
            return argMap.ContainsKey(arg);
        }

        static string GetArgParam(string arg)
        {
            if (!HasArg(arg))
                return null;
            return argMap[arg].Count == 0 ? null : argMap[arg][0];
        }

        static string[] GetArgParams(string arg)
        {
            return HasArg(arg) ? argMap[arg].ToArray() : Array.Empty<string>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void LogParams()
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
        
        #endif

        #endregion
    }
}