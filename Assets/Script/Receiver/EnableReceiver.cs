using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Receiver
{
    public class EnableReceiver : Receiver
    {
        [SerializeField] bool settingBool;
        [SerializeField] Behaviour[] behaviours;
        [SerializeField] GameObject[] gameObjects;

        protected override void _use()
        {
            setEnables(settingBool);
            setActives(settingBool);
        }

        protected override void _useByBool()
        {
            bool temp = getValueSensorSwitcher();
            setEnables(temp);
            setActives(temp);
        }

        protected override void _useByValue01()
        {
            bool temp = getValueSensorValue01() != 0;

            setEnables(temp);
            setActives(temp);
        }

        void setEnables(bool value)
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                behaviours[i].enabled = value;
            }
        }

        void setActives(bool value)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].SetActive(value);
            }
        }
    }
}
