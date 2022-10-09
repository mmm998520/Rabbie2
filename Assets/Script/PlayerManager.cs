using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    int currentFrame;

    Vector2 newPos = Vector2.zero;
    private void FixedUpdate()
    {
        currentFrame++;


        rb.MovePosition(newPos);
    }

    void CheckCollisions()
    {

    }
}
