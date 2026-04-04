using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsManager : MonoBehaviour
{
    [System.Serializable]
    public class SettingsMenu
    {
        [TextArea(1, 3)]
        public string menuName;
        public GameObject panel;
    }

    public SettingsMenu[] menus;
    private int currentIndex = 0;

    [Header("Header UI")]
    public TextMeshProUGUI currentMenuLabel;
    public TextMeshProUGUI prevMenuLabel;
    public TextMeshProUGUI nextMenuLabel;


    [Header("Arrow Buttons")]
    public Button prevButton;
    public Button nextButton;

    void Start()
    {
        prevButton.onClick.AddListener(OnPrevPressed);
        nextButton.onClick.AddListener(OnNextPressed);
        ShowMenu(currentIndex);
    }

    public void OnNextPressed()
    {
        currentIndex = (currentIndex + 1) % menus.Length;
        ShowMenu(currentIndex);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPrevPressed()
    {
        currentIndex = (currentIndex - 1 + menus.Length) % menus.Length;
        ShowMenu(currentIndex);
        EventSystem.current.SetSelectedGameObject(null);
    }

    void ShowMenu(int index)
    {
        // Hide all panels
        foreach (var menu in menus)
            menu.panel.SetActive(false);

        // Show active panel
        menus[index].panel.SetActive(true);

        // Update header labels
        currentMenuLabel.text = menus[index].menuName.Replace("\\n", "\n");

        int prevIndex = (index - 1 + menus.Length) % menus.Length;
        int nextIndex = (index + 1) % menus.Length;
        
        prevMenuLabel.text = menus[prevIndex].menuName.Replace("\\n", "\n");
        nextMenuLabel.text = menus[nextIndex].menuName.Replace("\\n", "\n");
    }
}