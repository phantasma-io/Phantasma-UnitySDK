using Newtonsoft.Json;
using UnityEngine;

// Unity MonoBehaviour used to demonstrate how to fetch all token balances for a given address
public class Example03_GetAddressBalances : MonoBehaviour
{
    // Entry point of example
    public void Run()
    {
        // Get reference to the scene-wide manager that stores API config and global variables
        var manager = FindObjectOfType<CoreExampleManager>();

        // Access the initialized Phantasma API instance
        var api = manager.phantasmaAPI;

        // Address to query - configured in the Unity inspector
        var address = manager.TestAddress;

        // Request account information including all token balances
        StartCoroutine(api.GetAccount(address, (accountResult) =>
            {
                // Convert full account result to readable JSON for logging
                var json = JsonConvert.SerializeObject(accountResult, Formatting.Indented);

                // Output all token balances including NFTs and fungible tokens
                Debug.Log($"[Balance] balances for {address}: {json}");
            },
            (errorCode, errorMessage) =>
            {
                Debug.LogError($"[Error][{errorCode}] {errorMessage}");
            }
        ));
    }
}
