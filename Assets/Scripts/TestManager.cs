using System.Collections;
using UnityEngine;

namespace Y3ADV
{
    public class TestManager : SingletonMonoBehaviour<TestManager>
    {
        public enum FailReason
        {
            Passed,
            BadParameter,
            Exception,
            NotImplemented
        }

        public static void Fail(FailReason reason)
        {
            Debug.LogError($"Test failed: {reason:G}");

            Instance.StartCoroutine(QuitCoroutine(reason));
        }

        private static IEnumerator QuitCoroutine(FailReason reason)
        {
            yield return null;
            Application.Quit((int)reason);
        }
    }
}