using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using XRMultiplayer;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private InputActionReference signActionReference;   // Input for signing
    [SerializeField] private InputActionReference verifyActionReference; // Input for verifying

    private NetworkedItem _networkedItem;
    private XRINetworkPlayer _player;

    private void Start()
    {
        _networkedItem = FindObjectOfType<NetworkedItem>();
        _player = GetComponent<XRINetworkPlayer>(); // Access the player's XRINetworkPlayer component

        // Bind input actions
        signActionReference.action.performed += OnSignItem;
        verifyActionReference.action.performed += OnVerifyItem;
    }

    // Method triggered when the player presses the A button to sign the item
    private void OnSignItem(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            string playerName = _player.playerName; // Get the player's name from the XRINetworkPlayer
            _networkedItem.SignItem("ItemData", playerName);  // Sign the item with the player's name
            Debug.Log(playerName + " signed the item.");

            // Broadcast the signed item to all players using an RPC
            BroadcastSignatureToAllClients(_networkedItem.GetSignature(), _networkedItem.GetSignedBy());
        }
    }

    // Broadcasts the signed item to all players in the session
    private void BroadcastSignatureToAllClients(string signature, string signedBy)
    {
        // Send the signed item to all players using an RPC
        _networkedItem.DisplaySignatureClientRpc(signature, signedBy); // Correct RPC call
        Debug.Log("Broadcasting signed item to all players.");
    }

    // Method triggered when the player presses the B button to verify the item
    private void OnVerifyItem(InputAction.CallbackContext context)
    {
        // Every player can verify the item, not just the owner
        bool isValid = _networkedItem.VerifyItem("ItemData");
        Debug.Log(isValid ? "Item verified successfully." : "Item verification failed.");
    }

    private new void OnDestroy()
    {
        // Unbind actions to avoid memory leaks
        signActionReference.action.performed -= OnSignItem;
        verifyActionReference.action.performed -= OnVerifyItem;
    }
}
//The script consist of input action binding, item signing (Button A), signature broadcast, 
// item verification (Button B)
