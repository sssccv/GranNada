using FishNet.Object;

using FishNet.Object.Synchronizing;

using System.Collections;

using UnityEngine;

using Unity.Cinemachine; // 1. IMPORTANTE: Necesitas esta librería


public class PlayerMovement : NetworkBehaviour

{

    [Header("References")]

    [SerializeField] private InputReader inputReader;

    [SerializeField] private CharacterController characterController;

    private Transform _mTransform;

    private Transform mainCamera;


    [Header("Camera Settings")]

    [SerializeField] private GameObject cameraPrefab;

    [SerializeField] private Transform cameraLookTarget;

    private GameObject _spawnedCamera;


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


    public override void OnStartClient()

    {

        base.OnStartClient();


        if (base.IsOwner)

        {

            inputReader.OnMoveEvent += HandleMovement;

            inputReader.OnJumpEvent += HandleJump;

            inputReader.OnSprintEvent += HandleSprint;


            _mTransform = transform;

            mainCamera = Camera.main.transform;


            // Llamamos a la inicialización corregida

            InitializeCamera();


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


            StartCoroutine(AssignSpawnerRoutine());

        }

    }


    // 4. Lógica para instanciar y configurar la cámara

    private void InitializeCamera()

    {

        if (cameraPrefab == null)

        {

            Debug.LogError("❌ Falta asignar el Camera Prefab en el PlayerMovement.");

            return;

        }


        // Instanciamos el prefab de la cámara localmente

        _spawnedCamera = Instantiate(cameraPrefab);


        // CAMBIO CLAVE: Ya no buscamos CinemachineFreeLook, sino CinemachineCamera

        var cineCam = _spawnedCamera.GetComponent<CinemachineCamera>();


        if (cineCam != null)

        {

            // Si no definiste un target específico, usa el transform del jugador

            Transform target = (cameraLookTarget != null) ? cameraLookTarget : transform;


            // En Cinemachine 3, Follow y LookAt siguen existiendo en CinemachineCamera

            cineCam.Follow = target;

            cineCam.LookAt = target;


            // Ajustar prioridad

            cineCam.Priority = 10;

        }

        else

        {

            Debug.LogError("❌ El prefab de cámara asignado no tiene un componente CinemachineCamera.");

        }

    }


    public override void OnStopClient()

    {

        base.OnStopClient();


        if (base.IsOwner)

        {

            inputReader.OnMoveEvent -= HandleMovement;

            inputReader.OnJumpEvent -= HandleJump;

            inputReader.OnSprintEvent -= HandleSprint;


            // 5. Destruir la cámara al desconectarse o morir

            if (_spawnedCamera != null)

            {

                Destroy(_spawnedCamera);

            }

        }

    }


    // ... (El resto del código AssignSpawnerRoutine, RPCs y Movimiento se queda IGUAL) ...


    private IEnumerator AssignSpawnerRoutine()

    {

        yield return new WaitForSeconds(0.15f);


        if (base.IsServerInitialized)

        {

            AssignSpawnServerSide();

        }

        else

        {

            RequestSpawnServerRpc();

        }

    }


    private void AssignSpawnServerSide()

    {

        int ownerId = (int)base.OwnerId;


        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();

        PlayerDeathHandler death = GetComponent<PlayerDeathHandler>();


        Vector3 spawn = spawner.GetSpawnPoint(ownerId);


        death.SetSpawner(spawner);

        death.initialSpawnPosition = spawn;

        death.spawnerAssigned = true;


        transform.position = spawn;


        TargetAssignSpawnClientRpc(base.Owner, spawn);

    }


    [ServerRpc]

    private void RequestSpawnServerRpc()

    {

        int clientId = base.OwnerId;


        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();


        Vector3 spawn = spawner.GetSpawnPoint(clientId);


        TargetAssignSpawnClientRpc(base.Owner, spawn);

    }


    [TargetRpc]

    private void TargetAssignSpawnClientRpc(FishNet.Connection.NetworkConnection conn, Vector3 spawnPos)

    {

        var death = GetComponent<PlayerDeathHandler>();

        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();


        death.SetSpawner(spawner);

        death.initialSpawnPosition = spawnPos;

        death.spawnerAssigned = true;


        CharacterController cc = GetComponent<CharacterController>();

        if (cc != null)

        {

            cc.enabled = false;

            transform.position = spawnPos;

            cc.enabled = true;

        }

        else transform.position = spawnPos;

    }


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


    public void DoThrow()

    {

        if (animator != null)

        {

            animator.SetTrigger(throwHash);

            StartCoroutine(ResetThrowTriggerCoroutine());

        }

    }


    private void Update()

    {

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