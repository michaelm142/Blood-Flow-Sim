using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class BloodFlow : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// Prefab instance of blood cell
    /// </summary>
    public GameObject BloodCellPrefab;

    /// <summary>
    /// Arrow that shows the direction of blood flow
    /// </summary>
    public GameObject conePrefab;
    private GameObject cone;

    /// <summary>
    /// Collection of spawned blood cells
    /// </summary>
    private List<BloodCellData> bloodCells = new List<BloodCellData>();

    /// <summary>
    /// Adjusts the color of the blood cell
    /// </summary>
    public Color bloodCellColor;


    /// <summary>
    /// Speed of the blood flow
    /// </summary>
    public float BloodSpeed;
    /// <summary>
    /// Rate at which cells will be spawned
    /// </summary>
    public float SpawnRate = 1.0f;
    /// <summary>
    /// Diameter of the blood vessel
    /// </summary>
    public float FlowDiameter = 1.0f;
    /// <summary>
    /// MThe ammout of time it takes for a blood cell to travel the lenght of the curve traveling at 'BloodSpeed'
    /// </summary>
    private float MaxCellLifeTime
    {
        get { return curve.length / BloodSpeed; }
    }
    /// <summary>
    /// Timer that counts down to spawn a blood cell
    /// </summary>
    private float spawnTimer;

    /// <summary>
    /// position of the cone on the blood flow
    /// </summary>
    private float conePosition;

    private Vector3 previousConePosition;

    /// <summary>
    /// The Curve component attached to this game object
    /// </summary>
    private BezierCurve curve;
    /// <summary>
    /// Renders the path the blood flow will take
    /// </summary>
    private LineRenderer line;
    #endregion

    #region Unity Functions 

    // Start is called before the first frame update
    void Start()
    {
        spawnTimer = 0;

        curve = GetComponent<BezierCurve>();

        cone = Instantiate(conePrefab);
        previousConePosition = cone.transform.position;

        line = gameObject.AddComponent<LineRenderer>();
        line.material.color = cone.GetComponent<MeshRenderer>().material.color;
        line.startWidth = line.endWidth = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer -= Time.deltaTime;
        // prevent spawn timer from going over spawn rate
        spawnTimer = Mathf.Clamp(spawnTimer, float.NegativeInfinity, SpawnRate);
        if (spawnTimer <= 0.0f)
        {
            SpawnCell();
            spawnTimer = 1.0f / SpawnRate;
        }

        for (int i = 0; i < bloodCells.Count; i++)
            UpdateBloodCell(bloodCells[i]);

        // update line
        line.positionCount = curve.resolution * 10;
        for (int i = 1; i <= line.positionCount; i++)
        {
            float w = i / (float)line.positionCount;
            line.SetPosition(i - 1, curve.GetPointAt(w));
        }
        // update arrow
        conePosition += Time.deltaTime * BloodSpeed;
        float coneW = conePosition / MaxCellLifeTime;
        if (Mathf.Abs(1.0f - coneW) < 0.1f || coneW > 1.0f)
        {
            coneW = 0.0f;
            conePosition = 0.0f;
        }
        coneW = Mathf.Clamp01(coneW);
        cone.transform.position = curve.GetPointAt(coneW);
        Vector3 coneDirection = cone.transform.position - previousConePosition;
        cone.transform.rotation = Quaternion.LookRotation(coneDirection, Vector3.up);
        previousConePosition = cone.transform.position;
    }

    #endregion

    #region Cell Management

    void UpdateBloodCell(BloodCellData bloodCell)
    {
        float t = bloodCell.LifeTime / MaxCellLifeTime;
        if (Mathf.Abs(1.0f - t) < 0.1f || t > 1.0f)
            RemoveCell(bloodCell);

        bloodCell.transform.position = curve.GetPointAt(t) + bloodCell.flowOffset * FlowDiameter;
        bloodCell.GetComponentInChildren<MeshRenderer>().material.color = bloodCellColor;

        bloodCell.LifeTime += Time.deltaTime * BloodSpeed;
        bloodCell.transform.rotation *= bloodCell.RotationSpeed;
    }

    void RemoveCell(BloodCellData bloodCell)
    {
        bloodCells.Remove(bloodCell);
        Destroy(bloodCell.gameObject);
    }

    void SpawnCell()
    {
        var bloodCell = Instantiate(BloodCellPrefab);
        var cellData = bloodCell.AddComponent<BloodCellData>();
        Vector3 offset = new Vector3
        {
            x = Random.Range(-1.0f, 1.0f),
            y = Random.Range(-1.0f, 1.0f),
            z = Random.Range(-1.0f, 1.0f),
        };
        offset.Normalize();

        Vector3 rotationSpeedEuler = new Vector3
        {
            x = Random.Range(-1.0f, 1.0f),
            y = Random.Range(-1.0f, 1.0f),
            z = Random.Range(-1.0f, 1.0f),
        };
        cellData.flowOffset = offset;
        cellData.RotationSpeed = Quaternion.Euler(rotationSpeedEuler);

        bloodCells.Add(cellData);
    }

    #endregion

    /// <summary>
    /// Contains tracking data for blood cells
    /// </summary>
    class BloodCellData : MonoBehaviour
    {
        public Vector3 flowOffset { get; set; }

        public Quaternion RotationSpeed { get; set; }

        public float LifeTime { get; set; }
    }
}
