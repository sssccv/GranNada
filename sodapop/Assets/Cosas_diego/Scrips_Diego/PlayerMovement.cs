using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
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
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    private float rotationSmoothVelocity;
    private float rotationSmoothTime = .1f;

    private Vector3 previousMovementInput;
    private Vector3 verticalVelocity;
    private bool isGrounded;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.OnMoveEvent += HandleMovement;
        inputReader.OnJumpEvent += HandleJump;

        _mTransform = transform;
        mainCamera = Camera.main.transform;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        if (inputReader != null)
            inputReader.OnMoveEvent -= HandleMovement;
            inputReader.OnJumpEvent -= HandleJump;
    }

    private void Update()
    {
        if (!IsOwner) { return; }

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
            // Fórmula física para salto (v = sqrt(2 * h * -g))
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void Movement()
    {
        // Detectar si está en el suelo
        isGrounded = characterController.isGrounded;

        // Resetear velocidad vertical si está en el suelo
        if (isGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;

        // Movimiento horizontal
        float x = previousMovementInput.x;
        float z = previousMovementInput.z;

        Vector3 direction = new Vector3(x, 0f, z).normalized;

        if (direction.magnitude >= .1f)
        {
            float targeAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(_mTransform.eulerAngles.y, targeAngle, ref rotationSmoothVelocity, rotationSmoothTime);
            _mTransform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targeAngle, 0f) * Vector3.forward;
            characterController.Move(moveDirection * (movementSpeed * Time.deltaTime));
        }

        // Aplicar gravedad
        verticalVelocity.y += gravity * Time.deltaTime;

        // Mover en Y
        characterController.Move(verticalVelocity * Time.deltaTime);
    }
}
