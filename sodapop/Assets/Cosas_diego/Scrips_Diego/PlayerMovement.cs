using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CharacterController characterController;
    private Transform _mTransform;
    private Transform mainCamera;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    private float rotationSmoothVelocity;
    private float rotationSmoothTime = .1f;

    private Vector3 previousMovementInput;
    private Vector3 verticalVelocity;
    private bool isGrounded;
    private bool isSprinting = false;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.OnMoveEvent += HandleMovement;
        inputReader.OnJumpEvent += HandleJump;
        inputReader.OnSprintEvent += HandleSprint;

        _mTransform = transform;
        mainCamera = Camera.main.transform;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        if (inputReader != null)
        {
            inputReader.OnMoveEvent -= HandleMovement;
            inputReader.OnJumpEvent -= HandleJump;
            inputReader.OnSprintEvent -= HandleSprint;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        Movement();
    }

    private void HandleMovement(Vector3 movementInput)
    {
        previousMovementInput = movementInput;
    }

    private void HandleJump()
    {
        if (isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void HandleSprint(bool sprinting)
    {
        isSprinting = sprinting;
    }

    private void Movement()
    {
        isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;

        float x = previousMovementInput.x;
        float z = previousMovementInput.z;

        Vector3 direction = new Vector3(x, 0f, z).normalized;

        if (direction.magnitude >= .1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(_mTransform.eulerAngles.y, targetAngle, ref rotationSmoothVelocity, rotationSmoothTime);
            _mTransform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = isSprinting ? movementSpeed * sprintMultiplier : movementSpeed;
            characterController.Move(moveDirection * (currentSpeed * Time.deltaTime));
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);
    }
}
