using UnityEngine;
using UnityEngine.UIElements;

namespace Menus
{
    public class UILogin : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;

        private VisualElement root;
        private VisualElement window;

        private TextField tAddress;
        private TextField tPort;
        private Button joinButton;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            root = uiDocument.rootVisualElement;
            window = root.Q<VisualElement>("loginWindow");

            tAddress = window.Q<TextField>("tAddress");
            tPort = window.Q<TextField>("tPort");

            joinButton = window.Q<Button>("joinBtn");

            joinButton.clicked += OnJoinButton;
        }

        private void OnJoinButton()
        {
            UIController.Instance.SwitchMenu(UIController.MENU_STATE.GAME);
            GameController.Instance.GameStart();
        }

    }
}
