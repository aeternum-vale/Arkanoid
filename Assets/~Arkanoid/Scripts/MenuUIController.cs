using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIController : MonoBehaviour
{
    public event Action NewGameButtonClick;
    public event Action ContinueButtonClick;
    public event Action QuitButtonClick;

    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _quitButton;

    private void Awake()
    {
        AddListeners();
    }

    private void AddListeners()
    {
        _newGameButton.onClick.AddListener(OnNewGameButtonClick);
        _continueButton.onClick.AddListener(OnContinueButtonClick);
        _quitButton.onClick.AddListener(OnQuitButtonClick);
    }
    private void RemoveListeners()
    {
        _newGameButton.onClick.RemoveListener(OnNewGameButtonClick);
        _continueButton.onClick.RemoveListener(OnContinueButtonClick);
        _quitButton.onClick.RemoveListener(OnQuitButtonClick);
    }

    public void SetContinueButtonActive()
    {
        _continueButton.interactable = true;
    }

    private void OnNewGameButtonClick() => NewGameButtonClick?.Invoke();
    private void OnContinueButtonClick() => ContinueButtonClick?.Invoke();
    private void OnQuitButtonClick() => QuitButtonClick?.Invoke();

    private void OnDestroy() => RemoveListeners();
}
