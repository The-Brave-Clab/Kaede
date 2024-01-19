using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Y3ADV
{
    public abstract class CommandBase
    {
        protected string[] args = null;
        protected string[] originalArgs = null;

        protected Y3ScriptModule scriptModule = null;

        public abstract bool SyncExecution
        {
            get;
        }

        public virtual bool ImmediateExecution => false;

        public CommandBase(Y3ScriptModule module, string[] arguments)
        {
            scriptModule = module;
            args = arguments;
            originalArgs = new string[arguments.Length];
            Array.Copy(arguments, originalArgs, arguments.Length);

            for (int i = 0; i < args.Length; i++)
            {
                string resolved = Y3ScriptModule.ResolveAlias(args[i]);
                if (resolved != null) 
                    args[i] = resolved;
            }
        }

        public override string ToString()
        {
            string result = "";
            foreach (var s in args)
            {
                result += s + "\t";
            }

            return result.Trim();
        }

        protected T Arg<T>(int index, T defaultValue = default)
        {
            try
            {
                if (index >= args.Length) return defaultValue;
                if (typeof(T) == typeof(string)) return (T)(object)args[index];
                if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(args[index]);
                if (typeof(T) == typeof(Ease)) return (T)(object)Utils.GetEase(args[index]);
                return scriptModule.Evaluate<T>(args[index]);
            }
            catch (Exception e)
            {
                Debug.LogError($"Cannot parse Arg[{index}] = {args[index]} as {typeof(T).Name}. Using default value {defaultValue}.\n{e.Message}");
                if (StartupSettings.TestMode)
                    TestManager.Fail(TestManager.FailReason.BadParameter);
                return defaultValue;
            }
        }

        protected static T FindEntity<T>(string name) where T : BaseEntity
        {
            var entities = Object.FindObjectsOfType<T>();
            var substituteName =
                Utils.FindClosestMatch(name, entities.Select(e => e.gameObject.name), out var distance);
            var result = entities.First(e => e.gameObject.name == substituteName);
            if (distance != 0)
                Debug.LogWarning($"{typeof(T).Name} '{name}' doesn't exist, using '{substituteName}' instead. Distance is {distance}.");
            return result;
        }

        public virtual IEnumerator MustWait()
        {
            yield return null;
        }
        public abstract IEnumerator Execute();

        public virtual void DryRun(ref ScenarioSyncPoint state)
        {
            // Does nothing by default
        }
    }
}
