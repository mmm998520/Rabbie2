using System.Collections;
using UnityEngine;

namespace TarodevController {
    public class CameraFollow : MonoBehaviour {
        [SerializeField] private Transform _player;
        [SerializeField] private float _smoothTime = 0.5f;
        [SerializeField] private float _minX, _maxX;
        [SerializeField] private float _minY, _maxY;

        private Vector3 _currentVel;
        private Vector3 _pos;

        private void Start() 
        {
            getPlayer();
        }

        public void getPlayer()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null) _player = player.transform;
        }

        private void Update() {
            if (!_player) return;
            _pos = _player.transform.position;
            var target = new Vector3(Mathf.Clamp(_player.position.x, _minX, _maxX), Mathf.Clamp(_player.position.y, _minY, _maxY), -10);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref _currentVel, _smoothTime);
        }
    }
}