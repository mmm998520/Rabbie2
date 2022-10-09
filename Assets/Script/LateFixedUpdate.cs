using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateFixedUpdate : MonoBehaviour
{
    public static LateFixedUpdate lateFixedUpdate;
    public delegate void LateFixedUpdateEventHandler();
    public event LateFixedUpdateEventHandler lateFixedUpdateLog;
    public delegate void LatePhysicsUpdateEventHandler();
    public event LatePhysicsUpdateEventHandler latePhysicsUpdateLog;

    private void Awake()
    {
        if (lateFixedUpdate == null)
        {
            lateFixedUpdate = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void FixedUpdate()
    {
        lateFixedUpdateLog?.Invoke();
        StartCoroutine(latePhysicsUpdate());
    }

    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    IEnumerator latePhysicsUpdate()
    {
        yield return waitForFixedUpdate;
        latePhysicsUpdateLog?.Invoke();
    }
}