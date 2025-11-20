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
    [SerializeField] public float movementSpeed = 5f;
    [SerializeField] public float sprintMultiplier = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] public float jumpHeight = 2f;

    private Vector3 previousMovementInput;
    private Vector3 verticalVelocity;
    private float rotationSmoothVelocity;
    private float rotationSmoothTime = .1f;
    private bool isGrounded;
    private bool isSprinting = false;

    public override void OnNetworkSpawn()
    {
        // --- INPUT DEL JUGADOR ---
        if (IsOwner)
        {
            inputReader.OnMoveEvent += HandleMovement;
            inputReader.OnJumpEvent += HandleJump;
            inputReader.OnSprintEvent += HandleSprint;

            _mTransform = transform;
            mainCamera = Camera.main.transform;
        }

        // --- ASIGNAR PLAYERSPAWNER DESDE EL SERVIDOR ---
        if (IsServer)
        {
            StartCoroutine(ServerAssignSpawnerRoutine());
        }
    }

    private IEnumerator ServerAssignSpawnerRoutine()
    {
        yield return null; // aseguramos que la escena ya cargó

        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();

        if (spawner == null)
        {
            Debug.LogError("❌ PlayerSpawner no encontrado en escena.");
            yield break;
        }

        // Obtenemos el spawn correcto
        Vector3 spawnPos = spawner.GetSpawnPoint(OwnerClientId);

        // TELETRANSPORTAR DESDE EL SERVIDOR
        transform.position = spawnPos;

        // PASAR A CLIENTE SU SPAWNER PARA EL RESPAWN
        AssignSpawnerClientRpc(OwnerClientId, spawnPos);
    }

    [ClientRpc]
    private void AssignSpawnerClientRpc(ulong targetClient, Vector3 spawnPosition)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClient)
            return;

        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
        PlayerDeathHandler death = GetComponent<PlayerDeathHandler>();

        if (death != null && spawner != null)
        {
            death.SetSpawner(spawner);
            transform.position = spawnPosition;
        }
    }

    // --- MOVIMIENTO ---
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

    public void ResetMovementState()
{
    previousMovementInput = Vector3.zero;
    verticalVelocity = Vector3.zero;
}

}

