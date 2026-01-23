using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class EarthquakePlayer : MonoBehaviour
{
    [Header("Data")]
    public TextAsset csvFile;

    [Header("Date Display")]
    public DateText dateTextUI;

    // ===== 選択可能日付範囲 =====
    private static readonly DateTime MinSelectableDate =
        new DateTime(1983, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime MaxSelectableDate =
        new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    // ===== 日付ジャンプ（Inspector操作用）=====
    [Header("Date Jump (UTC)")]
    [Range(1983, 2024)] public int jumpYear = 1983;
    [Range(1, 12)] public int jumpMonth = 1;
    public int jumpDay = 1;

    public bool jumpNow = false;

    void OnValidate()
    {
        jumpDay = Mathf.Clamp(jumpDay, 1,
            DateTime.DaysInMonth(jumpYear, jumpMonth));
    }

    [Header("Globe")]
    public Transform globe;
    public float globeRadius = 1.0f;
    public GameObject markerPrefab;

    [Header("Playback")]
    public double timeScale = 60 * 60 * 24;
    public int maxActiveMarkers = 0;

    [SerializeField, Range(0.0001f, 0.1f)]
    private float markerBaseScale = 0.05f;

    public Vector2 magScaleRange = new Vector2(0.1f, 1.0f);

    [Header("UI")]
    public EarthquakePopupUI popupUI;

    private readonly List<EarthquakeEvent> _events = new();
    private readonly Queue<GameObject> _activeMarkers = new();

    private DateTime _currentSimTime;
    private int _currentIndex;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;

    // ===== UIからTimeScale変更 =====
    public void SetTimeScale(float daysPerSecond)
    {
        timeScale = daysPerSecond * 60 * 60 * 24;
    }

    void Start()
    {
        if (csvFile == null) return;

        LoadCsv(csvFile.text);
        if (_events.Count == 0) return;

        _events.Sort((a, b) => a.timeUtc.CompareTo(b.timeUtc));

        _currentSimTime = MinSelectableDate;
        _currentIndex = 0;
        _isPlaying = true;

        dateTextUI?.SetDate(_currentSimTime);
    }

    void Update()
    {
        if (!_isPlaying) return;

        if (jumpNow)
        {
            JumpToDate();
            jumpNow = false;
            return;
        }

        _currentSimTime = _currentSimTime.AddSeconds(timeScale * Time.deltaTime);
        if (_currentSimTime > MaxSelectableDate)
        {
            _currentSimTime = MaxSelectableDate;
        }
        dateTextUI?.SetDate(_currentSimTime);

        while (_currentIndex < _events.Count &&
               _events[_currentIndex].timeUtc <= _currentSimTime)
        {
            SpawnMarker(_events[_currentIndex]);
            _currentIndex++;
        }
    }
    // ===== UI用：外部から呼ぶ =====
    public void JumpToDateFromUI(int year, int month, int day)
    {
        jumpYear = year;
        jumpMonth = month;
        jumpDay = day;

        JumpToDate();
    }

    // ===== 再生制御 =====
    public void Play() => _isPlaying = true;
    public void Pause() => _isPlaying = false;

    public void Stop()
    {
        _isPlaying = false;

        foreach (var m in _activeMarkers)
            if (m != null) Destroy(m);
        _activeMarkers.Clear();

        _currentSimTime = MinSelectableDate;
        _currentIndex = 0;
        dateTextUI?.SetDate(_currentSimTime);
    }

    // ===== 日付ジャンプ =====
    public void JumpToDate()
    {
        foreach (var m in _activeMarkers)
            if (m != null) Destroy(m);
        _activeMarkers.Clear();

        _currentSimTime = GetClampedJumpDate();
        _currentIndex = 0;

        while (_currentIndex < _events.Count &&
               _events[_currentIndex].timeUtc < _currentSimTime)
            _currentIndex++;

        dateTextUI?.SetDate(_currentSimTime);
    }

    private DateTime GetClampedJumpDate()
    {
        DateTime dt = new DateTime(
            jumpYear, jumpMonth, jumpDay,
            0, 0, 0, DateTimeKind.Utc);

        if (dt < MinSelectableDate) dt = MinSelectableDate;
        if (dt > MaxSelectableDate) dt = MaxSelectableDate;

        return dt;
    }

    // ===== マーカー生成（★最重要修正点）=====
    private void SpawnMarker(EarthquakeEvent ev)
    {
        var go = Instantiate(markerPrefab, globe);
        go.transform.localPosition = ev.worldPos; // ← ローカル座標
        go.transform.localRotation = Quaternion.identity;

        float t = Mathf.InverseLerp(2f, 9f, ev.magnitude);
        float scale = Mathf.Lerp(magScaleRange.x, magScaleRange.y, t);
        go.transform.localScale = Vector3.one * markerBaseScale * scale;

        go.GetComponent<EarthquakeMarker>()
            ?.SetData(ev, popupUI, this);

        _activeMarkers.Enqueue(go);
        Destroy(go, 5f);
    }

    // ===== CSV読み込み =====
    private void LoadCsv(string csv)
    {
        var lines = csv.Split(new[] { '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries);

        var header = SplitCsvLine(lines[0]);

        int idxTime = Array.IndexOf(header, "time");
        int idxLat = Array.IndexOf(header, "latitude");
        int idxLon = Array.IndexOf(header, "longitude");
        int idxDepth = Array.IndexOf(header, "depth");
        int idxMag = Array.IndexOf(header, "mag");
        int idxPlace = Array.IndexOf(header, "place");

        for (int i = 1; i < lines.Length; i++)
        {
            var cols = SplitCsvLine(lines[i]);
            if (!DateTimeOffset.TryParse(cols[idxTime],
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var dto)) continue;

            _events.Add(new EarthquakeEvent
            {
                timeUtc = dto.UtcDateTime,
                latitude = double.Parse(cols[idxLat]),
                longitude = double.Parse(cols[idxLon]),
                depthKm = float.Parse(cols[idxDepth]),
                magnitude = float.Parse(cols[idxMag]),
                place = cols[idxPlace],
                worldPos = LatLonToLocal(
                    double.Parse(cols[idxLat]),
                    double.Parse(cols[idxLon]),
                    globeRadius)
            });
        }
    }

    // ===== 緯度経度 → ローカル座標 =====
    private Vector3 LatLonToLocal(double latDeg, double lonDeg, float radius)
    {
        double lat = latDeg * Mathf.Deg2Rad;
        double lon = lonDeg * Mathf.Deg2Rad;

        return new Vector3(
            (float)(radius * Math.Cos(lat) * Math.Cos(lon)),
            (float)(radius * Math.Sin(lat)),
            (float)(radius * Math.Cos(lat) * Math.Sin(lon))
        );
    }

    private static string[] SplitCsvLine(string line)
    {
        var list = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"') inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                list.Add(sb.ToString());
                sb.Clear();
            }
            else sb.Append(c);
        }
        list.Add(sb.ToString());
        return list.ToArray();
    }
}