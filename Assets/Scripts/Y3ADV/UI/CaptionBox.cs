using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Y3ADV;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class CaptionBox : MonoBehaviour, IStateSavable<CaptionState>
{
    public TextMeshProUGUI text;
    public Image box;

    private RectTransform rt = null;

    private RectTransform rectTransform
    {
        get
        {
            if (rt == null)
                rt = GetComponent<RectTransform>();
            return rt;
        }
    }
    
    void Update()
    {
        Vector2 vector = new Vector2(text.preferredWidth, text.preferredHeight);
        text.rectTransform.sizeDelta = vector;
        rectTransform.sizeDelta = vector * 1.3f;
    }

    public CaptionState GetState()
    {
        return new()
        {
            enabled = gameObject.activeSelf,
            boxColor = box.color,
            text = text.text,
            textAlpha = text.color.a
        };
    }

    public IEnumerator RestoreState(CaptionState state)
    {
        gameObject.SetActive(state.enabled);
        box.color = state.boxColor;
        text.text = state.text;
        var color = text.color;
        color.a = state.textAlpha;
        text.color = color;
        yield break;
    }
}
