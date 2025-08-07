using UnityEngine;
using Newtonsoft.Json;
using PhantasmaPhoenix.Core;

public class Example04_GetAddressTokenBalance : MonoBehaviour
{
    public void Run()
    {
        var manager = FindObjectOfType<CoreExampleManager>();
        var api = manager.phantasmaAPI;

        var address = manager.TestAddress;
        var symbol = manager.TokenSymbol;

        StartCoroutine(api.GetToken(symbol, (tokenResult) =>
            {
                Debug.Log($"[TokenInfo] {symbol}: {JsonConvert.SerializeObject(tokenResult, Formatting.Indented)}");

                StartCoroutine(api.GetTokenBalance(address, symbol, "main", (tokenBalanceResult) =>
                    {
                        var json = JsonConvert.SerializeObject(tokenBalanceResult, Formatting.Indented);

                        if (tokenResult.IsFungible())
                        {
                            Debug.Log($"[Balance] Fungible {symbol} amount for {address}: {UnitConversion.ToDecimal(tokenBalanceResult.Amount, tokenBalanceResult.Decimals)}");
                        }
                        else
                        {
                            Debug.Log($"[Balance] NFT {symbol} count for {address}: {tokenBalanceResult.Amount}");
                        }
                    },
                    (errorCode, errorMessage) =>
                    {
                        Debug.LogError($"[Error][{errorCode}] {errorMessage}");
                    }
                ));
            },
            (errorCode, errorMessage) =>
            {
                Debug.LogError($"[Error][{errorCode}] {errorMessage}");
            }
        ));
    }
}
