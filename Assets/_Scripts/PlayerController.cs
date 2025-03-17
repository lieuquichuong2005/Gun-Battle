using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public StarterAssetsInputs inputs;
    public Rigidbody rb;

    public WeaponController weaponController;
    private CharacterController controller;

    private Vector3 velocity;

    public float moveSpeed = 10.0f;
    public float jumpHeight = 2f;    
    public float gravity = -9.81f;   
    private bool isGrounded;

    [Header("Ground Check")]
    public Transform groundCheck;    
    public float groundDistance = 0.4f;
    public LayerMask groundMask;     

    void Start()
    {
        inputs = GetComponent<StarterAssetsInputs>();
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        weaponController = GameObject.FindGameObjectWithTag("WeaponController")?.GetComponent<WeaponController>();
    }

    void Update()
    {
        HandleMovement();
        HandleInput();
        HandleJumpAndGravity();
    }

    void HandleMovement()
    {
        Vector3 move = new Vector3(inputs.move.x, 0, inputs.move.y);
        transform.position += move * moveSpeed * Time.deltaTime;
    }
    void HandleInput()
    {
        if(inputs.shoot)
        {
            Debug.Log("Shooting");
            weaponController.Shoot();
        }
    }
    void HandleJumpAndGravity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
}
