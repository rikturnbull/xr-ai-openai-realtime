using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public class CubeBehaviour : MonoBehaviour
    {
        private bool _isMoving = false;
        private Queue<Vector3> _path = new();
        private float _speed = 5.0f;

        public void MoveToCoordinates(List<Vector3> coordinates)
        {
            foreach (var coord in coordinates)
            {
                _path.Enqueue(new Vector3(coord.x, coord.y, coord.z));
            }
            _isMoving = true;
        }

        void Update()
        {
            if (_isMoving && _path.Count > 0)
            {
                Vector3 targetPosition = _path.Peek();
                float step = _speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

                if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
                {
                    _path.Dequeue();
                    if (_path.Count == 0)
                    {
                        _isMoving = false;
                    }
                }
            }
        }
    }
}