using System.Collections;
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
    public float movementSpeed = 5f;
    public float sprintMultiplier = 2f;
    private float gravity = -9.81f;
    public float jumpHeight = 2f;

    private Vector3 previousMovementInput;
    private Vector3 verticalVelocity;
    private float rotationSmoothVelocity;
    private float rotationSmoothTime = .1f;
    private bool isGrounded;
    private bool isSprinting = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            inputReader.OnMoveEvent += HandleMovement;
            inputReader.OnJumpEvent += HandleJump;
            inputReader.OnSprintEvent += HandleSprint;

            _mTransform = transform;
            mainCamera = Camera.main.transform;
        }

        StartCoroutine(AssignSpawnerRoutine());
    }

    private IEnumerator AssignSpawnerRoutine()
    {
        yield return new WaitForSeconds(0.15f);

        if (IsServer)
        {
            AssignSpawnServerSide();
        }
        else
        {
            RequestSpawnServerRpc();
        }
    }

    // =============================================
    //   HOST ASIGNA EL SPAWN INICIAL
    // =============================================

    private void AssignSpawnServerSide()
    {
        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
        PlayerDeathHandler death = GetComponent<PlayerDeathHandler>();

        Vector3 spawn = spawner.GetSpawnPoint(OwnerClientId);

        death.SetSpawner(spawner);
        death.initialSpawnPosition = spawn;
        death.spawnerAssigned = true;

        // Host puede mover su propio transform
        transform.position = spawn;
    }

    // CLIENTE → pide su spawn al servidor
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestSpawnServerRpc(RpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
    
            PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
            Vector3 spawn = spawner.GetSpawnPoint(clientId);
    
            AssignSpawnClientRpc(clientId, spawn);
        }

    // Servidor lo envía al cliente
    [ClientRpc]
    private void AssignSpawnClientRpc(ulong target, Vector3 spawnPos)
    {
        if (NetworkManager.Singleton.LocalClientId != target)
            return;

        var death = GetComponent<PlayerDeathHandler>();
        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();

        death.SetSpawner(spawner);
        death.initialSpawnPosition = spawnPos;
        death.spawnerAssigned = true;

        // Safe teleport con CharacterController
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            transform.position = spawnPos;
            cc.enabled = true;
        }
        else transform.position = spawnPos;
    }

    // =============================================
    //   MOVIMIENTO
    // =============================================

    private void HandleMovement(Vector3 movementInput) => previousMovementInput = movementInput;

    private void HandleJump()
    {
        if (isGrounded)
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void HandleSprint(bool sprinting) => isSprinting = sprinting;

    private void Update()
    {
        if (!IsOwner) return;
        Movement();
    }

    private void Movement()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && verticalVelocity.y < 0) verticalVelocity.y = -2f;

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

            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);
    }

    public void ResetMovementState()
    {
        previousMovementInput = Vector3.zero;
        verticalVelocity = Vector3.zero;

        if (characterController != null)
        {
            characterController.enabled = false;
            characterController.enabled = true;
        }
    }
}
