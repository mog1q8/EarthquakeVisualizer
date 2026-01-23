using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EarthquakeMarker : MonoBehaviour
{
    private EarthquakeEvent _data;
    private EarthquakePopupUI _popupUI;
    private EarthquakePlayer _player;

    [Header("Effect Settings")]
    public float lifeTime = 3f;
    public float startScale = 0.05f;
    public float endScale = 1.0f;

    private float _time;
    private Material _mat;
    private Color _startColor;

    public void SetData(
        EarthquakeEvent data,
        EarthquakePopupUI popupUI,
        EarthquakePlayer player
    )
    {
        _data = data;
        _popupUI = popupUI;
        _player = player;

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            _mat = renderer.material;
            _startColor = _mat.color;
        }

        transform.localScale = Vector3.one * startScale;
        _time = 0f;
    }

    void Update()
    {
        if (_mat == null) return;

        // 再生停止中はアニメーション停止
        if (_player != null && !_player.IsPlaying)
            return;

        _time += Time.deltaTime;
        float t = _time / lifeTime;

        float scale = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = Vector3.one * scale;

        Color c = _startColor;
        c.a = Mathf.Lerp(1f, 0f, t);
        _mat.color = c;

        if (_time >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 表側に見えている地震のみクリック可能
    /// </summary>
    void OnMouseDown()
    {
        if (_popupUI == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // ★ カメラ方向チェック（裏側は無効）
        Vector3 toCamera = (cam.transform.position - transform.position).normalized;
        Vector3 normal = transform.position.normalized;

        float dot = Vector3.Dot(normal, toCamera);

        if (dot <= 0f)
            return; // 地球の裏側

        _popupUI.Show(_data, transform.position);
    }
}