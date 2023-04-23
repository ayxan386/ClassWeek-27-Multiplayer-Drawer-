using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI playerNameText;
    [SerializeField] public TextMeshProUGUI playerAnswers;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color drawer;
    [SerializeField] private Color defaultColor;

    private TextMeshProUGUI joinLog;

    public void SetUIName(string name)
    {
        playerNameText.text = name;
    }

    public void AddAnswerToUI(string word)
    {
        playerAnswers.text = word + "\n" + playerAnswers.text;
    }

    public void UpdateBackgroundColor(bool isDrawer)
    {
        backgroundImage.color = isDrawer ? drawer : defaultColor;
    }
}