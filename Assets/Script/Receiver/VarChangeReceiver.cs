using UnityEngine;
using _Sensor;

namespace _Receiver
{
    public class VarChangeReceiver : Receiver
    {

        [Header("固定值傳遞選項")]
        [SerializeField] bool settingTrigger = true;
        [SerializeField] bool settingBool;
        [SerializeField] float settingValue01;

        [Header("傳值感應器列表")]
        [SerializeField] Sensor[] triggerSensors;
        [SerializeField] Sensor[] switcherSensors;
        [SerializeField] Sensor[] value01Sensors;

        [Header("是否在更改數值後進行播報")]
        [SerializeField] bool broadcastAfterChange;

        protected override void _use()
        {
            changeTriggers(settingTrigger);
            changeSwitchers(settingBool);
            changeValue01s(settingValue01);
        }

        protected override void _useByBool()
        {
            bool valueBool = getValueSensorSwitcher();
            float value01 = valueBool ? 0 : 1;
            changeTriggers(valueBool);
            changeSwitchers(valueBool);
            changeValue01s(value01);
        }

        protected override void _useByValue01()
        {
            bool valueBool = getValueSensorValue01() > 0;
            float value01 = getValueSensorValue01();
            changeTriggers(valueBool);
            changeSwitchers(valueBool);
            changeValue01s(value01);
        }

        void changeTriggers(bool value)
        {
            for (int i = 0; i < triggerSensors.Length; i++)
            {
                if (triggerSensors[i].dataType != SensorDataType.trigger)
                {
                    Debug.LogError("triggerSensors的第" + i + "個元件並非為trigger感應器", gameObject);
                }
                else
                {
                    triggerSensors[i].data.trigger = value;
                    if (broadcastAfterChange) triggerSensors[i].senserBroadcast();
                }
            }
        }

        void changeSwitchers(bool value)
        {
            for (int i = 0; i < switcherSensors.Length; i++)
            {
                if (switcherSensors[i].dataType != SensorDataType.switcher)
                {
                    Debug.LogError("switcherSensors的第" + i + "個元件並非為switcher感應器", gameObject);
                }
                else
                {
                    switcherSensors[i].data.switcher = value;
                    if (broadcastAfterChange) triggerSensors[i].senserBroadcast();
                }
            }
        }

        void changeValue01s(float value)
        {
            for (int i = 0; i < value01Sensors.Length; i++)
            {
                if (value01Sensors[i].dataType != SensorDataType.value01)
                {
                    Debug.LogError("value01Sensors的第" + i + "個元件並非為value01感應器", gameObject);
                }
                else
                {
                    value01Sensors[i].data.value01 = value;
                    if (broadcastAfterChange) triggerSensors[i].senserBroadcast();
                }
            }
        }
    }
}
