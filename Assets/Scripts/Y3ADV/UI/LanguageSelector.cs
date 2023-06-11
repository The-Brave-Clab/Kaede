using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Y3ADV
{
    public class LanguageSelector : MonoBehaviour
    {
        public Button Left;
        public Button Right;
        public RectTransform ContentHolder;
        public TextMeshProUGUI OptionTempalte;
        public int index = 0;
        public float animationDuration = 1.0f;
        
        private int optionCount;
        private float contentWidth;
        private Sequence sequence;

        private List<Locale> allLanguages = null;
        
        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;

            allLanguages = LocalizationSettings.AvailableLocales.Locales;
            optionCount = allLanguages.Count;

            for (int i = 0; i < optionCount; ++i)
            {
                var locale = allLanguages[i];
                var newOption = Instantiate(OptionTempalte.gameObject, OptionTempalte.transform.parent);
                newOption.transform.SetSiblingIndex(i);
                var optionText = newOption.GetComponent<TextMeshProUGUI>();
                optionText.text = locale.Identifier.CultureInfo.NativeName;
                LocalizationSettings.AssetDatabase.GetTable("AssetLocalizationTableCollection", locale)
                    .GetAssetAsync<TMP_FontAsset>("Compact UI Font")
                    .Completed += handle => optionText.font = handle.Result;
                newOption.name = locale.Identifier.CultureInfo.Name;
                newOption.SetActive(true);
            }

            contentWidth = ContentHolder.rect.width;
            index = allLanguages.IndexOf(LocalizationSettings.SelectedLocale);

            Vector2 position = ContentHolder.anchoredPosition;
            position.x = -1 * index * contentWidth;
            ContentHolder.anchoredPosition = position;

            Left.onClick.AddListener(() => { OnSelectButtonClicked(true); });
            Right.onClick.AddListener(() => { OnSelectButtonClicked(false); });
        }

        void OnSelectButtonClicked(bool left)
        {
            sequence?.Kill();

            var oldIndex = index;
            index += left ? -1 : 1;
            while (index < 0) index += optionCount;
            index %= optionCount;

            if (oldIndex == index) return;

            sequence = DOTween.Sequence();

            Vector2 originalPosition = ContentHolder.anchoredPosition;
            Vector2 targetPosition = new Vector2(-1 * index * contentWidth, originalPosition.y);
            
            sequence.Append(DOVirtual.Vector3(originalPosition, targetPosition, animationDuration,
                value =>
                {
                    if (ContentHolder != null)
                        ContentHolder.anchoredPosition = value;
                }));
            sequence.onKill += () => { sequence = null; };
            sequence.onComplete += () => { sequence = null; };
            sequence.Play();

            LocalizationSettings.Instance.SetSelectedLocale(allLanguages[index]);
        }
    }
}
