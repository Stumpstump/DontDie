using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    private void Awake()
    {
        LockMouse(CursorLockMode.Locked);
        HideMouse(true);
    }

    public void LockMouse(CursorLockMode Status)
    {
        Cursor.lockState = Status;
    }

    public void HideMouse(bool Status)
    {
        Cursor.visible = Status;
    }
}
