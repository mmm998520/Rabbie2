using System.Collections;
using UnityEngine;
using _Sensor;

namespace _Receiver
{
    public class Receiver : MonoBehaviour
    {
        [Tooltip("隨你要寫啥，單純筆記"), TextArea()]
        [SerializeField] string tips;

        public bool canUse = true;
        [SerializeField] Sensor[] sensors;
        [SerializeField] protected Sensor getValueSensor;
        [Tooltip("是否將getValueSensor接收到的數值反轉")]
        [SerializeField] bool reverse = false;
        int delayFrame = 0;
        [SerializeField] int delayFrames;
        int CDFrame = 0;
        [SerializeField] int CDFrames;

        void Awake()
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i].sensorChangeHandler += sensorChanged;
            }
        }

        void sensorChanged(Sensor sensor, SensorDataType dataType, SensorData data)
        {
            if (!canUse)
            {
                return;
            }

            if (GameManager._fixedFrame > delayFrame && GameManager._fixedFrame > CDFrame)
            {
                for (int i = 0; i < sensors.Length; i++)
                {
                    switch (sensors[i].dataType)
                    {
                        case SensorDataType.trigger:
                            if (!sensors[i].data.trigger)
                            {
                                return;
                            }
                            break;
                        case SensorDataType.switcher:
                            if (!sensors[i].data.switcher)
                            {
                                return;
                            }
                            break;
                    }
                }

                delayFrame = GameManager._fixedFrame + delayFrames;
                CDFrame = GameManager._fixedFrame + CDFrames;
                StartCoroutine(receiverDelay());
            }
            delayFrame = GameManager._fixedFrame + delayFrames;
            CDFrame = GameManager._fixedFrame + CDFrames;
            print("fixedFrame" + GameManager._fixedFrame);
            print("delayFrame" + delayFrame);
            print("CDFrame" + CDFrame);
        }

        WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        IEnumerator receiverDelay()
        {
            while(GameManager._fixedFrame < delayFrame)
            {
                yield return waitForFixedUpdate;
            }
            useReceiver();
            print("a");
            while (GameManager._fixedFrame < CDFrame)
            {
                yield return waitForFixedUpdate;
            }
            print("b");
        }

        void useReceiver()
        {
            if (getValueSensor)
            {
                switch (getValueSensor.dataType)
                {
                    case SensorDataType.switcher:
                        _useByBool();
                        break;
                    case SensorDataType.value01:
                        _useByValue01();
                        break;
                    default:
                        _use();
                        break;
                }
            }
            else
            {
                _use();
            }
        }

        protected virtual void _use()
        {

        }

        protected virtual void _useByBool()
        {

        }

        protected virtual void _useByValue01()
        {

        }

        protected bool getValueSensorSwitcher()
        {
            return reverse ? !getValueSensor.data.switcher : getValueSensor.data.switcher;
        }

        protected float getValueSensorValue01()
        {
            return reverse ? 1 - getValueSensor.data.value01 : getValueSensor.data.value01;
        }
        /*
        接收器上要掛有一個感應器的array
        當array內的感應器皆為觸發狀態時接收器才會運作(監聽器偵測到array內的感應器有變動時做檢查)
        接收器有個感應器欄位可以接取指定接收器的數值，其餘array內的感應器將僅作為觸發使用

        接收器將依據設定觸發機關(們)，並給予機關(們)在感應器欄位所得到的值作為引用
        ****感應器也可作為被觸發的機關****

        機關上有各種運作模式包含但不限於
        1.等速來回移動
        2.依據接收值移動位置
        3.移動到指定位置
        4.等速旋轉
        5.依據接收值旋轉
        6.旋轉至指定角度
        7.消失、出現
        8.****依據接收值觸發感應器****
        9.依據設定時間觸發感應器(可以做到定時開關)
        10.開啟/關閉感應器
         */
    }
}
