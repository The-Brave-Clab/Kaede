using UnityEngine;

[ExecuteAlways]
public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform progressBarFill;
    [SerializeField] private float padding = 2;
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 1f;
    public float value;
    
    private RectTransform rectTransform;

    private void Update()
    {
        if (progressBarFill == null) return;
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        var backgroundSize = rectTransform.sizeDelta;
        padding = Mathf.Clamp(padding, 0, backgroundSize.y / 2);
        value = Mathf.Clamp(value, minValue, maxValue);
        progressBarFill.offsetMin = Vector2.one * padding;
        progressBarFill.offsetMax = new Vector2(Mathf.Lerp(padding - backgroundSize.x + 12, -padding, Mathf.InverseLerp(minValue, maxValue, value)), -padding);
    }
}