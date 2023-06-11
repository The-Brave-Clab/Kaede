using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Y3ADV
{
    public class LoadingProgressBars : MonoBehaviour
    {
        public GameObject ProgressBarPrefab;

        private class Context
        {
            public string name;
            public Func<float> getProgress;
            public GameObject instance;
            public GameObject container;
            public TextMeshProUGUI text;
            public ProgressBar progressBar;
        }

        private List<Context> contexts;

        private void Awake()
        {
            contexts = new List<Context>(GameManager.JobCount);
            for (int i = 0; i < GameManager.JobCount; ++i)
            {
                var instance = Instantiate(ProgressBarPrefab, transform);
                var ctx = new Context
                {
                    name = "",
                    getProgress = null,
                    instance = instance,
                    container = instance.transform.Find("Container").gameObject,
                    text = instance.GetComponentInChildren<TextMeshProUGUI>(),
                    progressBar = instance.GetComponentInChildren<ProgressBar>()
                };
                ctx.container.SetActive(false);
                contexts.Add(ctx);
            }
        }

        private void OnDestroy()
        {
            foreach (var ctx in contexts)
            {
                Destroy(ctx.instance);
            }
        }

        private void OnEnable()
        {
            // register
            GameManager.Instance.loadingProgressBars = this;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null) return;
            // unregister
            if (GameManager.Instance.loadingProgressBars == this)
                GameManager.Instance.loadingProgressBars = null;
        }

        public void Add(string progressName, Func<float> getProgress)
        {
            var firstSlot = contexts.FindIndex(ctx => !ctx.container.activeSelf);
            if (firstSlot < 0) return;

            contexts[firstSlot].name = progressName;
            contexts[firstSlot].getProgress = getProgress;
            contexts[firstSlot].progressBar.value = 0;
            contexts[firstSlot].container.SetActive(true);
        }

        public void Delete(string progressName)
        {
            var index = contexts.FindIndex(ctx => ctx.name == progressName);
            if (index < 0) return;

            contexts[index].name = "";
            contexts[index].getProgress = null;
            contexts[index].container.SetActive(false);
        }

        private void Update()
        {
            foreach (var ctx in contexts)
            {
                if (ctx == null) continue;
                if (ctx.container == null) continue;
                if (!ctx.container.activeSelf) continue;

                ctx.text.text = ctx.name;
                ctx.progressBar.value = ctx.getProgress?.Invoke() ?? 0;
            }
        }
    }

}