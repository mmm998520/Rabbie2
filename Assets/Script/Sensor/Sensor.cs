using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Sensor
{
    public class Sensor : MonoBehaviour
    {
        [Tooltip("隨你要寫啥，單純筆記"), TextArea()]
        [SerializeField] string tips;

        public delegate void SensorChangeHandler(Sensor sensor, SensorDataType type, SensorData data);
        public event SensorChangeHandler sensorChangeHandler;

        [Header("傳值類型與預設值")]
        public SensorData data = new SensorData();
        [Header("傳值類型與預設值")]
        public SensorDataType dataType;
        public bool presetTrigger;
        public bool presetBool;
        public float presetValue01;

        [Header("運作選項")]
        [Tooltip("是否只會運作一次")]
        public bool onlyUseOneTime;
        [Tooltip("間隔多少幀才可以再次被使用")]
        public int useCDFrames;
        [Tooltip("是否為可被觸發狀態")]
        public bool canUse = true;
        int delayFrame = 0;
        [SerializeField] int delayFrames;
        int CDFrame = 0;
        [SerializeField] int CDFrames;
        /*
        [Tooltip("是否為反向sensor，開啟時傳false值，關閉傳true值，數值傳1-value01")]
        public bool reverse;
        */

        protected virtual void setUp()
        {
            switch (dataType)
            {
                case SensorDataType.trigger:
                    data.trigger = presetTrigger;
                    if (data.trigger == true)
                    {
                        senserBroadcast();
                    }
                    break;
                case SensorDataType.switcher:
                    data.switcher = presetBool;
                    break;
                case SensorDataType.value01:
                    data.value01 = presetValue01;
                    break;
            }
        }

        public void senserBroadcast()
        {
            if (!canUse)
            {
                if (dataType == SensorDataType.trigger)
                {
                    data.trigger = false;
                }
                return;
            }

            if (GameManager._fixedFrame > delayFrame && GameManager._fixedFrame > CDFrame)
            {
                delayFrame = GameManager._fixedFrame + delayFrames;
                CDFrame = GameManager._fixedFrame + CDFrames;
                StartCoroutine(sensorDelay());
            }
            delayFrame = GameManager._fixedFrame + delayFrames;
            CDFrame = GameManager._fixedFrame + CDFrames;
        }

        void Broadcast()
        {
            sensorChangeHandler?.Invoke(this, dataType, data);
            if (dataType == SensorDataType.trigger)
            {
                data.trigger = false;
            }
            if (onlyUseOneTime)
            {
                canUse = false;
            }
        }

        WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        IEnumerator sensorDelay()
        {
            while (GameManager._fixedFrame < delayFrame)
            {
                yield return waitForFixedUpdate;
            }
            Broadcast();
            while (GameManager._fixedFrame < CDFrame)
            {
                yield return waitForFixedUpdate;
            }
        }
    }

    public enum SensorDataType
    {
        trigger,
        switcher,
        value01
    }

    public class SensorData
    {
        public bool trigger;
        public bool switcher;
        public float value01;

        public SensorData()
        {
            trigger = false;
            switcher = false;
            value01 = 0;
        }
    }
}
