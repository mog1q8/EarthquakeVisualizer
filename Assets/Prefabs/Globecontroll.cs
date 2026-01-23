using UnityEngine;
using UnityEngine.EventSystems;

public class GlobeControll : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.3f;
    [SerializeField] private float maxVerticalAngle = 80f;

    private Vector3 _lastMousePos;
    private float _currentVerticalAngle = 0f;
    private bool _isDraggingGlobe = false;

    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        // ★ UIの上なら何もしない
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            _isDraggingGlobe = false;
            return;
        }

        // マウス押下開始
        if (Input.GetMouseButtonDown(0))
        {
            _isDraggingGlobe = IsClickOnGlobe();
            _lastMousePos = Input.mousePosition;
        }

        // ドラッグ中のみ回転
        if (Input.GetMouseButton(0) && _isDraggingGlobe)
        {
            Vector3 delta = Input.mousePosition - _lastMousePos;

            float rotY = delta.x * rotationSpeed;
            float rotX = -delta.y * rotationSpeed;

            // 横回転（地軸）
            transform.Rotate(Vector3.up, rotY, Space.World);

            // 縦回転（制限付き）
            float newAngle = _currentVerticalAngle + rotX;
            newAngle = Mathf.Clamp(newAngle, -maxVerticalAngle, maxVerticalAngle);

            float applied = newAngle - _currentVerticalAngle;
            transform.Rotate(Vector3.right, applied, Space.Self);

            _currentVerticalAngle = newAngle;

            _lastMousePos = Input.mousePosition;
        }

        // マウスを離したら終了
        if (Input.GetMouseButtonUp(0))
        {
            _isDraggingGlobe = false;
        }
    }

    /// <summary>
    /// マウスが Globe をクリックしているか判定
    /// </summary>
    private bool IsClickOnGlobe()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.transform == transform;
        }
        return false;
    }
}