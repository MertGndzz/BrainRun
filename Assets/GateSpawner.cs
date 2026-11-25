using UnityEngine;
using System.Collections.Generic;

public class GateSpawner : MonoBehaviour
{
    public static GateSpawner Instance;

    public GameObject gatePrefab;
    public NumberPlayer player;

    [Header("Konum Ayarlar?")]
    public int zekseni = 100;

    public float laneOffset = 5.2f;
    public float spawnHeightY = 5.0f;

    private GameObject currentCorrectGate;
    private GameObject currentWrongGate;
    private readonly List<int> operationBuffer = new List<int>(4);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (player == null)
        {
            player = SceneQuery.FindFirst<NumberPlayer>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        SpawnGates();
    }

    public void ResetAndSpawnFirstGates()
    {
        if (currentCorrectGate != null) Destroy(currentCorrectGate);
        if (currentWrongGate != null) Destroy(currentWrongGate);

        currentCorrectGate = null;
        currentWrongGate = null;
        zekseni = 100;
        SpawnGates();
    }

    public void SpawnGates()
    {
        if (gatePrefab == null)
        {
            return;
        }

        if (player == null)
        {
            player = SceneQuery.FindFirst<NumberPlayer>();
        }

        int randomLane = Random.Range(0, 2);
        float x1 = (randomLane == 0) ? -laneOffset : laneOffset;
        float x2 = (randomLane == 0) ? laneOffset : -laneOffset;

        operationBuffer.Clear();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CopyActiveOperations(operationBuffer);
        }

        if (operationBuffer.Count == 0)
        {
            operationBuffer.Add(0);
        }

        int operationType = operationBuffer[Random.Range(0, operationBuffer.Count)];

        string correctEquation = "";
        int targetResult = 0;

        string wrongEquation = "";
        int wrongResult = 0;

        switch (operationType)
        {
            case 0:
                int addA = Random.Range(2, 45);
                int addB = Random.Range(2, 45);
                targetResult = addA + addB;
                correctEquation = $"{addA}+{addB}";

                int offsetAdd = (Random.Range(0, 2) == 0 ? 1 : -1) * Random.Range(1, 4);
                wrongResult = Mathf.Max(2, targetResult + offsetAdd);
                int wAddA = Random.Range(1, wrongResult);
                int wAddB = wrongResult - wAddA;
                wrongEquation = $"{wAddA}+{wAddB}";
                break;

            case 1:
                int subA = Random.Range(15, 75);
                int subB = Random.Range(2, subA - 2);
                targetResult = subA - subB;
                correctEquation = $"{subA}-{subB}";

                int offsetSub = (Random.Range(0, 2) == 0 ? 1 : -1) * Random.Range(1, 4);
                wrongResult = Mathf.Max(2, targetResult + offsetSub);
                int wSubB = Random.Range(2, 20);
                int wSubA = wrongResult + wSubB;
                wrongEquation = $"{wSubA}-{wSubB}";
                break;

            case 2:
                int mulA = Random.Range(2, 11);
                int mulB = Random.Range(2, 11);
                targetResult = mulA * mulB;
                correctEquation = $"{mulA}×{mulB}";

                int wMulA, wMulB;
                do
                {
                    wMulA = Random.Range(2, 11);
                    wMulB = Random.Range(2, 11);
                } while (wMulA * wMulB == targetResult);

                wrongResult = wMulA * wMulB;
                wrongEquation = $"{wMulA}×{wMulB}";
                break;

            case 3:
                int divB = Random.Range(2, 10);
                targetResult = Random.Range(2, 12);
                int divA = targetResult * divB;
                correctEquation = $"{divA}÷{divB}";

                int wDivB = Random.Range(2, 10);
                int offsetDiv = (Random.Range(0, 2) == 0 ? 1 : -1) * Random.Range(1, 3);
                wrongResult = Mathf.Max(1, targetResult + offsetDiv);
                int wDivA = wrongResult * wDivB;
                wrongEquation = $"{wDivA}÷{wDivB}";
                break;
        }

        if (player != null)
        {
            player.SetTargetValue(targetResult);
        }

        zekseni += 100;

        currentCorrectGate = Instantiate(gatePrefab, new Vector3(x1, spawnHeightY, zekseni), Quaternion.identity);
        currentWrongGate = Instantiate(gatePrefab, new Vector3(x2, spawnHeightY, zekseni), Quaternion.identity);

        if (currentCorrectGate == null || currentWrongGate == null)
        {
            return;
        }

        OperationGate opCorrect = currentCorrectGate.GetComponent<OperationGate>();
        OperationGate opWrong = currentWrongGate.GetComponent<OperationGate>();

        if (opCorrect != null && opWrong != null)
        {
            opCorrect.Setup(correctEquation, targetResult, true);
            opWrong.Setup(wrongEquation, wrongResult, false);

            opCorrect.PairWith(opWrong);
            opWrong.PairWith(opCorrect);
        }
    }
}
