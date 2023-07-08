using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Y3ADV
{
    public static class StartupSettings
    {
        public static bool SpecifiedScenario => specifiedScenarioName != null;

        private static readonly string specifiedScenarioName = null;
        public static string SpecifiedScenarioName => specifiedScenarioName;

        public static bool OverrideTranslation => overrideTranslationFile != null;

        private static readonly string overrideTranslationFile = null;
        public static string OverrideTranslationFile => overrideTranslationFile;

        private static readonly bool testMode = false;
        public static bool TestMode => testMode;

        private static readonly bool batchMode = false;
        public static bool BatchMode => batchMode;
        
        public static bool OverrideLoadPath => overrideLoadPathUri != null;
        
        private static readonly Uri overrideLoadPathUri = null;
        public static Uri OverrideLoadPathUri => overrideLoadPathUri;

        private const string SCENARIO_ARG = "scenario";
        private const string OVERRIDE_TRANSLATION_ARG = "override-translation";
        private const string TEST_MODE_ARG = "test-mode";
        private const string BATCH_MODE_ARG = "batchmode";
        private const string OVERRIDE_LOAD_PATH_ARG = "override-load-path";

        static StartupSettings()
        {
            #if !WEBGL_BUILD

            InitializeCommandLineArguments();

            if (HasArg(SCENARIO_ARG))
                specifiedScenarioName = GetArgParam(SCENARIO_ARG);

            if (HasArg(OVERRIDE_TRANSLATION_ARG))
                overrideTranslationFile = GetArgParam(OVERRIDE_TRANSLATION_ARG);

            if (HasArg(OVERRIDE_LOAD_PATH_ARG))
            {
                var localPath = GetArgParam(OVERRIDE_LOAD_PATH_ARG);
                if (Directory.Exists(localPath))
                {
                    overrideLoadPathUri = new Uri(localPath);
                }
                else
                {
                    Debug.LogError($"Invalid override load path: {localPath}");
                    overrideLoadPathUri = null;
                }
            }

            testMode = HasArg(TEST_MODE_ARG);
            batchMode = HasArg(BATCH_MODE_ARG);

            if (testMode && !SpecifiedScenario)
            {
                Debug.LogError("Can't enter test mode without specifying a scenario!");
                Application.Quit(-1);
            }

            if (batchMode && !testMode)
            {
                Debug.LogError("Can't enter batch mode without specifying test mode!");
                Application.Quit(-1);
            }

            if (testMode && !OverrideLoadPath)
            {
                Debug.LogError("Can't enter test mode without specifying an override local load path!");
                Application.Quit(-1);
            }

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
                    string currentArg = args[i].TrimStart(prefixes).ToLowerInvariant();
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
            return argMap.ContainsKey(arg.ToLowerInvariant());
        }

        static string GetArgParam(string arg)
        {
            var argLower = arg.ToLowerInvariant();
            if (!HasArg(argLower))
                return null;
            return argMap[argLower].Count == 0 ? null : argMap[argLower][0];
        }

        static string[] GetArgParams(string arg)
        {
            var argLower = arg.ToLowerInvariant();
            return HasArg(argLower) ? argMap[argLower].ToArray() : Array.Empty<string>();
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