using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Sensor
{
    public class ExitSensor : Sensor
    {
        public void checkCollisionIsPlayer(Collider2D collision)
        {
            if ((1 << collision.gameObject.layer) == TarodevController.PlayerController.playerLayer)
            {
                data.trigger = true;
                senserBroadcast();
            }
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            checkCollisionIsPlayer(collision);
        }
    }
}
