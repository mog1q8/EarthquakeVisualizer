using UnityEngine;

public class GlobeCameraController : MonoBehaviour
{
    [SerializeField] private Transform target; // 地球
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 5f;

    private float _currentDistance;

    void Start()
    {
        _currentDistance = Vector3.Distance(transform.position, target.position);
    }

    void Update()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            _currentDistance -= scroll * zoomSpeed * Time.deltaTime;
            _currentDistance = Mathf.Clamp(_currentDistance, minDistance, maxDistance);

            transform.position = target.position - transform.forward * _currentDistance;
        }
    }
}