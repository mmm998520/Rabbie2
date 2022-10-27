using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform _player;

    private Vector3 _locked;

    void Start()
    {
        if (_player == null)
        {
            var player = FindObjectOfType<TarodevController.PlayerController >();
            if (player != null) _player = player.transform;
        }
        _locked = transform.position - _player.position;
    }

    void FixedUpdate()
    {
        transform.position = _player.position + _locked;
    }
}
