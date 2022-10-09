using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedUpdateInputManager : MonoBehaviour
{
    public static float horizontal;
    public static float vertical;
    public class Key
    {
        public bool down, stay, up;
        public KeyCode keyCode;

        public Key(KeyCode keyCode)
        {
            down = false;
            stay = false;
            up = false;
        }
    }

    public static Key space = new Key(KeyCode.Space);
    public static Key leftShift = new Key(KeyCode.LeftShift);
    public static Key tab = new Key(KeyCode.Tab);


    private void Start()
    {
        LateFixedUpdate.lateFixedUpdate.lateFixedUpdateLog += new LateFixedUpdate.LateFixedUpdateEventHandler(lateFixedUpdate);
    }

    void Update()
    {
        axisInput();
        getKeyDown(space);
        getKeyDown(leftShift);
        getKeyDown(tab);
        getKeyUp(space);
        getKeyUp(leftShift);
        getKeyUp(tab);
    }

    void lateFixedUpdate()
    {
        resetKeyDown(space);
        resetKeyDown(leftShift);
        resetKeyDown(tab);
        resetKeyUp(space);
        resetKeyUp(leftShift);
        resetKeyUp(tab);
    }

    public static void axisInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Horizontal");
    }

    public static void getKeyDown(Key key)
    {
        if (Input.GetKeyDown(key.keyCode))
        {
            key.down = true;
            key.stay = true;
        }
    }
    public static void getKeyDown(Key key, bool useStay)
    {
        if (Input.GetKeyDown(key.keyCode))
        {
            key.down = true;
            if (useStay)
            {
                key.stay = true;
            }
        }
    }
    public static void resetKeyDown(Key key)
    {
        key.down = false;
    }
    public static void getKeyUp(Key key)
    {
        if (Input.GetKeyDown(key.keyCode))
        {
            key.up = true;
            key.stay = true;
        }
    }
    public static void getKeyUp(Key key, bool useStay)
    {
        if (Input.GetKeyDown(key.keyCode))
        {
            key.up = true;
            if (useStay)
            {
                key.stay = true;
            }
        }
    }
    public static void resetKeyUp(Key key)
    {
        key.up = false;
    }
}
