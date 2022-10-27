using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Sensor;

namespace _Receiver
{
    public class CanUseSettingReceiver : Receiver
    {
        [SerializeField] bool settingBool;
        [SerializeField] Sensor[] sensors;
        [SerializeField] Receiver[] receivers;

        protected override void _use()
        {
            setCanUse(settingBool);
        }

        protected override void _useByBool()
        {
            bool temp = getValueSensorSwitcher();
            setCanUse(temp);
        }

        protected override void _useByValue01()
        {
            bool temp = getValueSensorValue01() != 0;
            setCanUse(temp);
        }

        void setCanUse(bool value)
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i].canUse = value;
            }
            for (int i = 0; i < receivers.Length; i++)
            {
                receivers[i].canUse = value;
            }
        }
    }
}
