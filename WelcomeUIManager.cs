public class WelcomeUIManager: MonoBehaviour
{
    public TMP_Text messageText;
    public Button connectButton;

    private void Start()
    {
        connectButton.onClick.AddListener(async () => await OnConnectButtonClicked());
    }

    private async Task OnConnectButtonClicked()
    {
        messageText.text = "connecting";
        bool connected = await NakamaSessionManager.Instance.Connect();

        if (connected)
        {
            messageText.text = "connected";
            Debug.Log("Connected successfully.");
            SceneManager.LoadScene("AboutScene");
            
        }
        else
        {
            Debug.LogError("Failed to connect.");
        }
    }
}
//WelcomeUIManager.cs script initiates the authentication session at the entry point of the application. 
// It depends on the NakamaSessionManager singleton being initialized properly in the scene
