using UnityEngine;
using UnityEngine.UIElements;

namespace Menus
{
    public class UIGame : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;

        private VisualElement root;
        private VisualElement window;

        private Label score1;
        private Label score2;
        
        private Label lblWinning;
        private Button finishButton;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            root = uiDocument.rootVisualElement;
            window = root.Q<VisualElement>("resultWindow");

            score1 = root.Q<Label>("score1");
            score2 = root.Q<Label>("score2");
            
            lblWinning = root.Q<Label>("lblWinning");
            finishButton = root.Q<Button>("finishBtn");

            finishButton.clicked += OnFinishButton;
        }

        private void OnFinishButton()
        {
            UIController.Instance.SwitchMenu(UIController.MENU_STATE.LOGIN);
        }

        
    }
}