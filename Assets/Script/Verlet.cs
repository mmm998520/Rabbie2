using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Verlet
{
    public static float deltaTime;

    public float pos;
    public float velecty;
    public float acc;
    
    public static void setDeltaTime()
    {
        deltaTime = Time.fixedDeltaTime;
    }

    public void countPos()
    {
        pos += velecty * deltaTime + 1f / 2f * acc * deltaTime * deltaTime;
    }

    public void countVelecty()
    {
        velecty += acc * deltaTime;
    }
}