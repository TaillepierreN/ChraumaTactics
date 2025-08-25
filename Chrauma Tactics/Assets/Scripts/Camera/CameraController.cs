using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    private const float MIN_ZOOM = 2f;
    private const float MAX_ZOOM = 12f;
    private float _moveSpeed = 10f;
    private float _rotationSpeed = 100f;
    private float _zoomSpeed = 1f;
    [SerializeField] private CinemachineFollow _cinemachineFollow;
    [SerializeField] private Transform _cameraTransform;
    private Vector3 _followOffset;
    private Vector3 _cameraPosition;

    private void Start()
    {
        _followOffset = _cinemachineFollow.FollowOffset;
        _cameraPosition = _cameraTransform.position;
    }

    void Update()
    {
        HandleCameraMovement();
        HandleCameraRotation();
        HandleCameraZoom();
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
        _cameraPosition.y = Mathf.Clamp(_cameraPosition.y, MIN_ZOOM - 9f, MAX_ZOOM);
        _cinemachineFollow.FollowOffset = Vector3.Lerp(_cinemachineFollow.FollowOffset, _followOffset, Time.deltaTime * 10f);
        _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, _cameraPosition, Time.deltaTime * 10f);

    }

}
