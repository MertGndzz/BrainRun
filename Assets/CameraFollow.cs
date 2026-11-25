using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    Vector3 offset;
    bool hasOffset;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player == null)
        {
            enabled = false;
            return;
        }

        offset = transform.position - player.position;
        hasOffset = true;
    }

    void LateUpdate()
    {
        if (!hasOffset || player == null)
        {
            return;
        }

        transform.position = player.position + offset;
    }
}
