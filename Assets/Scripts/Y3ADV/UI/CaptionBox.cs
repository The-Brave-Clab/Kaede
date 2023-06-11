using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class CaptionBox : MonoBehaviour
{
    public TextMeshProUGUI text;

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
}
