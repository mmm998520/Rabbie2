using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyAreaController : Receiver
{
    [SerializeField] LayerMask flyAreaLayer;
    bool trigger;

    void FixedUpdate()
    {
        StartCoroutine(lateFixedUpdate());
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((1 << collision.gameObject.layer) == flyAreaLayer)
        {
            TarodevController.PlayerController.canFlyFrames++;
            trigger = true;
        }
    }

    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    IEnumerator lateFixedUpdate()
    {
        yield return waitForFixedUpdate;
        if (!trigger)
        {
            TarodevController.PlayerController.canFlyFrames = 0;
        }
        trigger = false;
    }
}
