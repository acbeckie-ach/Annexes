using Unity.Netcode;
using Debug = UnityEngine.Debug;
using System.Security.Cryptography;

public class NetworkedItem : NetworkBehaviour
{
    private string _signature;   // Digital signature of the hashed item data
    private string _signedBy;    // Username of the player who signed the item
    private string _itemDataHash; // Hash of the item data for tamper detection

    private RSAParameters _privateKey;
    private RSAParameters _publicKey;

    [SerializeField] private TextMeshProUGUI signatureText; // Displays the signature on the object
    [SerializeField] private TextMeshProUGUI feedbackText;  // Displays verification feedback
    [SerializeField] private TextMeshProUGUI signedByText;  // Displays who signed the item

    private string filePath;

    private void Start()
    {
        // Generate public/private key pair
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            _privateKey = rsa.ExportParameters(true);
            _publicKey = rsa.ExportParameters(false);
        }

        filePath = Application.persistentDataPath + "/signatureData.json";
        LoadSignatureFromFile();
    }
    public void SignItem(string itemData, string signedBy)
    {
        if (IsServer)  // Ensure only the server signs items
        {
            // Signing process
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.ImportParameters(_privateKey);

                byte[] dataBytes = Encoding.UTF8.GetBytes(itemData);
                byte[] signedBytes = rsa.SignData(dataBytes,   CryptoConfig.MapNameToOID("SHA256"));

                _signature = Convert.ToBase64String(signedBytes);
                _signedBy = signedBy;

                UpdateSignatureUI();
                SaveSignatureToFile();
                Debug.Log($"Item signed by: {_signedBy} with signature: {_signature}");
            }
        }
    }

    //Verify the signature and detect unauthorized modifications
   public bool VerifyItem(string itemData)
    {
        bool isValid = false;
        if (string.IsNullOrEmpty(_signature))
         {
            feedbackText.text = "No signature found.";
           return false;
         }
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.ImportParameters(_publicKey);

            byte[] dataBytes = Encoding.UTF8.GetBytes(itemData);
            byte[] signatureBytes = Convert.FromBase64String(_signature);

            isValid = rsa.VerifyData(dataBytes, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
            feedbackText.text = isValid ? $"Signature is valid! Signed by {_signedBy}" : "Signature is invalid!";
        }

           return isValid;
        }

        // Display signature details on UI
        private void UpdateSignatureUI()
{
signatureText.text = $"Signature: {_signature}";
signedByText.text = $"Signed by: {_signedBy}";
}
// Remote procedure call to display signature for the receiver
[ClientRpc]
public void DisplaySignatureClientRpc(string signature, string signedBy)
{
_signature = signature;
_signedBy = signedBy;
UpdateSignatureUI();
}
// Save signature to a file
private void SaveSignatureToFile()
{
SignatureData data = new SignatureData
{
Signature = _signature,
SignedBy = _signedBy,
ItemDataHash = _itemDataHash
};

string json = JsonUtility.ToJson(data);
File.WriteAllText(filePath, json);
Debug.Log("Signature saved to file: " + filePath);
}

// Load signature from a file
private void LoadSignatureFromFile()
{
if (File.Exists(filePath))
{
string json = File.ReadAllText(filePath);
SignatureData data = JsonUtility.FromJson<SignatureData>(json);
_signature = data.Signature;
_signedBy = data.SignedBy;
_itemDataHash = data.ItemDataHash;
UpdateSignatureUI();
Debug.Log("Signature loaded from file: " + filePath);
}
else
{
Debug.Log("No signature file found.");
}
}

public string GetSignature() => _signature;
public string GetSignedBy() => _signedBy;
}

// Class to store signature data
[System.Serializable]
public class SignatureData
{
    public string Signature;
    public string SignedBy;
    public string ItemDataHash; // Store the hash of the item data to detect tampering
}
//The NetworkedItem.cs consists of key pair generation, digital signing, 
// signature verification, security feedback and persistent logging
