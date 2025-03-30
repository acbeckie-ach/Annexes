using Newtonsoft.Json.Linq;
public class GeolocationManager : MonoBehaviour
{
    private string apiKey = "9549a1362fd7e7";
    private string apiUrl = "https://ipinfo.io";
    public PrivacyDashboardManager privacyDashboard;

    void Start()
    {
        StartCoroutine(GetUserLocation());
    }

    IEnumerator GetUserLocation()
    {
        string url = $"{apiUrl}?token={apiKey}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Full API Response: " + jsonResponse);

                string countryCode = ExtractCountryCode(jsonResponse);
                string compliance = GetComplianceByCountry(countryCode);

                privacyDashboard.UpdatePrivacySettings(countryCode, compliance);
            }
            else
            {
                Debug.LogError("Failed to retrieve location: " + request.error);
            }
        }
    }

    private string ExtractCountryCode(string jsonResponse)
    {
        try
        {
            JObject json = JObject.Parse(jsonResponse);
            if (json["country"] != null)
            {
                string countryCode = json["country"].ToString().Trim();
                Debug.Log("Extracted Country Code: " + countryCode); // Confirm the extracted country
                return countryCode;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error extracting country code: " + ex.Message);
        }
        return "Unknown";
    }

    private string GetComplianceByCountry(string countryCode)
    {
        Debug.Log("Checking compliance for country: " + countryCode); // debug log

        switch (countryCode.ToUpper()) // Ensures case consistency
        {
            case "FR":
            case "DE":
            case "RO": 
            case "IT":
            case "ES":
                return "GDPR";
            case "US":
                return "CCPA";
            case "CN":
                return "PIPL";
            case "BR":
                return "LGPD";
            case "GH":
                return "DPA_GH";

            default:
                return "Standard Privacy Policy";
        }
    }

}
//The script enables automatic privacy compliance enforcement by 
// dynamically detecting the user's geographical location based on their IP address
