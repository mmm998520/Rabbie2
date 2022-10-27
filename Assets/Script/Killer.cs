using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((1 << collision.gameObject.layer) == TarodevController.PlayerController.playerLayer)
        {
            Saver.goToSavePos();
        }
    }
}
