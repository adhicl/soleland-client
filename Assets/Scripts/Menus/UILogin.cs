using System;
using UnityEditor.VersionControl;
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
        private Label lblError;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            root = uiDocument.rootVisualElement;
            window = root.Q<VisualElement>("loginWindow");

            tAddress = window.Q<TextField>("tAddress");
            tPort = window.Q<TextField>("tPort");
            lblError = window.Q<Label>("lblError");

            tAddress.value = NetworkController.Instance.host;
            tPort.value = NetworkController.Instance.tcpPort.ToString();

            joinButton = window.Q<Button>("joinBtn");

            joinButton.clicked += OnJoinButton;

            NetworkController.Instance.onError += OnErrorMessage;
            NetworkController.Instance.onConnected += OnGameStart;
            NetworkController.Instance.onDisconnected += OnGameOver;

            ShowJoinButton(true);
            lblError.style.display = DisplayStyle.None;
        }

        private void OnDestroy()
        {
            NetworkController.Instance.onError -= OnErrorMessage;
            NetworkController.Instance.onConnected -= OnGameStart;
            NetworkController.Instance.onDisconnected -= OnGameOver;
        }

        private void OnJoinButton()
        {
            //UIController.Instance.SwitchMenu(UIController.MENU_STATE.GAME);
            //GameController.Instance.GameStart();

            NetworkController.Instance.Connect(tAddress.value, int.Parse(tPort.value));
            ShowJoinButton(false);

            OnErrorMessage("Connecting to server");
        }

        private void ShowJoinButton(bool show)
        {
            if (show)
            {
                joinButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                joinButton.style.display = DisplayStyle.None;
            }
        }

        private void OnGameStart()
        {
            window.AddToClassList("hide");
            //UIController.Instance.SwitchMenu(UIController.MENU_STATE.GAME);
            //GameController.Instance.GameStart();
        }

        private void OnGameOver()
        {
            //UIController.Instance.SwitchMenu(UIController.MENU_STATE.LOGIN);
            window.RemoveFromClassList("hide");
            ShowJoinButton(true);
        }

        private void OnErrorMessage(string message)
        {
            lblError.style.display = DisplayStyle.Flex;
            lblError.text = message;
        }
    }
}
