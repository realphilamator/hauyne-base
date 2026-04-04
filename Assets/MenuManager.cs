using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public GameObject MainMenu;
    public GameObject SettingsMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Options()
    {
        Dither.SwapUI(MainMenu, SettingsMenu);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
