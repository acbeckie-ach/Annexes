using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nakama;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Register Manager : Mon Behaviour
{
    private IClient Client => NakamaSessionManager.Instance. Client;
    private ISession Session => NakamaSessionManager.Instance. Session;
    public string apiUrl = "http://localhost:8081/send_email"; 
    public async Task RegisterUser(string email, string password, string username)
    {
        try
        {
            await Client.AuthenticateEmailAsync(email, password, username, true);
            Debug.Log("User registered successfully.");
            LoadLoginScene();
        }
        catch (ApiResponseException e)
        {
            Debug.LogError($"Error registering user: {e.Message} (Code: {e.StatusCode})");
        }
        catch (Exception e)
        {
            Debug.LogError($"Unexpected error: {e.Message}");
        }
    }
    // User login
    public async Task LoginUser(string email, string password, TMP_Text errorMessageText)
    {
        try
        {
            var newSession = await Client.AuthenticateEmailAsync(email, password);
            NakamaSessionManager.Instance.Session = newSession;
            errorMessageText.text = "";
            LoadCollaborativeScene();
        }
        catch (ApiResponseException e)
        {
            errorMessageText.text = "Login failed. Please check your email and password.";
            Debug.LogError($"Login failed: {e.Message}");
        }
        catch (Exception e)
        {
            errorMessageText.text = "An error occurred. Please try again later.";
            Debug.LogError($"Unexpected error: {e.Message}");
        }
    }
    public async Task<string> RequestPasswordReset(string email, TMP_Text messageText)
    {
        Debug.Log($"[RegisterManager] Requesting password reset for email: {email}");

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("[RegisterManager] ERROR: Email is null or empty.");
            return null;
        }
        try
        {
            string resetToken = GenerateSecureToken();
            Debug.Log($"[RegisterManager] Generated token: {resetToken}");
            await StoreResetToken(email, resetToken);
            Debug.Log($"[RegisterManager] Stored token for {email}");
            SendEmail(email, "Password Reset Request", $"Use this token to reset your password: {resetToken}", resetToken);
            Debug.Log($"[RegisterManager] Sending email via API...");

            messageText.text = "Password reset email sent!";
            return resetToken;
        }
        catch (Exception e)
        {
            messageText.text = "Error occurred. Please try again.";
            Debug.LogError($"[RegisterManager] Error requesting password reset: {e.Message}");
            return null;
        }
    }
    public async Task ResetPassword(string email, string token, string newPassword, TMP_Text messageText)
    {
        Debug.Log($"[RegisterManager] Resetting password for {email} using token {token}");
        // Check if the user session is valid before reading from Nakama
        if (Session == null || Session.IsExpired)
        {
            Debug.LogError("[RegisterManager] ERROR: No active session. Redirecting to login.");
            messageText.text = "Session expired. Please log in again.";
            SceneManager.LoadScene("LoginScene");
            return;
        }
        try
        {
            Debug.Log($"[RegisterManager] Fetching stored token for {email} from Nakama...");
            var result = await Client.ReadStorageObjectsAsync(Session, new[] { new StorageObjectId { Collection = "reset_tokens", Key = email } });
            if (!result.Objects.Any())
            {
                messageText.text = "Invalid or expired token.";
                Debug.LogError($"[RegisterManager] No reset token found for {email}");
                return;
            }
            var storedObject = result.Objects.First();
            var storedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(storedObject.Value);
            Debug.Log($"[RegisterManager] Retrieved stored token for {email}: {storedData["token"]}");

            if (!storedData.ContainsKey("token") || storedData["token"] != token)
            {
                messageText.text = "Invalid or expired token.";
                Debug.LogError($"[RegisterManager] Provided token does not match stored token for {email}");
                return;
            }
            // Update the user's password
            await Client.UpdateAccountAsync(Session, null, email, newPassword, null);
            messageText.text = "Password reset successful!";
            Debug.Log($"[RegisterManager] Password reset successfully for {email}");

            // Remove the token after a successful password reset
            await Client.DeleteStorageObjectsAsync(Session, new[] { new StorageObjectId { Collection = "reset_tokens", Key = email } });
        }
        catch (Exception e)
        {
            messageText.text = "Error resetting password. Please try again.";
            Debug.LogError($"[RegisterManager] Error resetting password for {email}: {e.Message}");
        }
    }
//generate token
    private string GenerateSecureToken()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private async Task StoreResetToken(string email, string resetToken)
    {
        var resetTokenData = new Dictionary<string, string> { { "token", resetToken } };
        var storageWrite = new WriteStorageObject
        {
            Collection = "reset_tokens",
            Key = email,
            Value = JsonConvert.SerializeObject(resetTokenData),
            PermissionRead = 2,
            PermissionWrite = 1
        };

        await Client.WriteStorageObjectsAsync(Session, new[] { storageWrite });
    }

    public void SendEmail(string recipientEmail, string subject, string body, string token)
    {
        StartCoroutine(SendEmailCoroutine(recipientEmail, subject, body, token));
    }

    private IEnumerator SendEmailCoroutine(string recipientEmail, string subject, string body, string token)
    {
        EmailRequest emailRequest = new EmailRequest
        {
            recipientEmail = recipientEmail,
            subject = subject,
            body = body,
            token = token
        };
        string jsonData = JsonUtility.ToJson(emailRequest);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[RegisterManager] Email sent successfully: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"[RegisterManager] Error sending email: {request.error}");
        }
    }
    [System.Serializable]
    public class EmailRequest
    {
        public string recipientEmail;
        public string subject;
        public string body;
        public string token;
    }
    private void LoadLoginScene() => SceneManager.LoadScene("LoginScene");
    private void LoadCollaborativeScene() => SceneManager.LoadScene("Collaborativetraining");
}
//RegisterManager.cs script connects the front-end with Nakama’s authentication. 
