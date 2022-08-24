using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUpController : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Ball _ball;

    [Space]
    [SerializeField] private float _avgPowerUpIntervalSec = 5f;

    private Dictionary<EPowerUpType, int> _powerUpCount = new Dictionary<EPowerUpType, int>();
    private EPowerUpType[] _powerUpValues;
    private CancellationTokenSource _cts;
    private float _animationDuration = 1f;
    private List<Tween> _tweens = new List<Tween>();

    private void Awake()
    {
        _powerUpValues = (EPowerUpType[])Enum.GetValues(typeof(EPowerUpType));
        foreach (EPowerUpType pu in _powerUpValues)
            _powerUpCount.Add(pu, 0);
    }

    public void RestoreInitialState()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        ClearCounts();

        foreach (var t in _tweens)
            t.Kill();

        _tweens.Clear();
    }

    private void ClearCounts()
    {
        foreach (EPowerUpType pu in _powerUpValues)
            _powerUpCount[pu] = 0;
    }

    [Button]
    public async void PowerUp()
    {
        EPowerUpType powerUpType = (EPowerUpType)_powerUpValues.GetValue(Random.Range(1, _powerUpValues.Length));
        Debug.Log($"powerUpType={powerUpType}");

        float intervalSec = EnablePowerUp(powerUpType);
        await UniTask.Delay(TimeSpan.FromSeconds(intervalSec), cancellationToken: _cts.Token);
        DisablePowerUp(powerUpType);
    }

    private float EnablePowerUp(EPowerUpType powerUpType)
    {
        _powerUpCount[powerUpType]++;
        float intervalSec = _avgPowerUpIntervalSec;

        switch (powerUpType)
        {
            case EPowerUpType.AlmightyBall:
                _ball.IsAlmighty = true;
                break;

            case EPowerUpType.WiderSlider:
                Animate(() => _slider.Width, v => _slider.Width = v, _slider.Width * 2f);
                intervalSec *= 2;
                break;

            case EPowerUpType.Boost:
                _ball.IsBoosted = true;
                Animate(
                    () => _slider.Width, v => _slider.Width = v, _slider.BoostedWidth,
                        () => Animate(() => _ball.Speed, v => _ball.Speed = v, _ball.InitialSpeed * 5f));
                intervalSec /= 1.5f;
                break;

            default:
                throw new Exception("invalid power-up type");
        }

        return intervalSec;
    }

    private void DisablePowerUp(EPowerUpType powerUpType)
    {
        _powerUpCount[powerUpType]--;

        if (_powerUpCount[powerUpType] != 0) return;

        switch (powerUpType)
        {
            case EPowerUpType.AlmightyBall:
                _ball.IsAlmighty = false;
                break;

            case EPowerUpType.WiderSlider:
                Animate(() => _slider.Width, v => _slider.Width = v, _slider.InitialWidth);
                break;

            case EPowerUpType.Boost:
                _ball.IsBoosted = false;
                Animate(
                    () => _ball.Speed, v => _ball.Speed = v, _ball.InitialSpeed,
                        () => Animate(() => _slider.Width, v => _slider.Width = v, _slider.InitialWidth));
                break;

            default:
                throw new Exception("invalid power-up type");
        }
    }

    public void Animate(DOGetter<float> getter, DOSetter<float> setter, float target, Action onComplete = null)
    {
        _tweens.Add(DOTween.To(getter, setter, target, _animationDuration).OnComplete(() => onComplete?.Invoke()));
    }
}