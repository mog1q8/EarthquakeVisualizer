using UnityEngine;
using TMPro;

/// <summary>
/// 地震情報ポップアップUI（左下固定）
/// </summary>
public class EarthquakePopupUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("Settings")]
    [SerializeField] private Vector2 margin = new Vector2(20, 20); // 左下余白

    void Awake()
    {
        Hide();
    }

    public void Show(EarthquakeEvent ev, Vector3 worldPosition)
    {
        if (root == null) return;

        // ★ 左下固定位置
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.zero;
        root.pivot = Vector2.zero;

        root.anchoredPosition = margin;
        root.gameObject.SetActive(true);

        titleText.text = ev.place;
        bodyText.text =
            $"Time (UTC): {ev.timeUtc:yyyy-MM-dd HH:mm:ss}\n" +
            $"Magnitude: {ev.magnitude:F1}\n" +
            $"Depth: {ev.depthKm:F1} km\n" +
            $"Lat: {ev.latitude:F3}, Lon: {ev.longitude:F3}";
    }

    public void Hide()
    {
        if (root != null)
            root.gameObject.SetActive(false);
    }
}