public class RegistrationUIManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField userNameInputField;
    public TMP_Text errorMessageText;
    public Button registerButton;

    private RegisterManager registerManager;

    private void Start()
    {
        //the registermanager is a singleton and is present
        registerManager = FindObjectOfType<RegisterManager>();

        //Attach listener to login button
        registerButton.onClick.AddListener(async () => await OnregisterButtonClicked());
    }
    private async Task OnregisterButtonClicked()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;
        string username = userNameInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
        {
            errorMessageText.text = "All fields must be filled out.";

            return;
        }

        //clear any previous error
        errorMessageText.text = "";

        //call RegisterManager to register the user
        await registerManager.RegisterUser(email, password, username);
    }
}
//This script provides the logic necessary for capturing user input and invoking the backend 
// registration functionality securely and responsively
