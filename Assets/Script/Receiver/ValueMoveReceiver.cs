using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Receiver
{
    public class ValueMoveReceiver : Receiver
    {
        [SerializeField] Vector2[] points;
        List<float> startToPointDistances = new List<float>();
        void Start()
        {
            startToPointDistances.Add(0);

            if (points.Length >= 2)
            {
                for (int i = 0; i < points.Length - 1; i++)
                {
                    startToPointDistances.Add(startToPointDistances[i] + Vector2.Distance(points[i], points[i + 1]));
                }
            }
            else
            {
                Debug.LogError("移動裝置至少要有兩個point繪製為路徑", gameObject);
            }
        }

        protected override void _useByValue01()
        {
            float value01 = getValueSensorValue01();
            float length = Mathf.Lerp(0, startToPointDistances[startToPointDistances.Count - 1], value01);
            for (int i = 0; i < startToPointDistances.Count; i++)
            {
                if (length > startToPointDistances[i])
                {
                    length -= startToPointDistances[i];
                }
                else
                {
                    transform.position = Vector2.Lerp(points[i], points[i - 1], length / Vector2.Distance(points[i], points[i - 1]));
                }
            }
        }
    }
}
