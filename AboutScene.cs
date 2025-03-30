public class AboutUIManager : MonoBehaviour
{
    // Method for the New User button
    public void OnNewUserButtonClicked()
    {
        SceneManager.LoadScene("RegisterScene"); // Load RegisterScene for new users
    }

    // Method for the Existing User button
    public void OnExistingUserButtonClicked()
    {
        SceneManager.LoadScene("LoginScene"); // Load LoginScene for existing users
    }
}
AboutScene.cs script directs user to either registration or login depending on their account status 
