using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;
using UnityEngine.InputSystem;

public class InputSystemMenuController : MonoBehaviour
{
    public List<SimpleMenuManager> menus = new List<SimpleMenuManager>();

    bool gamepadConnected = false;

    private void Update()
    {
        if (Gamepad.current != null && !gamepadConnected)
        {
            for(int i = 0; i < menus.Count; ++i)
            {
                menus[i].SelectFirstUIObject = true;
            }

            gamepadConnected = true;

            Cursor.visible = false;
        } 
        else if (Gamepad.current == null && gamepadConnected)
        {
            for (int i = 0; i < menus.Count; ++i)
            {
                menus[i].SelectFirstUIObject = false;
            }

            gamepadConnected = false;

            Cursor.visible = true;
        }


    }
}
