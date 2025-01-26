using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    #region singleton

    public static UIController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    #endregion

    [SerializeField] UIDocument loginMenu;
    [SerializeField] UIDocument gameMenu;

    private VisualElement loginRoot;
    private VisualElement gameRoot;

    public enum MENU_STATE: byte
    {
        LOGIN,
        GAME
    };

    private MENU_STATE menuState = MENU_STATE.LOGIN;

    private void Start()
    {
        loginRoot = loginMenu.rootVisualElement;
        gameRoot = gameMenu.rootVisualElement;
        
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        switch (menuState)
        {
            case MENU_STATE.LOGIN:
                loginRoot.style.display = DisplayStyle.Flex;
                gameRoot.style.display = DisplayStyle.None;
                break;
            case MENU_STATE.GAME:
                loginRoot.style.display = DisplayStyle.None;
                gameRoot.style.display = DisplayStyle.Flex;
                break;
        }
    }

    public void SwitchMenu(MENU_STATE newState)
    {
        menuState = newState;
        UpdateMenu();
    }
}