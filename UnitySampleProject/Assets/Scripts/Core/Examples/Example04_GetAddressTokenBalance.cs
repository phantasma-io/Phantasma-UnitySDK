using UnityEngine;
using Newtonsoft.Json;
using PhantasmaPhoenix.Core;

// Unity MonoBehaviour used to demonstrate how to query specific token balance for a given address
public class Example04_GetAddressTokenBalance : MonoBehaviour
{
    // Entry point of example
    public void Run()
    {
        // Get reference to the scene-wide manager that stores API config and global variables
        var manager = FindObjectOfType<CoreExampleManager>();

        // Access the initialized Phantasma API instance
        var api = manager.phantasmaAPI;

        // Address to check balance for - configured in the Unity inspector
        var address = manager.TestAddress;

        // Token symbol to query (e.g. SOUL, KCAL, NFT symbol)
        var symbol = manager.TokenSymbol;

        StartCoroutine(api.GetToken(symbol,
            // Callback on success
            (tokenResult) =>
            {
                // Log full token info (decimals, supply, flags, etc.)
                Debug.Log($"[TokenInfo] {symbol}: {JsonConvert.SerializeObject(tokenResult, Formatting.Indented)}");

                StartCoroutine(api.GetTokenBalance(address, symbol, "main",
                    // Callback on success
                    (tokenBalanceResult) =>
                    {
                        // Check whether the token is fungible (e.g. SOUL, KCAL) or non-fungible (NFT)
                        if (tokenResult.IsFungible())
                        {
                            // UnitConversion.ToDecimal() converts raw token amount into human-readable decimal format
                            Debug.Log($"[Balance] Fungible {symbol} amount for {address}: {UnitConversion.ToDecimal(tokenBalanceResult.Amount, tokenBalanceResult.Decimals)}");
                        }
                        else
                        {
                            Debug.Log($"[Balance] NFT {symbol} count for {address}: {tokenBalanceResult.Amount}");
                        }
                    },
                    // Callback for RPC errors (invalid token, network error, etc.)
                    (errorCode, errorMessage) =>
                    {
                        Debug.LogError($"[Error][{errorCode}] {errorMessage}");
                    }
                ));
            },
            // Callback for RPC errors (invalid token, network error, etc.)
            (errorCode, errorMessage) =>
            {
                Debug.LogError($"[Error][{errorCode}] {errorMessage}");
            }
        ));
    }
}
