using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private MenuUIController _menuUiController;
    private SessionSaver _sessionSaver;

    private void Awake()
    {
        AddListeners();
        _sessionSaver = new SessionSaver();

        if (_sessionSaver.IsSessionFileExist())
            _menuUiController.SetContinueButtonActive();

        PlayerPrefs.SetInt(Constants.ContinueModeKey, 0);
    }

    private void AddListeners()
    {
        _menuUiController.NewGameButtonClick += OnNewGameButtonClick;
        _menuUiController.ContinueButtonClick += OnContinueButtonClick;
        _menuUiController.QuitButtonClick += OnQuitButtonClick;
    }

    private void RemoveListeners()
    {
        _menuUiController.NewGameButtonClick -= OnNewGameButtonClick;
        _menuUiController.ContinueButtonClick -= OnContinueButtonClick;
        _menuUiController.QuitButtonClick -= OnQuitButtonClick;
    }

    private void OnQuitButtonClick()
    {
        Application.Quit();
    }

    private void OnContinueButtonClick()
    {
        PlayerPrefs.SetInt(Constants.ContinueModeKey, 1);
        LoadGameScene();
    }

    private void OnNewGameButtonClick()
    {
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(Constants.GameSceneIndex);
    }

    private void OnDestroy() => RemoveListeners();
}