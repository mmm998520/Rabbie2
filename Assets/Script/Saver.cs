using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saver : MonoBehaviour
{
    public static Saver currentSaver;
    public static TarodevController.CameraFollow _camera;
    public static GameObject _player;
    public static GameObject _playerPrefab;
    public GameObject playerPrefab;

    void Start()
    {
        if (_camera == null)
        {
            var camera = FindObjectOfType<TarodevController.CameraFollow>();
            if (camera != null) _camera = camera;
        }
        if (_player == null)
        {
            var player = FindObjectOfType<TarodevController.PlayerController>();
            if (player != null) _player = player.gameObject;
        }
        if (_playerPrefab == null)
        {
            _playerPrefab = playerPrefab;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((1 << collision.gameObject.layer) == TarodevController.PlayerController.playerLayer)
        {
            currentSaver = this;
        }
    }

    public static void goToSavePos()
    {
        Destroy(_player);
        _player = Instantiate(_playerPrefab, currentSaver.transform.position, Quaternion.identity, null);
        _camera.getPlayer();
    }
}
