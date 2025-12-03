using FishNet.Object; // Reemplaza Unity.Netcode
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class PlayerMovementFishNet : NetworkBehaviour
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
    // Si necesitas que el estado de sprint se sincronice con otros clientes, 
    // podrías usar [SyncVar] o un ObserverRpc en el HandleSprint, 
    // pero para movimiento local puro, una simple variable es suficiente.
    private bool isSprinting = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private int xSpeedHash;
    private int ySpeedHash;
    private int jumpHash;
    private int isGroundedHash;
    private int throwHash;
    private string throwTriggerName = "Throw";
    [SerializeField] private float animatorDampTime = 0.08f;
    [SerializeField] private float throwResetDelay = 0.5f;

    // FishNet usa OnStartClient y OnStartServer.
    // Para la lógica que solo debe ejecutarse cuando el objeto se inicializa en la red 
    // y tienes control (eres el dueño), OnStartClient es el lugar.
    public override void OnStartClient()
    {
        base.OnStartClient(); // Asegúrate de llamar a la base

        // FishNet utiliza IsOwner para la propiedad local, igual que Netcode.
        if (base.IsOwner)
        {
            inputReader.OnMoveEvent += HandleMovement;
            inputReader.OnJumpEvent += HandleJump;
            inputReader.OnSprintEvent += HandleSprint;

            _mTransform = transform;
            // La cámara principal solo la buscamos si somos el dueño
            mainCamera = Camera.main.transform;

            // Inicializar animator y hashes de parámetros (sigue igual)
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            xSpeedHash = Animator.StringToHash("xspeed");
            ySpeedHash = Animator.StringToHash("yspeed");
            jumpHash = Animator.StringToHash("Jump");
            isGroundedHash = Animator.StringToHash("IsGrounded");
            throwHash = Animator.StringToHash("Throw");

            if (animator != null)
            {
                animator.SetBool(jumpHash, false);
                animator.ResetTrigger(throwTriggerName);
            }

            // Iniciar la rutina de asignación de spawn
            StartCoroutine(AssignSpawnerRoutine());
        }
    }

    // Usamos OnStopClient para desuscribir eventos.
    // Esto es equivalente a OnNetworkDespawn de Netcode.
    public override void OnStopClient()
    {
        base.OnStopClient();

        if (base.IsOwner)
        {
            inputReader.OnMoveEvent -= HandleMovement;
            inputReader.OnJumpEvent -= HandleJump;
            inputReader.OnSprintEvent -= HandleSprint;
        }
    }

    private IEnumerator AssignSpawnerRoutine()
    {
        // Esperar un poco para asegurar que la red y el PlayerSpawner estén listos
        yield return new WaitForSeconds(0.15f);

        // FishNet usa IsServerInitialized (la propiedad recomendada) para determinar si eres el servidor/host.
        // base.IsServer ya no se recomienda.
        if (base.IsServerInitialized)
        {
            // El servidor/host realiza la asignación localmente.
            AssignSpawnServerSide();
        }
        else
        {
            // El cliente debe pedir al servidor que asigne el spawn.
            RequestSpawnServerRpc();
        }
    }

    // =============================================
    //   HOST ASIGNA EL SPAWN INICIAL
    // =============================================

    private void AssignSpawnServerSide()
    {
        // CAMBIO CLAVE: Cambia 'uint' por 'int'
        int ownerId = (int)base.OwnerId;

        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
        PlayerDeathHandler death = GetComponent<PlayerDeathHandler>();

        // Asegúrate de que GetSpawnPoint acepte 'int' o usa la conversión explícita
        Vector3 spawn = spawner.GetSpawnPoint(ownerId);

        death.SetSpawner(spawner);
        death.initialSpawnPosition = spawn;
        death.spawnerAssigned = true;

        transform.position = spawn;

        TargetAssignSpawnClientRpc(base.Owner, spawn);
    }

    // CLIENTE → pide su spawn al servidor.
    // FishNet usa la propiedad [ServerRpc] (similar a [Rpc(SendTo.Server)])
    // y no requiere RpcParams para obtener el cliente, usa base.OwnerId.
    [ServerRpc]
    private void RequestSpawnServerRpc()
    {
        // CAMBIO CLAVE: Cambia 'uint' por 'int'
        int clientId = base.OwnerId; // El Id del cliente que invocó este RPC.

        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();

        // Asegúrate de que este método acepte 'int'
        Vector3 spawn = spawner.GetSpawnPoint(clientId);

        // Llamamos al ClienteRpc para enviar la posición solo al dueño del objeto.
        TargetAssignSpawnClientRpc(base.Owner, spawn);
    }

    // Servidor lo envía al cliente.
    // FishNet usa TargetRpc para enviar a un cliente específico, 
    // tomando el NetworkConnection (base.Owner) como primer parámetro.
    // No requiere la verificación de NetworkManager.Singleton.LocalClientId != target.
    [TargetRpc]
    private void TargetAssignSpawnClientRpc(FishNet.Connection.NetworkConnection conn, Vector3 spawnPos)
    {
        // La lógica solo se ejecuta en el cliente 'conn' (que será el dueño del objeto).

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
    //   MOVIMIENTO
    // =============================================

    // Los manejadores de input son iguales (no dependen de la red)
    private void HandleMovement(Vector3 movementInput) => previousMovementInput = movementInput;

    private void HandleJump()
    {
        if (isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null)
            {
                animator.SetBool(jumpHash, true);
                StartCoroutine(ResetAnimatorBool(jumpHash));
                animator.SetBool(isGroundedHash, false);
            }
        }
    }

    private void HandleSprint(bool sprinting) => isSprinting = sprinting;

    // Método público para disparar la animación de lanzar (Throw).
    public void DoThrow()
    {
        if (animator != null)
        {
            // Opcional: Si quieres que el tiro se vea en otros clientes, 
            // deberías invocar un ObserversRpc aquí. Por ahora, solo se ejecuta en el dueño.
            animator.SetTrigger(throwHash);
            StartCoroutine(ResetThrowTriggerCoroutine());
        }
    }

    // FishNet recomienda usar el método Update() solo si eres el dueño.
    private void Update()
    {
        // base.IsOwner es la propiedad clave en FishNet para verificar la propiedad.
        if (!base.IsOwner) return;

        Movement();
    }

    private IEnumerator ResetThrowTriggerCoroutine()
    {
        yield return new WaitForSeconds(throwResetDelay);
        if (animator != null)
            animator.ResetTrigger(throwTriggerName);
    }

    private void Movement()
    {
        // Lógica de movimiento y animación (sin cambios, ya que se ejecuta en el dueño)
        isGrounded = characterController.isGrounded;
        if (isGrounded && verticalVelocity.y < 0) verticalVelocity.y = -2f;

        if (animator != null)
            animator.SetBool(isGroundedHash, isGrounded);

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

            if (animator != null)
            {
                float xClamped = Mathf.Clamp(x, -1f, 1f);
                float yClamped = Mathf.Clamp(z, -1f, 1f);
                animator.SetFloat(xSpeedHash, xClamped, animatorDampTime, Time.deltaTime);
                animator.SetFloat(ySpeedHash, yClamped, animatorDampTime, Time.deltaTime);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetFloat(xSpeedHash, 0f, animatorDampTime, Time.deltaTime);
                animator.SetFloat(ySpeedHash, 0f, animatorDampTime, Time.deltaTime);
            }
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

    private IEnumerator ResetAnimatorBool(int hash)
    {
        yield return new WaitForSeconds(0.1f);
        if (animator != null)
            animator.SetBool(hash, false);
    }
}
