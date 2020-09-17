using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private static bool showCursor = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursor(!showCursor);
        }
    }

    public static void ToggleCursor(bool toggle)
    {
        showCursor = toggle;

        if (showCursor)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = showCursor;
    }
}
