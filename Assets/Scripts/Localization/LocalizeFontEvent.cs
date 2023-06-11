using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace Y3ADV
{
    [System.Serializable]
    public class LocalizedFont : LocalizedAsset<TMP_FontAsset> {}
 
    [System.Serializable]
    public class UpdateFontEvent : UnityEvent<TMP_FontAsset>{}
 
    public class LocalizeFontEvent :  LocalizedAssetEvent<TMP_FontAsset, LocalizedFont, UpdateFontEvent>
    {
    }
}