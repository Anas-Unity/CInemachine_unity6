using UnityEngine;
using UnityEngine.InputSystem;

// Ensures a CharacterController component is present on this GameObject.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    [Header("Camera Transform")]
    [Tooltip("Assign your Main Camera's Transform here.")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("If true, the player will rotate to face the direction of movement.")]
    [SerializeField] private bool shouldFaceMoveDirection = false;

    [Header("Movement Settings")]
    [Tooltip("The speed at which the player moves horizontally.")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("The height the player can reach when jumping.")]
    [SerializeField] private float jumpHeight = 2f;

    [Tooltip("The force of gravity applied to the player. Standard Earth gravity is -9.81f.")]
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private Vector2 currentMovementInput;
    private Vector3 playerVelocity;
    private bool shouldJump;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.SetCallbacks(this);
        controller = GetComponent<CharacterController>();

        // Ensure cameraTransform is assigned, otherwise default to Camera.main
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
            {
                Debug.LogWarning("Camera Transform not assigned and no Main Camera found. Player movement will be world-relative.");
            }
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        // --- Calculate Camera-Relative Movement Direction ---
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward; // Use camera forward or world forward as fallback
        Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;     // Use camera right or world right as fallback

        forward.y = 0f; // Flatten the vectors to the horizontal plane
        right.y = 0f;

        forward.Normalize(); // Ensure unit length after flattening
        right.Normalize();

        // Calculate the desired movement direction based on camera orientation and input
        Vector3 desiredMoveDirection = forward * currentMovementInput.y + right * currentMovementInput.x;

        // --- Handle Player Rotation ---
        // Only rotate if there's significant movement input and shouldFaceMoveDirection is true
        if (shouldFaceMoveDirection && desiredMoveDirection.sqrMagnitude > 0.001f)
        {
            // Create a rotation that looks in the direction of movement
            Quaternion toRotation = Quaternion.LookRotation(desiredMoveDirection, Vector3.up);
            // Smoothly interpolate to the new rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }

        // --- Handle Jumping ---
        if (isGrounded && shouldJump)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            shouldJump = false;
            Debug.Log("Jump executed! isGrounded: " + isGrounded);
        }

        // --- Apply Gravity ---
        playerVelocity.y += gravity * Time.deltaTime;

        // --- Combine all movements and apply to CharacterController ---
        // The horizontal movement is based on the camera-relative 'desiredMoveDirection'
        // The vertical movement is based on 'playerVelocity.y' (gravity/jump)
        Vector3 finalMovement = desiredMoveDirection * moveSpeed + new Vector3(0, playerVelocity.y, 0);

        // Apply the final combined movement to the CharacterController
        controller.Move(finalMovement * Time.deltaTime);
    }

    #region InputSystem_Actions.IPlayerActions Implementation

    public void OnMove(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("OnJump callback triggered! Phase: " + context.phase);

        if (context.performed)
        {
            shouldJump = true;
            Debug.Log("Jump input received (context.performed)!");
        }
    }

    public void OnLook(InputAction.CallbackContext context) { }
    public void OnAttack(InputAction.CallbackContext context) { }
    public void OnInteract(InputAction.CallbackContext context) { }
    public void OnCrouch(InputAction.CallbackContext context) { }
    public void OnPrevious(InputAction.CallbackContext context) { }
    public void OnNext(InputAction.CallbackContext context) { }
    public void OnSprint(InputAction.CallbackContext context) { }

    #endregion
}
