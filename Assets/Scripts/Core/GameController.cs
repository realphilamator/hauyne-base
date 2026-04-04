using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public CursorController cursorController;

    void Start()
    {
        cursorController.LockCursor();
    }
}
