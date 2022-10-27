using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Sensor
{
    public class InteractiveSensor : Sensor
    {
        TarodevController.FrameInput frameInput;
        private void Start()
        {
            frameInput = TarodevController.PlayerController._input.FrameInput;
        }

        public void checkCollisionIsPlayer(Collider2D collision)
        {
            if ((1 << collision.gameObject.layer) == TarodevController.PlayerController.playerLayer)
            {
                playerStay(collision);
            }
        }

        public void playerStay(Collider2D collision)
        {
            senserBroadcast();
        }

        public void OnTriggerStay2D(Collider2D collision)
        {
            if (frameInput.InteractiveDown)
            {
                checkCollisionIsPlayer(collision);
            }
        }
    }
}
