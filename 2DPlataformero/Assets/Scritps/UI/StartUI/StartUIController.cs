using UnityEngine;
using UnityEngine.UI;

public class StartUIController : MonoBehaviour
{
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private GameObject multiplayerGO;
    [SerializeField] private GameObject optionsGO;
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private RectTransform gameNameText;
    [SerializeField] private GameObject gameNameTextGO;

    public void OnClickMultiplayerButton()
    {
        multiplayerPanel.SetActive(true);
        multiplayerGO.SetActive(false);
        optionsGO.SetActive(false);
        gameNameText.anchoredPosition = new Vector2(0, 225);
    }

    public void OnClickMultiplayerBackButton()
    {
        multiplayerGO.SetActive(true);
        optionsGO.SetActive(true);
        multiplayerPanel.SetActive(false);
        gameNameText.anchoredPosition = new Vector2(0, 87);
    }

    public void OnClickOptionsButton()
    {
        optionsPanel.SetActive(true);
        optionsGO.SetActive(false);
        multiplayerGO.SetActive(false);
        gameNameTextGO.SetActive(false);
    }

    public void OnClickOptionsBackButton()
    {
        optionsGO.SetActive(true);
        multiplayerGO.SetActive(true);
        optionsPanel.SetActive(false);
        gameNameTextGO.SetActive(true);
    }
}
