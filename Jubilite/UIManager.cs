using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Systems;
using DG.Tweening;
using RhythmGameStarter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public enum UIElement
{
    Battery,
    ProgressBar,
    StatText,
    ComboText,
    MultiplierText,
    ScoreText,
    BreakText,
    CountDownText,
    PauseScoreText,
    PauseProgressText,
    GameOverScoreText,
    GameOverProgressText,
    GameOverPerfects,
    GameOverGoods,
    GameOverOks,
    GameOverMisses,
    
}

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider batteryPower, progressBar;
    [SerializeField] private TMP_Text statText, comboText, scoreText, breakText, countdownText, multiplierText;
    [SerializeField] private TMP_Text pauseScoreText, pauseProgressText;
    [SerializeField] private TMP_Text gameOverScoreText, gameOverProgressText, gameOverPerfectsText, gameOverGoodsText, gameOverOksText, gameOverMissesText;
    private float _modifier = 1;
    
    public static event OnPlayerDeath onPlayerDeath;
    public delegate void OnPlayerDeath();
    
    private void OnEnable()
    {
        StatsSystem.onUpdateSlider += ChangeSlider;
        StatsSystem.onUpdateText += UpdateTextEvent;
        StatsSystem.onPunchText += PunchText;
        StatsSystem.onShakeText += ShakeText;
        StatsSystem.onUpdateColor += UpdateColor;

        StatsSystem.onEnableDisableUI += EnableDisableUI;
    }

    private void OnDisable()
    {
        StatsSystem.onUpdateSlider -= ChangeSlider;
        StatsSystem.onUpdateText -= UpdateTextEvent;
        StatsSystem.onPunchText -= PunchText;
        StatsSystem.onShakeText -= ShakeText;
        StatsSystem.onUpdateColor -= UpdateColor;

        StatsSystem.onEnableDisableUI -= EnableDisableUI;
    }
    void Start()
    {
        batteryPower.value = batteryPower.maxValue;
    }

    void Update()
    {
        if (batteryPower.value < 25f)
        {
            UpdateColor(UIElement.Battery,Color.red);
        }
        if (batteryPower.value > 25f)
        {
            UpdateColor(UIElement.Battery,Color.magenta);
        }
        if (batteryPower.value <= 0)
        {
            InvokeOnPlayerDeath();
        }
    }

    public static void InvokeOnPlayerDeath()
    {
        onPlayerDeath?.Invoke();
    }

    public void EnableDisableUI(UIElement UIToUpdate, bool change)
    {
        GetUIElement(UIToUpdate).SetActive(change);
    }

    public void ChangeSlider(UIElement sliderToUpdate, float change, bool lerp)
    {
        switch (sliderToUpdate)
        {
            case UIElement.Battery:
                if (lerp)
                {
                    StartCoroutine(LerpSlider(sliderToUpdate, batteryPower.value, batteryPower.value += (int)change * _modifier));
                }
                else
                {
                    batteryPower.value += (int)change * _modifier;
                }
                if (change < 0)
                {
                    
                }
                break;
            case UIElement.ProgressBar:
                if (lerp)
                {
                    StartCoroutine(LerpSlider(sliderToUpdate, progressBar.value, (int) change * _modifier));
                }
                else
                {
                    progressBar.value = change;
                }
                break;
        }
        
        UpdateTextEvent(UIElement.PauseProgressText, "Completed: " + Mathf.RoundToInt(progressBar.value * 100f) + "%");
        UpdateTextEvent(UIElement.GameOverProgressText, "Completed: " + Mathf.RoundToInt(progressBar.value * 100f) + "%");
    }
    IEnumerator MoveObject(Vector3 source, Vector3 target, float overTime)
    {
        float startTime = Time.time;
        while(Time.time < startTime + overTime)
        {
            transform.position = Vector3.Lerp(source, target, (Time.time - startTime)/overTime);
            yield return null;
        }
        transform.position = target;
    }
    private IEnumerator LerpSlider(UIElement sliderToUpdate, float _initial, float _target)
    {
        float _curr = 0;

        float _timer = 0.15f;
        
        float startTime = Time.time;
        while(Time.time < startTime + _timer)
        {
            _curr = Mathf.Lerp(_initial, _target, (Time.time - startTime)/_timer);
            switch (sliderToUpdate)
            {
                case UIElement.Battery:
                    batteryPower.value = _curr;
                    break;
                case UIElement.ProgressBar:
                    progressBar.value = _curr;
                    break;
            }
            yield return null;
        }

        switch (sliderToUpdate)
        {
            case UIElement.Battery:
                batteryPower.value = _target;
                break;
            case UIElement.ProgressBar:
                progressBar.value = _target;
                break;
        }
    }
    
    public void UpdateTextEvent(UIElement textToUpdate, string name)
    {
        GetUIElement(textToUpdate).GetComponent<TMP_Text>().text = name;
    }

    public void UpdateColor(UIElement textToUpdate, Vector4 change)
    {
        if (textToUpdate == UIElement.Battery)
        {
            GetUIElement(textToUpdate).GetComponent<Slider>().fillRect.GetComponent<Image>().color = change;
            return;
        }
        var shader = GetUIElement(textToUpdate).GetComponent<TMP_Text>().fontMaterial;

        shader.SetColor("_FaceColor", change);
        shader.SetColor("_GlowColor", change);
    }
    
    public void PunchText(UIElement textToShake, float punchY, int vibrato = 1, float duration = .8f, float elasticity = .8f, bool snapping = false)
    {
        DOTween.KillAll(true);
        Vector3 _punch = new Vector3(0f, punchY, 0f);
        GetUIElement(textToShake).transform.DOPunchPosition(_punch, duration, vibrato, elasticity, snapping);
    }
    
    public void ShakeText(UIElement textToShake, float shakeIntensity)
    {
        DOTween.KillAll(true);
        GetUIElement(textToShake).transform.DOShakePosition(.2f, shakeIntensity, 10, 90, true, false);
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    
    public void ReloadScene()
    {
        GameManager.ReloadScene();
    }
    public GameObject GetUIElement(UIElement element)
    {
        switch (element)
        {
            case UIElement.Battery:
                return batteryPower.gameObject;
            case UIElement.BreakText:
                return breakText.gameObject;
            case UIElement.ComboText:
                return comboText.gameObject;
            case UIElement.ProgressBar:
                return progressBar.gameObject;
            case UIElement.ScoreText:
                return scoreText.gameObject;
            case UIElement.StatText:
                return statText.gameObject;
            case UIElement.CountDownText:
                return countdownText.gameObject;
            case UIElement.PauseProgressText:
                return pauseProgressText.gameObject;
            case UIElement.PauseScoreText:
                return pauseScoreText.gameObject;
            case UIElement.GameOverProgressText:
                return gameOverProgressText.gameObject;
            case UIElement.GameOverScoreText:
                return gameOverScoreText.gameObject;
            case UIElement.MultiplierText:
                return multiplierText.gameObject;
            case UIElement.GameOverPerfects:
                return gameOverPerfectsText.gameObject;
            case UIElement.GameOverGoods:
                return gameOverGoodsText.gameObject;
            case UIElement.GameOverOks:
                return gameOverOksText.gameObject;
            case UIElement.GameOverMisses:
                return gameOverMissesText.gameObject;
        }

        return null;
    }
}