using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeScaleUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private EarthquakePlayer earthquakePlayer;

    [Header("UI")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedLabel;

    // ===== 日/秒の最小・最大 =====
    private const float MinDaysPerSec = 1f / 24f;      // 1時間/秒
    private const float MaxDaysPerSec = 365.2422f;     // 1年/秒

    void Start()
    {
        if (speedSlider != null)
        {
            // ★ スライダーは 0〜1 のみ扱う
            speedSlider.minValue = 0f;
            speedSlider.maxValue = 1f;
            speedSlider.wholeNumbers = false;

            speedSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        // 初期値：1日/秒
        ApplyTimeScaleFromButton(1f);
    }

    // ===== ボタン用 =====
    public void Set1HourPerSec()
    {
        ApplyTimeScaleFromButton(1f / 24f);
    }

    public void Set1DayPerSec()
    {
        ApplyTimeScaleFromButton(1f);
    }

    public void Set1MonthPerSec()
    {
        ApplyTimeScaleFromButton(30f);
    }

    public void Set1YearPerSec()
    {
        ApplyTimeScaleFromButton(365.2422f);
    }

    // ===== スライダー用 =====
    private void OnSliderChanged(float t)
    {
        // t : 0〜1 → 対数変換
        float logDays = Mathf.Lerp(
            Mathf.Log10(MinDaysPerSec),
            Mathf.Log10(MaxDaysPerSec),
            t
        );

        float days = Mathf.Pow(10f, logDays);

        earthquakePlayer.SetTimeScale(days);
        UpdateLabel(days);
    }

    // ===== ボタン共通処理 =====
    private void ApplyTimeScaleFromButton(float days)
    {
        earthquakePlayer.SetTimeScale(days);

        // days → slider値(0〜1)へ逆変換
        float t = Mathf.InverseLerp(
            Mathf.Log10(MinDaysPerSec),
            Mathf.Log10(MaxDaysPerSec),
            Mathf.Log10(days)
        );

        speedSlider.SetValueWithoutNotify(t);
        UpdateLabel(days);
    }

    // ===== 表示更新 =====
    private void UpdateLabel(float days)
    {
        if (speedLabel != null)
        {
            speedLabel.text = $"{days:F2} days / sec";
        }
    }
}