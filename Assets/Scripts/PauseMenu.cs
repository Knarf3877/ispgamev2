using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject playerHUD;
    public GameObject playerStats;
    public GameObject winMenu;
    public GameObject winText;
    bool isPaused;
    static public bool isWon;

    // Start is called before the first frame update
    void Start()
    {
        AudioListener.pause = false;
        isWon = false;
        if (Time.timeScale != 1)
            Time.timeScale = 1;

        if (winMenu.activeSelf == true)
            winMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape) && isPaused == false && Respawn.isDead == false && !isWon)
        //{
        //    PauseGame();
        //    Debug.Log("game paused");
        //}
        //else if (Input.GetKeyDown(KeyCode.Escape) && isPaused && Respawn.isDead == false && !isWon)
        //{
        //    ResumeGame();
        //}
    }
    public void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        playerHUD.SetActive(false);
        playerStats.SetActive(false);
        isPaused = true;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        playerHUD.SetActive(true);
        playerStats.SetActive(true);
        isPaused = false;
        AudioListener.pause = false;
    }
    public void ExitGame()
    {
        isWon = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isPaused = false;
        AudioListener.pause = false;
    }

    public void RestartLevel()
    {
        isWon = false;
        Time.timeScale = 1;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        isPaused = false;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public void LevelComplete()
    {
        isWon = true;
        string difficulty;
        switch (PlayerPrefs.GetFloat("difficulty"))
        {
            case 1:
                difficulty = "Easy";
                break;
            case 2:
                difficulty = "Normal";
                break;
            case 3:
                difficulty = "Hard";
                break;
            default:
                difficulty = "Difficulty not found";
                break;
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0.5f;
        winMenu.SetActive(true);
        winText.GetComponent<TMP_Text>().text = "Level Complete!\non\n" + difficulty + " Mode";
        playerHUD.SetActive(false);
        playerStats.SetActive(true);
        isPaused = false;
        AudioListener.pause = true;
    }

}
