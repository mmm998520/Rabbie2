using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int _fixedFrame;

    private void FixedUpdate()
    {
        _fixedFrame++;
    }
}
