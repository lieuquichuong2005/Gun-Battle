// PlayerController.cs (sửa phần rotation và camera)
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    public GameObject playerStatCanvas;

    [Header("Movement Settings")]
    public float moveSpeed = 10.0f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Camera")]
    public Camera playerCamera;
    public Transform cameraParent;

    [Networked] public Vector3 NetworkPosition { get; set; }
    [Networked] public Vector3 NetworkRotation { get; set; }
    [Networked] public float CameraXRotation { get; set; }

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    public NetworkedGunController gunController;
    public NetworkedPlayerStats playerStats;


    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();

        if (Object.HasInputAuthority)
        {
            playerCamera = Camera.main;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            gunController = GetComponent<NetworkedGunController>();
            playerStats = GetComponent<NetworkedPlayerStats>();
            playerStatCanvas?.SetActive(true);
        }
        else if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }

        NetworkPosition = transform.position;
        NetworkRotation = transform.rotation.eulerAngles;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<NetworkInputData>(out var input))
        {
            HandleMovement(input);
            HandleRotation(input);
            HandleJumpAndGravity(input);
            HandleCursor(input);

            //HandleAnimation(input);
        }

    }

    public override void Render()
    {
        if (!Object.HasInputAuthority)
        {
            transform.position = Vector3.Lerp(transform.position, NetworkPosition, 0.25f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(NetworkRotation), 0.25f);

            if (cameraParent != null)
            {
                cameraParent.localRotation = Quaternion.Euler(CameraXRotation, 0f, 0f);
            }
        }
        else if(Object.HasInputAuthority && cameraParent != null)
        {
            cameraParent.localRotation = Quaternion.Euler(CameraXRotation, 0f, 0f);
        }   
    }

    void HandleMovement(NetworkInputData input)
    {
        Vector3 move = transform.TransformDirection(input.moveDirection);
        move.y = 0;
        controller.Move(move * moveSpeed * Runner.DeltaTime);
        NetworkPosition = transform.position;
    }

    void HandleRotation(NetworkInputData input)
    {
        float mouseX = input.mouseInput.x * mouseSensitivity;
        float mouseY = input.mouseInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        NetworkRotation = transform.rotation.eulerAngles;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        CameraXRotation = xRotation;
    }

    void HandleJumpAndGravity(NetworkInputData input)
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (input.isJumping && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Runner.DeltaTime;
        controller.Move(velocity * Runner.DeltaTime);
        NetworkPosition = transform.position;
    }

    void HandleCursor(NetworkInputData input)
    {
        if (input.isHidingCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    //void HandleAnimation(NetworkInputData input)
    //{
    //    // Nếu đang di chuyển hoặc bắn thì reset idle timer
    //    if (input.moveDirection.magnitude > 0.1f || input.isShooting)
    //    {
    //        idleTimer = 0f;
    //    }
    //    else
    //    {
    //        idleTimer += Runner.DeltaTime;
    //    }

    //    // Ưu tiên bắn trước
    //    if (input.isShooting)
    //    {
    //        animator?.SetTrigger("Shoot");
    //    }
    //    else if (idleTimer >= idleThreshold)
    //    {
    //        animator?.SetTrigger("CheckGun");
    //        idleTimer = 0f; // Reset sau khi kiểm tra
    //    }
    //    else
    //    {
    //        animator?.SetBool("IsIdle", true);
    //    }
    //}

}
