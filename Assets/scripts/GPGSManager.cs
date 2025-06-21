using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.UI;

public class GPGSManager : MonoBehaviour
{
    public static GPGSManager Instance { get; private set; }
    private bool isAuthenticated = false;
    public Image image;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializeGPGS();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGPGS()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        // PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(OnSignInResult);
    }

    private void OnSignInResult(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("Google Play Games sign-in successful.");
            isAuthenticated = true;
            image.color = Color.green;
        }
        else
        {
            Debug.LogError("Google Play Games sign-in failed: " + status);
            isAuthenticated = false;
            image.color = Color.red;
        }
    }

    public bool IsAuthenticated()
    {
        return isAuthenticated;
    }
}
