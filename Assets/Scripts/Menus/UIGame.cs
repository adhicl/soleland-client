using UnityEngine;
using UnityEngine.SceneManagement;
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

            score1 = root.Q<Label>("Score1");
            score2 = root.Q<Label>("Score2");

            score1.text = "0";
            score2.text = "0";
            
            lblWinning = root.Q<Label>("lblWinning");
            finishButton = root.Q<Button>("finishBtn");

            finishButton.clicked += OnFinishButton;

            GameController.Instance.OnGameScore += UpdateScore;
            GameController.Instance.OnGameOver += ShowResult;
        }

        private void OnFinishButton()
        {
            //UIController.Instance.SwitchMenu(UIController.MENU_STATE.LOGIN);
            NetworkController.Instance.Disconnect();
            SceneManager.LoadScene(0);
        }

        private void UpdateScore(int position, int score)
        {
            if (position == 0)
            {
                score1.text = score.ToString();
            }
            else
            {
                score2.text = score.ToString();
            }
        }

        private void ShowResult(int position)
        {
            lblWinning.text = "Player "+(position + 1).ToString()+ " Win!";
            window.RemoveFromClassList("hide");
        }
        
        
    }
}