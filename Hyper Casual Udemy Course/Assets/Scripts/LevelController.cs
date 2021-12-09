﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{

    public static LevelController Current;
    public bool gameActive = false;

    public GameObject startMenu, gameMenu, gameOverMenu, finishMenu;
    public Button rewardedAdButton;
    public Text scoreText, finishScoretext, currentLevelText, nextLevelText, startingMenuMoneyText, gameoverMenuMoneyText,finishGameMenuMoneyText;
    public Slider levelProgressBar;
    public float maxDistance;
    public GameObject finishLine;
    public AudioSource gameMusicAudioSource;
    public AudioClip victoryAudioClip, gameOverAudioClip;
    public DailyReward dailyReward;

    int currentLevel;
    public int score;
    // Start is called before the first frame update
    void Start()
    {
        if(Current == null)
        {
            Current = this;
        }

        currentLevel = PlayerPrefs.GetInt("currentLevel");
        PlayerController.Current = GameObject.FindObjectOfType<PlayerController>();
        GameObject.FindObjectOfType<MarketController>().InitializeMarketController();
        dailyReward.InitializeDailyReward();
        currentLevelText.text = (currentLevel + 1).ToString();
        nextLevelText.text = (currentLevel + 2).ToString();
        UpdateMoneyTexts();
        gameMusicAudioSource = Camera.main.GetComponent<AudioSource>();
        if (AdController.Current.IsRaedyInterstitialAd())
        {
            AdController.Current.interstitial.Show();
        }
    }

    public void ShowRewardedAd()
    {
        if (AdController.Current.rewardedAd.IsLoaded())
        {
            AdController.Current.rewardedAd.Show();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameActive)
        {
            PlayerController player = PlayerController.Current;
            float distance = finishLine.transform.position.z - PlayerController.Current.transform.position.z;
            levelProgressBar.value = 1-(distance/maxDistance);
        }
    }

    public void StartLevel()
    {
        AdController.Current.bannerView.Hide();
        maxDistance = finishLine.transform.position.z - PlayerController.Current.transform.position.z;
        PlayerController.Current.ChangeSpeed(PlayerController.Current.runningSpeed);
        startMenu.SetActive(false);
        gameMenu.SetActive(true);
        PlayerController.Current.animator.SetBool("running", true);
        gameActive = true;
    }

    public void RestartLevel()
    {
        LevelLoader.Current.ChangeLevel(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        LevelLoader.Current.ChangeLevel("Level " + (currentLevel + 1));
    }

    public void GameOver()
    {
        if (AdController.Current.IsRaedyInterstitialAd())
        {
            AdController.Current.interstitial.Show();
        }
        AdController.Current.bannerView.Show();
        UpdateMoneyTexts();
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(gameOverAudioClip);
        gameMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        gameActive = false;
    }

    public void FinishGame()
    {
        if (AdController.Current.rewardedAd.IsLoaded())
        {
            rewardedAdButton.gameObject.SetActive(true);
        }
        else
        {
            rewardedAdButton.gameObject.SetActive(false);
        }
        AdController.Current.bannerView.Show();
        GiveMoneyToPlayer(score);
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(victoryAudioClip);
        PlayerPrefs.SetInt("currentLevel", currentLevel+1);
        finishScoretext.text = score.ToString();
        gameMenu.SetActive(false);
        finishMenu.SetActive(true);
        gameActive = false;
    }

    public void ChangeScore(int increment)
    {
        score += increment;
        scoreText.text = score.ToString();
    }

    public void UpdateMoneyTexts()
    {
        int money = PlayerPrefs.GetInt("money");
        startingMenuMoneyText.text = money.ToString();
        gameoverMenuMoneyText.text = money.ToString();
        finishGameMenuMoneyText.text = money.ToString();       
    }

    public void GiveMoneyToPlayer(int increment)
    {
        int money = PlayerPrefs.GetInt("money");
        money = Mathf.Max(0,money + increment); // 2 değerden en yüksek olanını döndürür
        PlayerPrefs.SetInt("money", money );
        UpdateMoneyTexts();
    }
}
