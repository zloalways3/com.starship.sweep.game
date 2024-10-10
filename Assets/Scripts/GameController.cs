using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public GameObject initPanel;
    public GameObject menuPanel;
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject winDialog;
    public GameObject loseDialog;
    public Slider slider;
    public GameObject[] objects;
    public Text scoreText;
    public Text timerText;
    public Text winScore;
    public Text failScore;
    public Text gameDifficulty;
    public Image[] hearts;
    private int lives = 3;
    private float spawnTime;
    private float gameTime;
    private int targetScore;
    private int currentScore;
    private bool isGameActive = false;
    private bool isPaused = false;
    private List<GameObject> activeObjects = new List<GameObject>();
    public Button[] levelButtons;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button tryAgainWin;
    public Button tryAgainFail;
    public Button menuWin;
    public Button menuFail;
    private string selectedDifficulty = "low";
    private int selectedLevel = 1;
    public Sprite boomSprite;
    public Vector2 spawnAreaMin;
    public Vector2 spawnAreaMax;
    public Text difficultyWin;
    public Text difficultyLose;
    public Image EmptyStarWin1;
    public Image EmptyStarWin2;
    public Image EmptyStarWin3;
    public Sprite winStar;
    public Text finalScoreWin;
    public Text finalScoreLose;

    void Start()
    {
        isPaused = false;
        SelectDifficulty("low");
        easyButton.onClick.AddListener(() => SelectDifficulty("low"));
        mediumButton.onClick.AddListener(() => SelectDifficulty("mid"));
        hardButton.onClick.AddListener(() => SelectDifficulty("hard"));
        tryAgainFail.onClick.AddListener(() => StartGame());
        tryAgainWin.onClick.AddListener(() => StartGame());
        menuWin.onClick.AddListener(() => ShowMenu());
        menuFail.onClick.AddListener(() => ShowMenu());
        ChangeButtons();
    }

    private void Update()
    {
        if (initPanel.activeSelf)
        {
            slider.value += (Time.deltaTime / 2);

            if (slider.value > 0.9f)
            {
                startPanel.SetActive(true);
                initPanel.SetActive(false);
            }
        }
    }

    void ChangeButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1;
            bool isUnlocked = IsLevelUnlocked(levelIndex);
            levelButtons[i].interactable = isUnlocked;

            int currentLevel = levelIndex;
            levelButtons[i].onClick.AddListener(() => SelectLevel(currentLevel));

            UpdateButtonVisual(levelButtons[i], isUnlocked);
            LoadStars(levelButtons[i], currentLevel);
        }
    }

    void UpdateButtonVisual(Button levelButton, bool isUnlocked)
    {
        foreach (Transform child in levelButton.transform)
        {
            if (child.name == "Locker")
            {
                child.gameObject.SetActive(!isUnlocked);
            }
            else
            {
                child.gameObject.SetActive(isUnlocked);
            }
        }
    }

    void LoadStars(Button levelButton, int levelIndex)
    {
        int stars = PlayerPrefs.GetInt("starsLevel" + levelIndex, 0);
        Image[] starsImages = levelButton.GetComponentsInChildren<Image>();

        foreach (Image img in starsImages)
        {
            if (img.gameObject.name.Contains("Star"))
            {
                int starIndex = int.Parse(img.gameObject.name.Replace("Star", "")) - 1;
                img.enabled = (starIndex < stars);
            }
        }
    }

    bool IsLevelUnlocked(int level)
    {
        int highestUnlockedLevel = PlayerPrefs.GetInt("highestUnlockedLevel", 1);
        return level <= highestUnlockedLevel;
    }

    void UnlockNextLevel()
    {
        int highestUnlockedLevel = PlayerPrefs.GetInt("highestUnlockedLevel", 1);
        if (selectedLevel >= highestUnlockedLevel)
        {
            PlayerPrefs.SetInt("highestUnlockedLevel", selectedLevel + 1);
        }
    }

    void ShowMenu()
    {
        isGameActive = false;
        CancelInvoke("SpawnMultipleObjects");
        StopAllCoroutines();
        ClearActiveObjects();
        currentScore = 0;
        lives = hearts.Length;
        UpdateHeartsDisplay();
        scoreText.text = "Score: 0/" + targetScore;
        timerText.text = "Time: 00:00";
        Time.timeScale = 1;
        isPaused = false;
        menuPanel.SetActive(true);
        gamePanel.SetActive(false);
        winDialog.SetActive(false);
        loseDialog.SetActive(false);
    }

    void ClearActiveObjects()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Planet"))
        {
            Destroy(obj);
        }
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Bomb"))
        {
            Destroy(obj);
        }
    }

    public void ExitGame()
    {
        ShowMenu();
    }

    void StartGame()
    {
        ClearActiveObjects();
        CancelInvoke();
        StopAllCoroutines();
        winDialog.SetActive(false);
        loseDialog.SetActive(false);
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        lives = hearts.Length;
        UpdateHeartsDisplay();
        isPaused = false;
        StartGameProcess();
    }

    void SelectLevel(int level)
    {
        selectedLevel = level;
        StartGame();
    }

    void SelectDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;

        Color fullColor = new Color(1f, 1f, 1f, 1f);
        Color fadedColor = new Color(1f, 1f, 1f, 0.5f);

        easyButton.image.color = difficulty == "low" ? fullColor : fadedColor;
        mediumButton.image.color = difficulty == "mid" ? fullColor : fadedColor;
        hardButton.image.color = difficulty == "hard" ? fullColor : fadedColor;
        gameDifficulty.text = "DIFFICULTY: " + difficulty;
    }

    void StartGameProcess()
    {
        SetLevelAndDifficulty();
        currentScore = 0;
        scoreText.text = "Score: " + currentScore + "/" + targetScore;
        isGameActive = true;
        StartCoroutine(GameTimer());
        InvokeRepeating("SpawnMultipleObjects", 0f, spawnTime);
    }

    void SpawnMultipleObjects()
    {
        SpawnObject(5);
    }

    void SetLevelAndDifficulty()
    {
        targetScore = 200 + (selectedLevel - 1) * 25;
        gameTime = 120 - (selectedLevel - 1) * 3;

        switch (selectedDifficulty)
        {
            case "low":
                spawnTime = 1f;
                break;
            case "mid":
                spawnTime = 0.8f;
                break;
            case "hard":
                spawnTime = 0.5f;
                break;
        }
    }

    IEnumerator GameTimer()
    {
        while (gameTime > 0)
        {
            timerText.text = "Time: " + Mathf.FloorToInt(gameTime / 60).ToString("00") + ":" + Mathf.FloorToInt(gameTime % 60).ToString("00");
            yield return new WaitForSeconds(1f);
            gameTime--;

            if (gameTime <= 0)
            {
                EndGame(false);
            }
        }
    }

    void SpawnObject(int numberOfObjects)
    {
        if (!isGameActive) return;

        for (int i = 0; i < numberOfObjects; i++)
        {
            int randomIndex = Random.Range(0, objects.Length);
            GameObject spawnedObject = Instantiate(objects[randomIndex], RandomPosition(), Quaternion.identity);
            Destroy(spawnedObject, spawnTime);
        }
    }

    Vector2 RandomPosition()
    {
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        return new Vector2(randomX, randomY);
    }

    public void ObjectTapped(GameObject obj)
    {
        if (!isGameActive) return;

        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        Transform pointsTransform = obj.transform.Find("points");
        if (pointsTransform != null)
        {
            pointsTransform.gameObject.SetActive(true);
        }

        spriteRenderer.sprite = boomSprite;
        StartCoroutine(DestroyAfterDelay(obj, 0.5f));

        if (obj.tag == "Planet")
        {
            currentScore += 20;
        }
        else if (obj.tag == "Bomb")
        {
            currentScore -= 50;
            LoseLife();

            if (currentScore < 0)
            {
                currentScore = 0;
            }
        }

        scoreText.text = "Score: " + currentScore + "/" + targetScore;
        if (currentScore >= targetScore)
        {
            EndGame(true);
        }
    }

    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    void LoseLife()
    {
        lives--;
        UpdateHeartsDisplay();

        if (lives <= 0)
        {
            EndGame(false);
        }
    }

    void UpdateHeartsDisplay()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < lives;
        }
    }

    void SaveStars()
    {
        int currentStars = PlayerPrefs.GetInt("starsLevel" + selectedLevel, 0);
        int starsEarned = selectedDifficulty == "low" ? 1 : selectedDifficulty == "mid" ? 2 : 3;

        if (starsEarned > currentStars)
        {
            PlayerPrefs.SetInt("starsLevel" + selectedLevel, starsEarned);
        }
    }

    void EndGame(bool win)
    {
        SetActiveObjectsState(false);
        isGameActive = false;
        CancelInvoke("SpawnObject");

        string difficultyText = "DIFFICULTY: " + (selectedDifficulty == "low" ? "LOW" : selectedDifficulty == "mid" ? "MID" : "HARD");
        difficultyWin.text = difficultyText;
        difficultyLose.text = difficultyText;

        if (currentScore < 0)
            currentScore = 0;
        finalScoreWin.text = "FINAL SCORE: " + currentScore;
        finalScoreLose.text = "FINAL SCORE: " + currentScore;

        if (win)
        {
            FindObjectOfType<AudioManager>().PlaySoundByIndex(4); 
            UnlockNextLevel();
            SaveStars();

            if (selectedDifficulty == "low")
            {
                EmptyStarWin1.sprite = winStar;
            }
            else if (selectedDifficulty == "mid")
            {
                EmptyStarWin1.sprite = winStar;
                EmptyStarWin2.sprite = winStar;
            }
            else if (selectedDifficulty == "hard")
            {
                EmptyStarWin1.sprite = winStar;
                EmptyStarWin2.sprite = winStar;
                EmptyStarWin3.sprite = winStar;
            }

            winDialog.SetActive(true);
        }
        else
        {
            FindObjectOfType<AudioManager>().PlaySoundByIndex(2);  
            loseDialog.SetActive(true);
        }

        ChangeButtons();
    }

    public void PauseGame()
    {
        if (isPaused) return;

        Time.timeScale = 0;
        isPaused = true;

        activeObjects.Clear();
        activeObjects.AddRange(GameObject.FindGameObjectsWithTag("Planet"));
        activeObjects.AddRange(GameObject.FindGameObjectsWithTag("Bomb"));

        SetActiveObjectsState(false);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        Time.timeScale = 1;
        isPaused = false;

        SetActiveObjectsState(true);
    }

    void SetActiveObjectsState(bool state)
    {
        foreach (var obj in activeObjects)
        {
            if (obj != null) 
            {
                obj.SetActive(state);
            }
        }
    }
}
