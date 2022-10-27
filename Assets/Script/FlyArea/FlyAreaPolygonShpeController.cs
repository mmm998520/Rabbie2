using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class FlyAreaPolygonShpeController : MonoBehaviour
{
    Transform[] controllPoses;
    PolygonCollider2D polygonCollider2D;
    SpriteShapeController spriteShapeController;

    void Start()
    {
        reset();
    }

#if(UNITY_EDITOR)
    void Update()
    {
        if(transform.childCount != controllPoses.Length)
        {
            if (controllPoses.Length != polygonCollider2D.points.Length || controllPoses.Length != spriteShapeController.spline.GetPointCount())
            {
                reset();
            }
        }

        attachPos();
    }
#else   
    private void FixedUpdate()
    {
        attachPos();
    }
#endif

    private void reset()
    {
        controllPoses = new Transform[transform.childCount];
        for (int i = 0; i < controllPoses.Length; i++)
        {
            controllPoses[i] = transform.GetChild(i);
        }
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        polygonCollider2D.points = new Vector2[controllPoses.Length];
        spriteShapeController = GetComponent<SpriteShapeController>();
        int temp = spriteShapeController.spline.GetPointCount() - controllPoses.Length;
        for (int i = 0; i < -temp; i++)
        {
            spriteShapeController.spline.InsertPointAt(controllPoses.Length - 1, Vector3.zero);
        }
        for (int i = 0; i < temp; i++)
        {
            spriteShapeController.spline.RemovePointAt(0);
        }

        print(controllPoses.Length + "," + polygonCollider2D.points.Length + "," + spriteShapeController.spline.GetPointCount());
    }

    void attachPos()
    {
        for (int i = 0; i < controllPoses.Length; i++)
        {
            polygonCollider2D.points[i] = controllPoses[i].position;
            spriteShapeController.spline.SetPosition(i, controllPoses[i].position);
        }
    }
}
