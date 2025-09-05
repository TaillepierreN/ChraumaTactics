using UnityEngine;
using Unity.Cinemachine;
using CT.Tools;

public class CameraController : MonoBehaviour
{
    private const float MIN_ZOOM = 2f;
    private const float MAX_ZOOM = 12f;

    [Header("Speed")]
    private float _moveSpeed = 10f;
    private float _rotationSpeed = 100f;
    private float _zoomSpeed = 1f;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineFollow _cinemachineFollow;
    [SerializeField] private Transform _cameraTransform;

    [Header("Spawns")]
    [Tooltip("Start point for player1")]
    [SerializeField] private Transform _p1Start;
    [Tooltip("Start point for player 2")]
    [SerializeField] private Transform _p2Start;

    [Header("Boundary")]
    [Tooltip("Play area box collider")]
    [SerializeField] private BoxCollider _playArea;
    [SerializeField] private float _edgeMargin = 0.5f;

    private Vector3 _followOffset;
    private Vector3 _cameraPosition;
    private Bounds _activeBounds;

    private void Start()
    {
        bool isPlayer2 = NetX.IsListening ? !NetX.IsServer : false;

        Transform anchor = (isPlayer2 && _p2Start) ? _p2Start : _p1Start;
        if (anchor)
        {
            transform.SetPositionAndRotation(anchor.position, anchor.rotation);
        }
        if (_playArea)
            _activeBounds = _playArea.bounds;

        _followOffset = _cinemachineFollow.FollowOffset;
        _cameraPosition = _cameraTransform.position;

        ClampInsideBounds();
    }

    void Update()
    {
        HandleCameraMovement();
        HandleCameraRotation();
        HandleCameraZoom();
        ClampInsideBounds();
    }

    private void HandleCameraMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputMoveDir = new Vector3(horizontal, 0, vertical);
        Vector3 moveVector = transform.forward * inputMoveDir.z + transform.right * inputMoveDir.x;
        if (Input.GetKey(KeyCode.LeftShift))
            moveVector *= 4f;
        transform.position += moveVector * _moveSpeed * Time.deltaTime;
    }

    private void HandleCameraRotation()
    {
        float rotationInput = 0;

        if (Input.GetKey(KeyCode.Q))
            rotationInput = -1f;
        if (Input.GetKey(KeyCode.E))
            rotationInput = +1f;

        transform.Rotate(Vector3.up * rotationInput * _rotationSpeed * Time.deltaTime);

        if (Input.GetMouseButton(1))
        {
            float mouseDeltaX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up * mouseDeltaX * _rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleCameraZoom()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            _followOffset.y -= _zoomSpeed;
            _cameraPosition.y -= _zoomSpeed;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            _followOffset.y += _zoomSpeed;
            _cameraPosition.y += _zoomSpeed;
        }

        _followOffset.y = Mathf.Clamp(_followOffset.y, MIN_ZOOM, MAX_ZOOM);
        _cameraPosition.y = Mathf.Clamp(_cameraPosition.y, MIN_ZOOM, MAX_ZOOM);
        _cinemachineFollow.FollowOffset = Vector3.Lerp(_cinemachineFollow.FollowOffset, _followOffset, Time.deltaTime * 10f);
        _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, _cameraPosition, Time.deltaTime * 10f);

    }
    private void ClampInsideBounds()
    {
        if (_activeBounds.size == Vector3.zero) return;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, _activeBounds.min.x + _edgeMargin, _activeBounds.max.x - _edgeMargin);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, _activeBounds.min.z + _edgeMargin, _activeBounds.max.z - _edgeMargin);

        transform.position = clampedPosition;

        _cameraPosition = _cameraTransform.position = transform.position;
    }

}
