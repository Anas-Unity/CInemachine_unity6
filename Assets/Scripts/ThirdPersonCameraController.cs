using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{

    [SerializeField] float zoomSpeed = 2f;
    [SerializeField] float zoomLerpSpeed = 10f;
    [SerializeField] float minDistance = 3f;
    [SerializeField] float maxDistance = 15f;

    private InputSystem_Actions controls;

    private CinemachineCamera camera;
    private CinemachineOrbitalFollow orbital;
    private Vector2 scrollDelta;

    private float targetZoom;
    private float currentZoom;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controls = new InputSystem_Actions();
        controls.Enable();
        controls.CameraControls.MouseZoom.performed += HandleMouseScroll;
        
        Cursor.lockState = CursorLockMode.Locked;

        camera = GetComponent<CinemachineCamera>();
        orbital = camera.GetComponent<CinemachineOrbitalFollow>();

        targetZoom = currentZoom = orbital.Radius;
    }

    // Update is called once per frame
    void Update()
    {
        if (scrollDelta.y != 0)
        {
            if (orbital != null)
            {
                targetZoom = Mathf.Clamp(orbital.Radius - scrollDelta.y * zoomSpeed, minDistance, maxDistance);
                scrollDelta = Vector2.zero;
            }
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbital.Radius = currentZoom;

    }

    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrollDelta = context.ReadValue<Vector2>();
        Debug.Log($" mouse is scrolling. Value: {scrollDelta}");
    }

}
