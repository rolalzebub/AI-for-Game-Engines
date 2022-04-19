using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float WalkSpeed;
    private float Speed;
    public bool IsGrounded;
    public bool IsMoving;
    public bool IsJumping;
    public bool CanJump;
    public float Gravity = -9.81f;
    public float GroundDistance = 0.4f;
    public float JumpHeight = 2.0f;
    public float TurnSmoothTime = 0.1f;
    public float TurnSmoothVelocity;

    public LayerMask GroundMask;
    Vector3 Velocity;

    public CharacterController Controller;
    public Transform GroundCheck;
    public Transform Camera;
    public GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        Speed = WalkSpeed;
        IsMoving = false;
        IsJumping = false;
        CanJump = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Runs a check that looks to see if the player is on the ground
        IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);

        float X = Input.GetAxisRaw("Horizontal");
        float Z = Input.GetAxisRaw("Vertical");
        Vector3 Direction = new Vector3(X, 0f, Z).normalized;

        if (Direction.magnitude >= 0.1f)
        {
            IsMoving = true;
            float TargetAngle = Mathf.Atan2(Direction.x, Direction.z) * Mathf.Rad2Deg + Camera.eulerAngles.y;
            float Angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetAngle, ref TurnSmoothVelocity, TurnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, Angle, 0f);

            Vector3 MoveDirection = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;
            Controller.Move(MoveDirection.normalized * Speed * Time.deltaTime);
            
        }
        if (Direction.magnitude < 0.1f)
        {
            IsMoving = false;
        }

        // Stops the velocity from constantly increasing if player is on ground
        if (IsGrounded && Velocity.y < 0)
        {
            IsJumping = false;
            Velocity.y = -2.0f;
        }

        // Makes the player jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded && CanJump)
        {
            IsJumping = true;
            Velocity.y = Mathf.Sqrt(JumpHeight * -2.0f * Gravity);

        }

        // Handles the velocity and movement
        Velocity.y += Gravity * Time.deltaTime;
        Controller.Move(Velocity * Time.deltaTime);

    }


}
