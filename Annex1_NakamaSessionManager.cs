using Nakama;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class NakamaSessionManager : MonoBehaviour
{
    public static NakamaSessionManager Instance { get; private set; }
    public IClient Client { get; set; }
    public ISession Session { get; set; }
    private string deviceId;

    private const string Scheme = "http";
    private const string Host = "localhost";
    private const int Port = 7350;
    private const string ServerKey = "defaultkey";

    private bool isRefreshing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Initialize the Nakama client
        Client = new Client(Scheme, Host, Port, ServerKey, UnityWebRequestAdapter.Instance);
        deviceId = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("Nakama client initialized in NakamaSessionManager.");
    }

    // Connect with device authentication, refresh token if expired.
    public async Task<bool> Connect()
    {
        try
        {
            if (Session == null || Session.IsExpired)
            {
                if (Session != null && Session.RefreshToken != null)
                {
                    await RefreshSession();
                }
                else
                {
                    Session = await Client.AuthenticateDeviceAsync(deviceId);
                    PlayerPrefs.SetString("nakama_refresh_token", Session.RefreshToken);
                    PlayerPrefs.Save();
                    Debug.Log("Session created with Nakama server.");
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection error: {e.Message}");
            return false;
        }
    }

        // Automatically refresh the session if needed.
        private async Task RefreshSession()
    {
        if (isRefreshing) return; // Prevent multiple refreshes at the same time

        try
        {
            isRefreshing = true;
            // Attempt to refresh the session using the refresh token
            string refreshToken = PlayerPrefs.GetString("nakama_refresh_token", null);
            if (string.IsNullOrEmpty(refreshToken))
            {
                Debug.LogWarning("No refresh token found. Re-authentication required.");
                Session = null; // Force re-authentication
                return;
            }

            Session = await Client.SessionRefreshAsync(Session);
            PlayerPrefs.SetString("nakama_refresh_token", Session.RefreshToken);
            
            Debug.Log("Session successfully refreshed.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to refresh session: {e.Message}");
            Session = null; // Force re-authentication if refreshing fails
        }
        finally
        {
            isRefreshing = false;
        }
    }
    // Check if the user is authenticated and ensure session is refreshed.
       public bool IsUserAuthenticated()
    {
        if (Session == null || Session.IsExpired)
        {
            Debug.LogWarning("User session expired or not authenticated.");
            return false;
        }

        Debug.Log("User is authenticated.");
        return true;
    }
    // Refresh session automatically at regular intervals.
        private async void Update()
    {
        // Check every frame for session expiration (can be optimized with a coroutine)
        if (Session != null && Session.HasExpired(DateTime.UtcNow.AddMinutes(1)))
        {
            Debug.Log("Session about to expire, refreshing...");
            await RefreshSession();
        }
    }
}
//The script Annex 1.1 for session management. 
// A singleton pattern to ensure that only one 
// instance of the session manager exists at runtime, 
// allowing persistent session access across different scenes in Unity. 
