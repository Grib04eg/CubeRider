using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Controller player;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI multiplierText;
    [SerializeField] TextMeshProUGUI distanceText;
    [SerializeField] CanvasGroup loseGroup;
    [SerializeField] TextMeshProUGUI loseScoreText;

    float score;
    float distance;
    float lastMultiplierPosition;
    float multiplier;
    public void AddScore(float amount)
    {
        score += amount;
        scoreText.text = "Score: " + score.ToString("0");
    }
    float bonusScore = 0;
    public void AddBonusScore(float amount)
    {
        AddScore(amount);
        bonusScore += amount;
    }

    private void Start()
    {
        loseGroup.alpha = 0f;
        loseGroup.gameObject.SetActive(false);
    }

    void Update()
    {
        var position = player.transform.position.x;
        var positionDelta = position - lastMultiplierPosition;

        multiplier = positionDelta / Time.deltaTime;
        if (multiplier < 1)
            multiplier = 1;
        multiplierText.text = "multiplier: " + multiplier.ToString("0.0") + "x";
        lastMultiplierPosition = position;

        if (position > distance)
        {
            AddScore(positionDelta * multiplier);
            distance = position;
            distanceText.text = "Distance: " + distance.ToString("0") + "m";
        }
    }

    public void Lose()
    {
        loseGroup.gameObject.SetActive(true);
        loseGroup.DOFade(1, 0.2f);
        var averageMultiplier = (score - bonusScore) / distance;
        loseScoreText.text = $"Score: {score.ToString("0")}!\nDistance: {distance.ToString("0")}m\nAverage multiplier: {averageMultiplier.ToString("0.0")}x\nBonus score: {bonusScore.ToString("0")}\nMission: ? / &";
    }
}
