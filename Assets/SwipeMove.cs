using UnityEngine;

public class SwipeMove : MonoBehaviour
{
    [Header("Lane Settings")]
    public float laneDistance = 3f;
    public float laneMoveSpeed = 15f;

    [Header("Forward Movement")]
    public float baseForwardSpeed = 2f;

    int currentLane = 0;
    Vector2 startTouchPos;
    bool isSwiping;

    void Update()
    {
        if (!UIManager.isGameStarted) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        HandleKeyboard();
#endif
        HandleSwipe();
        MoveForward();
        MoveToLane();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void HandleKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.A))
            ChangeLane(-4);

        if (Input.GetKeyDown(KeyCode.D))
            ChangeLane(4);
    }
#endif

    void HandleSwipe()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            startTouchPos = touch.position;
            isSwiping = true;
        }

        if (touch.phase == TouchPhase.Ended && isSwiping)
        {
            float deltaX = touch.position.x - startTouchPos.x;

            if (Mathf.Abs(deltaX) > 50f)
                ChangeLane(deltaX > 0 ? 4 : -4);

            isSwiping = false;
        }
    }

    void ChangeLane(int direction)
    {
        currentLane += 2 * direction;
        currentLane = Mathf.Clamp(currentLane, -4, 4);
    }

    void MoveToLane()
    {
        Vector3 pos = transform.position;
        float targetX = currentLane * laneDistance;

        pos.x = Mathf.Lerp(pos.x, targetX, laneMoveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    void MoveForward()
    {
        float currentSpeed = GetCalculatedSpeed();
        transform.position += Vector3.forward * currentSpeed * Time.deltaTime;
    }

    float GetCalculatedSpeed()
    {
        int level = UIManager.gameSpeedLevel;

        switch (level)
        {
            case 1:
                return baseForwardSpeed * 0.75f;
            case 3:
                return baseForwardSpeed * 1.35f;
            default:
                return baseForwardSpeed;
        }
    }
}
