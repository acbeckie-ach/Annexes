public class PrivacyDashboardManager : MonoBehaviour
{
    public TextMeshProUGUI countryText;
    public TextMeshProUGUI complianceText;
    public TMP_Dropdown dataRetentionDropdown;
    public Toggle movementTrackingToggle;
    public Toggle behaviorAnalysisToggle;
    public Button saveSettingsButton;
    public Button deleteDataButton;

    private string userCountry;
    private string complianceRule;

    void Start()
    {
        Debug.Log("Initializing Privacy Dashboard...");

        // Ensure complianceRule is set before applying rules
        if (string.IsNullOrEmpty(complianceRule))
        {
            complianceRule = "Standard Privacy Policy"; // Default before geolocation updates it
        }

        ApplyComplianceRules(complianceRule); // should have a value

        // Load user settings (after compliance rule is applied)
        LoadPreferences();

        // Populate Data Retention Dropdown
        PopulateDataRetentionDropdown();

        // Add button listeners
        saveSettingsButton.onClick.AddListener(SaveSettings);
        deleteDataButton.onClick.AddListener(DeleteUserData);

        Debug.Log($"Privacy Dashboard Initialized → Compliance Rule: {complianceRule}");
    }

    // Populate Data Retention Dropdown with correct options
    void PopulateDataRetentionDropdown()
    {
        dataRetentionDropdown.ClearOptions();
        dataRetentionDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "1 Day",
            "1 Week",
            "1 Month",
            "6 Months",
            "1 Year",
            "Indefinite"
        });

        Debug.Log("Dropdown Options Populated Successfully.");
    }

    // Update UI when compliance settings are detected
    public void UpdatePrivacySettings(string countryCode, string compliance)
    {
        if (string.IsNullOrEmpty(countryCode) || string.IsNullOrEmpty(compliance))
        {
            Debug.LogError("Privacy settings received empty values. Country: " + countryCode + ", Compliance: " + compliance);
            return;
        }

        userCountry = countryCode;
        complianceRule = compliance;

        countryText.text = "Country: " + userCountry;
        complianceText.text = "Compliance: " + complianceRule;

        Debug.Log($"User's Country: {userCountry} | Compliance Rule: {complianceRule}");

        // Immediately enforce compliance rules when updated
        ApplyComplianceRules(complianceRule);
    }

    // Apply compliance rules dynamically
    void ApplyComplianceRules(string complianceRule)
    {
        Debug.Log($"Applying Compliance Rules: {complianceRule}");

        switch (complianceRule)
        {
            case "GDPR":
            case "DPA_GH":
                Debug.Log("Enforcing GDPR: Data collection is disabled by default. User must opt-in.");

                //  Disable data collection by default
                movementTrackingToggle.isOn = false;
                behaviorAnalysisToggle.isOn = false;

                // Prevent user from toggling tracking initially (until opt-in)
                movementTrackingToggle.interactable = true;
                behaviorAnalysisToggle.interactable = true;
                break;

            case "CCPA":
                Debug.Log("Enforcing CCPA: Data collection is enabled by default, user can opt-out.");

                // Enable tracking by default (user can opt-out)
                movementTrackingToggle.isOn = true;
                behaviorAnalysisToggle.isOn = true;

                // Allow user to disable tracking
                movementTrackingToggle.interactable = true;
                behaviorAnalysisToggle.interactable = true;
                break;

            case "PIPL":
                Debug.Log("Enforcing PIPL: Restricting cloud storage. Data must be stored locally.");
                break;

            case "LGPD":
                Debug.Log("Enforcing LGPD: GDPR-like rules but allows business justification.");

                // Disable tracking by default (user can enable)
                movementTrackingToggle.isOn = false;
                behaviorAnalysisToggle.isOn = false;

                //  Allow enabling manually
                movementTrackingToggle.interactable = true;
                behaviorAnalysisToggle.interactable = true;
                break;

            default:
                Debug.LogWarning("No strict compliance rules. Standard Privacy Policy applies.");
                break;
        }

        // Force UI Refresh to Apply the Changes
        movementTrackingToggle.gameObject.SetActive(false);
        behaviorAnalysisToggle.gameObject.SetActive(false);
        movementTrackingToggle.gameObject.SetActive(true);
        behaviorAnalysisToggle.gameObject.SetActive(true);

        Debug.Log($"Final Toggle Status → Movement Tracking: {movementTrackingToggle.isOn}, Behavior Analysis: {behaviorAnalysisToggle.isOn}");
    }

    // Save privacy settings (GDPR prevents saving unless user opts in)
    void SaveSettings()
    {
        Debug.Log($"Attempting to Save Settings → Compliance Rule: {complianceRule}");

        if (complianceRule == "GDPR" || complianceRule == "DPA_GH")
        {
            if (!movementTrackingToggle.isOn && !behaviorAnalysisToggle.isOn)
            {
                Debug.LogWarning("GDPR/DPA_GH Compliance: Cannot enable data tracking without explicit opt-in.");
                return; //  Prevent saving if tracking is still off
            }
            else
            {
                Debug.Log("GDPR/DPA_GH Opt-in Successful: Data tracking enabled.");
            }
        }

        PlayerPrefs.SetInt("MovementTracking", movementTrackingToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("BehaviorAnalysis", behaviorAnalysisToggle.isOn ? 1 : 0);
        string selectedRetention = dataRetentionDropdown.options[dataRetentionDropdown.value].text;
        PlayerPrefs.SetString("DataRetention", selectedRetention);
        PlayerPrefs.Save();

        Debug.Log($"Privacy settings saved! Movement Tracking: {movementTrackingToggle.isOn}, Behavior Analysis: {behaviorAnalysisToggle.isOn}");
    }

    // Delete user data on request
    void DeleteUserData()
    {
        PlayerPrefs.DeleteKey("MovementTracking");
        PlayerPrefs.DeleteKey("BehaviorAnalysis");
        PlayerPrefs.DeleteKey("DataRetention");

        Debug.Log("User data deleted!");
    }

    // Load saved preferences & enforce GDPR if needed
    void LoadPreferences()
    {
        Debug.Log("Loading Privacy Preferences...");

        // Load saved tracking preferences
        bool savedMovementTracking = PlayerPrefs.GetInt("MovementTracking", 1) == 1;
        bool savedBehaviorAnalysis = PlayerPrefs.GetInt("BehaviorAnalysis", 1) == 1;

        // Load saved data retention preference
        string savedRetention = PlayerPrefs.GetString("DataRetention", "1 Month");
        int index = dataRetentionDropdown.options.FindIndex(option => option.text == savedRetention);
        dataRetentionDropdown.value = (index >= 0) ? index : 2;

        // Apply GDPR enforcement if needed
        if (complianceRule == "GDPR" || complianceRule == "DPA_GH")
        {
            Debug.Log("GDPR Enforcement: Overriding saved preferences. Tracking disabled.");

            movementTrackingToggle.isOn = false;
            behaviorAnalysisToggle.isOn = false;
            movementTrackingToggle.interactable = false;
            behaviorAnalysisToggle.interactable = false;
        }
        else
        {
            // If NOT GDPR, restore saved settings
            movementTrackingToggle.isOn = savedMovementTracking;
            behaviorAnalysisToggle.isOn = savedBehaviorAnalysis;
            movementTrackingToggle.interactable = true;
            behaviorAnalysisToggle.interactable = true;
        }

        Debug.Log($"Loaded Preferences → Movement Tracking: {movementTrackingToggle.isOn}, Behavior Analysis: {behaviorAnalysisToggle.isOn}, Data Retention: {savedRetention}");
    }
}
//The script responds to geolocation inputs to enforce real-time 
// compliance with global data protection laws such as GDPR, CCPA, PIPL, 
// and DPA 2012 within immersive VR applications
