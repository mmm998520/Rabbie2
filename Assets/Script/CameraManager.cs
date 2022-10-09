using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Transform locked;
    [SerializeField] float  cameraWidth,cameraHeight;
    Vector3 cameraPos;

    void Start()
    {
        cameraPos = transform.position;
        LateFixedUpdate.lateFixedUpdate.latePhysicsUpdateLog += new LateFixedUpdate.LatePhysicsUpdateEventHandler(latePhysicsUpdate);
    }

    void latePhysicsUpdate()
    {
        cameraFollow();
    }

    void cameraFollow()
    {
        cameraPos.x = Mathf.Clamp(cameraPos.x, locked.position.x - cameraWidth / 2f, locked.position.x + cameraWidth / 2f);
        cameraPos.y = Mathf.Clamp(cameraPos.y, locked.position.y - cameraHeight / 2f, locked.position.y + cameraHeight / 2f);
        transform.position = cameraPos;
    }
}
