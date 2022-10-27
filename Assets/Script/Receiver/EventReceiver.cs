using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Receiver
{
    public class EventReceiver : Receiver
    {
        public UnityEvent unityEvent = new UnityEvent();
        bool tempBool;
        float tempValue01;

        protected override void _use()
        {
            unityEvent.Invoke();
        }

        protected override void _useByBool()
        {
            tempBool = getValueSensorSwitcher();
            unityEvent.Invoke();
        }

        protected override void _useByValue01()
        {
            tempValue01 = getValueSensorValue01();
            unityEvent.Invoke();
        }
    }
}
