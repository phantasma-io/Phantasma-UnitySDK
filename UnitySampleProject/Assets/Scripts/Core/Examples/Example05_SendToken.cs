using UnityEngine;
using System;
using PhantasmaPhoenix.VM;
using PhantasmaPhoenix.Cryptography;
using PhantasmaPhoenix.Core;

public class Example05_SendToken : MonoBehaviour
{
    public void Run()
    {
        var manager = FindObjectOfType<CoreExampleManager>();
        var keys = manager.keys;
        var senderAddress = keys.Address;
        var api = manager.phantasmaAPI;

        var nexus = manager.Nexus;
        var destinationAddress = manager.TestAddress;
        var symbol = manager.TokenSymbol;
        var amount = manager.TokenAmount;
        var feePrice = 100000; // TODO: Adapt to new fee model.
        var feeLimit = 21000; // TODO: Adapt to new fee model.

        byte[] script;

        StartCoroutine(api.GetToken(symbol, (tokenResult) =>
            {
                try
                {
                    var sb = new ScriptBuilder();
                    sb.AllowGas(senderAddress, Address.Null, feePrice, feeLimit);

                    sb.TransferTokens(symbol, senderAddress, destinationAddress, UnitConversion.ToBigInteger((decimal)amount, tokenResult.Decimals));

                    sb.SpendGas(senderAddress);
                    script = sb.EndScript();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Error] Could not built transaction scripts {e}");
                    return;
                }

                StartCoroutine(api.SignAndSendTransactionWithPayload(keys, nexus, script, "main", "example5-tx-payload", (hashText, encodedTx, txHash) =>
                    {
                        if ( !string.IsNullOrEmpty(hashText) )
                        {
                            Debug.Log($"Transaction was sent, hash: {hashText}. Check transaction status using GetTransaction() call");
                            return;
                        }
                        else
                        {
                            Debug.LogError("[Error] Failed to send transaction");
                        }
                    }, (errorCode, errorMessage) =>
                    {
                        Debug.LogError($"[Error][{errorCode}] Failed to send transaction: {errorMessage}");
                    }));
            },
            (errorCode, errorMessage) =>
            {
                Debug.LogError($"[Error][{errorCode}] {errorMessage}");
            }
        ));
    }
}
