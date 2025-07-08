using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
using UnityEngine.UI;

public class LeaderboardType
{
    public const string one = "CgkInZfemK8XEAIQAQ";
    public const string two = "CgkInZfemK8XEAIQAg";
    public const string four = "CgkInZfemK8XEAIQAw";

    public static string GetType(int i)
    {
        switch (i)
        {
            case 1:
                {
                    return one;
                }
            case 2:
                {
                    return two;
                }
            case 4:
                {
                    return four;
                }
        }
        return one;
    }
}

public class GPLogin : MonoBehaviour
{
    public bool connected = false;
    public TextMeshProUGUI usernameText;
    public Image usernameImage;
    public Sprite redButton;
    public Sprite greenButton;
    // Start is called before the first frame update

    public void ManualSignIn()
    {
        if (connected) return;
        PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
    }

    public void AutoSignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            // Continue with Play Games Services

            string name = PlayGamesPlatform.Instance.GetUserDisplayName();
            string id = PlayGamesPlatform.Instance.GetUserId();
            string ImgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();
            connected = true;
            //DetailsText.text = "Success \n " + name;
            LoadScoreFromLeaderboard(1);
            LoadScoreFromLeaderboard(2);
            LoadScoreFromLeaderboard(4);
            //ShowLeadreboard(1);
            usernameText.text = "Logged in:\n" + name;
            usernameImage.sprite = greenButton;
        }
        else
        {
            usernameText.text = "Login with Play Games";
            //DetailsText.text = "Sign in Failed!!";
            connected = false;
            usernameImage.sprite = redButton;
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
        }
    }

    public void LoadScoreFromLeaderboard(int id)
    {
        PlayGamesPlatform.Instance.LoadScores(
            LeaderboardType.GetType(id),
            LeaderboardStart.PlayerCentered,
            1,
            LeaderboardCollection.Public,
            LeaderboardTimeSpan.AllTime,
            (data) =>
            {
                if (data.Valid && data.PlayerScore != null)
                {
                    long localHighScore = long.MaxValue;
                    if (PlayerPrefs.HasKey("HighScore"))
                    {
                        localHighScore = (long)PlayerPrefs.GetFloat("HighScore") * 1000;
                    }

                    Debug.Log("Player's best score on leaderboard: " + data.PlayerScore.value + "\nplayers best score on device storage: " + localHighScore);
                    Debug.Log("Rank: " + data.PlayerScore.rank);

                    //if the player gets a score offline, he can write it later to the leaderboard when he connects to the internet.
                    //and also if he uninstalls the game, his highscore will be updated again if he wishes to play again
                    if (localHighScore < data.PlayerScore.value)
                    {
                        WriteToLeaderboard(localHighScore, id);
                    }
                    else
                    {
                        PlayerPrefs.SetFloat("HighScore", (float)data.PlayerScore.value / 1000);
                    }


                }
                else
                {
                    Debug.LogWarning("Failed to retrieve player score or no score exists.");
                }
            });
    }

    public void WriteToLeaderboard(long time, int type)
    {
        if (!connected) return;
        PlayGamesPlatform.Instance.ReportScore(time, LeaderboardType.GetType(type), (bool success) =>
        {
            // Handle success or failure
            //DetailsText.text = success ? "Score uploaded to scoreboard :)" : "Error uploading score X_X";
        });
    }

    public void ShowLeaderboardAutoPick()
    {
        ShowLeadreboard(GameManager.instance.gameMode.id);
    }

    public void ShowLeadreboard(int type)
    {
        PlayGamesPlatform.Instance.ShowLeaderboardUI(LeaderboardType.GetType(type));
    }
}
