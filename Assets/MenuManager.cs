using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class MenuManager : MonoBehaviour
{

    public GameObject MainMenu;
    public GameObject SettingsMenu;
    private GameObject previousMenu;
    private GameObject currentMenu;

    // Start is called before the first frame update
    void Start()
    {
        currentMenu = MainMenu;
    }

    private Stack<GameObject> menuHistory = new Stack<GameObject>();

    [ReadOnly] public List<GameObject> debugHistory;

    public void NavigateTo(GameObject nextMenu)
    {
        Dither.SwapUI(currentMenu, nextMenu);
        debugHistory = new List<GameObject>(menuHistory); // mirror the stack
        menuHistory.Push(currentMenu);
        currentMenu = nextMenu;
    }

    public void GoToSettings()
    {
        NavigateTo(SettingsMenu);
    }

    public void Back()
    {
        if (menuHistory.Count == 0) return;
        GameObject prev = menuHistory.Pop();
        debugHistory = new List<GameObject>(menuHistory); // mirror the stack
        Dither.SwapUI(currentMenu, prev);
        currentMenu = prev;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
