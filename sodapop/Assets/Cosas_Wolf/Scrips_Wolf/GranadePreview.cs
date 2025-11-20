using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrenadePreview : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public PlayerShooter shooter;
    public InputReader inputReader;
    public GameObject impactMarkerPrefab;

    [Header("Settings")]
    public int resolution = 30;
    public float timeStep = 0.1f;
    public Color previewColor = Color.red;

    private LineRenderer lineRenderer;
    private bool isHolding;
    private GameObject impactMarkerInstance;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = resolution;
        lineRenderer.startColor = previewColor;
        lineRenderer.endColor = previewColor;
        lineRenderer.enabled = false;

        if (impactMarkerPrefab != null)
        {
            impactMarkerInstance = Instantiate(impactMarkerPrefab);
            impactMarkerInstance.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // El PlayerShooter ya filtra con IsOwner, así que aquí no hace falta
        inputReader.OnFireEvent += HandleFire;
    }

    private void OnDisable()
    {
        inputReader.OnFireEvent -= HandleFire;
    }

    private void HandleFire(bool isPressed)
    {
        // Solo dibujamos si este jugador es el dueño
        if (!shooter.IsOwner) return;

        isHolding = isPressed;

        if (!isHolding)
        {
            lineRenderer.enabled = false;
            if (impactMarkerInstance != null)
                impactMarkerInstance.SetActive(false);
        }
    }

    private void Update()
    {
        if (!shooter.IsOwner) return; // seguridad extra

        if (isHolding)
        {
            ShowTrajectory();
            ShowImpactPoint();
        }
    }

    private void ShowTrajectory()
    {
        lineRenderer.enabled = true;

        Vector3 startPos = firePoint.position;
        Vector3 startVel = firePoint.forward * shooter.shootForce + Vector3.up * shooter.upwardForce;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + startVel * t + 0.5f * Physics.gravity * t * t;
            lineRenderer.SetPosition(i, point);
        }
    }

    private void ShowImpactPoint()
    {
        if (impactMarkerInstance == null) return;

        Vector3 startPos = firePoint.position;
        Vector3 startVel = firePoint.forward * shooter.shootForce + Vector3.up * shooter.upwardForce;

        Vector3 lastPoint = startPos;
        for (int i = 1; i < resolution; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + startVel * t + 0.5f * Physics.gravity * t * t;

            if (Physics.Raycast(lastPoint, (point - lastPoint).normalized, out RaycastHit hit, (point - lastPoint).magnitude))
            {
                impactMarkerInstance.SetActive(true);
                impactMarkerInstance.transform.position = hit.point + Vector3.up * 0.01f;
                impactMarkerInstance.transform.rotation = Quaternion.LookRotation(hit.normal);
                return;
            }

            lastPoint = point;
        }

        impactMarkerInstance.SetActive(false);
    }
}
