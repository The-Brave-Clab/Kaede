using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class NamePanel : MonoBehaviour
{
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

    public TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        rectTransform.sizeDelta = new Vector2(
            Mathf.Max(400, text.preferredWidth + 140f), 
            68f);
    }
}
