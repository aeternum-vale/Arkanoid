using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public event Action PauseMenuBackButtonClick;
    public event Action PauseMenuSaveButtonClick;
    public event Action PauseMenuQuitButtonClick;

    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _livesText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _highscoreText;

    [Header("Pause")]
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private Button _pauseMenuBackButton;
    [SerializeField] private Button _pauseMenuSaveButton;
    [SerializeField] private Button _pauseMenuQuitButton;
    [SerializeField] private GameObject _sessionSavedMessage;

    public int Level { set => _levelText.text = $"Level: {value}"; }
    public int Lives { set => _livesText.text = $"Lives: {value}"; }
    public int Score { set => _scoreText.text = $"Score: {value}"; }
    public int Highscore { set => _highscoreText.text = $"Highscore: {value}"; }


    private void Awake()
    {
        AddListeners();
    }

    private void AddListeners()
    {
        _pauseMenuBackButton.onClick.AddListener(OnPauseMenuBackButtonClicked);
        _pauseMenuSaveButton.onClick.AddListener(OnPauseMenuSaveButtonClicked);
        _pauseMenuQuitButton.onClick.AddListener(OnPauseMenuQuitButtonClicked);
    }

    private void RemoveListeners()
    {
        _pauseMenuBackButton.onClick.RemoveListener(OnPauseMenuBackButtonClicked);
        _pauseMenuSaveButton.onClick.RemoveListener(OnPauseMenuSaveButtonClicked);
        _pauseMenuQuitButton.onClick.RemoveListener(OnPauseMenuQuitButtonClicked);
    }

    private void OnPauseMenuQuitButtonClicked()
    {
        PauseMenuQuitButtonClick?.Invoke();
    }

    private void OnPauseMenuSaveButtonClicked()
    {
        PauseMenuSaveButtonClick?.Invoke();
    }

    private void OnPauseMenuBackButtonClicked()
    {
        PauseMenuBackButtonClick?.Invoke();
    }

    public void ShowPauseMenu()
    {
        _pauseMenu.SetActive(true);
    }

    public void HidePauseMenu()
    {
        _pauseMenu.SetActive(false);
    }

    public async void ShowSessionSavedMessage()
    {
        _sessionSavedMessage.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(3f), DelayType.Realtime);
        _sessionSavedMessage.SetActive(false);
    }

    private void OnDestroy() => RemoveListeners();

}
