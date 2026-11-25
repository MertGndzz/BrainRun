using UnityEngine;
using TMPro;

public class OperationGate : MonoBehaviour
{
    [Header("Referanslar")]
    public TextMeshPro textMesh;

    [HideInInspector] public string equationText = "";
    [HideInInspector] public int gateResult = 0;
    [HideInInspector] public bool isCorrectGate = false;
    [HideInInspector] public OperationGate pairedGate;

    private bool isTriggered = false;
    private Transform playerTransform;
    private Collider cachedCollider;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        ApplyLayout();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    private void OnDestroy()
    {
        pairedGate = null;
        playerTransform = null;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (transform.position.z < playerTransform.position.z - 15f)
        {
            Destroy(gameObject);
        }
    }

    public void ApplyLayout()
    {
        BoxCollider col = cachedCollider as BoxCollider;
        if (col == null)
        {
            col = GetComponent<BoxCollider>();
            if (col != null)
            {
                cachedCollider = col;
            }
        }

        if (col != null)
        {
            col.isTrigger = true;

            float posX = transform.position.x;
            float colliderWidth = 8.5f;
            float colliderCenterX = 0f;

            if (posX > 0.1f)
            {
                colliderCenterX = -(posX / 2f);
                colliderWidth = posX + 4.0f;
            }
            else if (posX < -0.1f)
            {
                colliderCenterX = (-posX / 2f);
                colliderWidth = (-posX) + 4.0f;
            }

            col.center = new Vector3(colliderCenterX, 0f, 0f);
            col.size = new Vector3(colliderWidth, 5.5f, 1.5f);
        }

        Transform cubeChild = transform.Find("Cube");
        if (cubeChild != null)
        {
            cubeChild.localPosition = Vector3.zero;
            cubeChild.localRotation = Quaternion.identity;
            cubeChild.localScale = new Vector3(7.4f, 5.0f, 0.4f);
        }

        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMeshPro>();
        }

        if (textMesh != null)
        {
            textMesh.transform.SetParent(transform, false);
            textMesh.transform.localPosition = new Vector3(0f, 0f, -0.25f);
            textMesh.transform.localRotation = Quaternion.identity;
            textMesh.transform.localScale = Vector3.one;

            RectTransform rt = textMesh.rectTransform;
            if (rt != null)
            {
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition3D = new Vector3(0f, 0f, -0.25f);
                rt.sizeDelta = new Vector2(7.0f, 4.8f);
            }

            textMesh.horizontalAlignment = HorizontalAlignmentOptions.Center;
            textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
            textMesh.margin = Vector4.zero;
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.color = Color.white;

            textMesh.enableAutoSizing = true;
            textMesh.fontSizeMin = 8f;
            textMesh.fontSizeMax = 44f;
            textMesh.characterSpacing = 4f;

            textMesh.ForceMeshUpdate();
        }
    }

    public void Setup(string equation, int result, bool isCorrect = false)
    {
        equationText = equation;
        gateResult = result;
        isCorrectGate = isCorrect;

        if (textMesh != null)
        {
            textMesh.text = equationText;
            textMesh.ForceMeshUpdate();
        }

        ApplyLayout();
    }

    public void PairWith(OperationGate other)
    {
        pairedGate = other;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        if (isTriggered || (pairedGate != null && pairedGate.isTriggered))
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (transform.position.x > 0f && other.transform.position.x < 0f)
        {
            return;
        }

        if (transform.position.x < 0f && other.transform.position.x > 0f)
        {
            return;
        }

        LockConsumed();

        if (pairedGate != null)
        {
            pairedGate.LockConsumed();
        }

        NumberPlayer player = other.GetComponent<NumberPlayer>();
        if (player != null)
        {
            if (isCorrectGate)
            {
                if (ScoreManager.instance != null)
                {
                    ScoreManager.instance.AddScore(1);
                }

                player.CorrectChoice();
            }
            else if (player.IsInvulnerable)
            {
                player.ContinueWithoutPenalty();
            }
            else
            {
                player.WrongChoice();
            }
        }

        OperationGate partner = pairedGate;
        pairedGate = null;
        if (partner != null)
        {
            partner.pairedGate = null;
            Destroy(partner.gameObject);
        }

        Destroy(gameObject);
    }

    private void LockConsumed()
    {
        isTriggered = true;
        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }
    }
}
