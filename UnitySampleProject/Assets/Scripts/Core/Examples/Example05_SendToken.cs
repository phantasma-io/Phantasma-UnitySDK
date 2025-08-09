using UnityEngine;
using System;
using PhantasmaPhoenix.VM;
using PhantasmaPhoenix.Cryptography;
using PhantasmaPhoenix.Core;

// Unity MonoBehaviour used to demonstrate how to transfer fungible tokens
public class Example05_SendToken : MonoBehaviour
{
    // Entry point of example
    public void Run()
    {
        // Get reference to the scene-wide manager that stores API config and global variables
        var manager = FindObjectOfType<CoreExampleManager>();

        var keys = manager.keys;

        // Abort if keys are not initialized - transaction cannot be signed
        if (keys == null)
        {
            throw new Exception("Private key is not set");
        }

        // Address derived from the loaded private key - used as transaction sender
        var senderAddress = keys.Address;

        // Access the initialized Phantasma API instance
        var api = manager.phantasmaAPI;

        // Target chain Nexus name (e.g. "testnet" or "mainnet") - configured in the Unity inspector
        var nexus = manager.Nexus;

        // Recipient address for the token transfer - configured in the Unity inspector
        var destinationAddress = manager.TestAddress;

        // Token symbol to transfer (e.g. SOUL, KCAL)
        var symbol = manager.TokenSymbol;

        // Amount to send - configured in the Unity inspector
        var amount = manager.TokenAmount;

        // Not used right now, use as is
        var feePrice = 100000; // TODO: Adapt to new fee model.
        var feeLimit = 21000; // TODO: Adapt to new fee model.

        byte[] script;

        StartCoroutine(api.GetToken(symbol, (tokenResult) =>
            {
                try
                {
                    // ScriptBuilder is used to create a serialized transaction script
                    var sb = new ScriptBuilder();
                    // Instruction to allow gas fees for the transaction - required by all transaction scripts
                    sb.AllowGas(senderAddress, Address.Null, feePrice, feeLimit);

                    // Add instruction to transfer tokens from sender to destination, converting human-readable amount to chain format
                    sb.TransferTokens(symbol, senderAddress, destinationAddress, UnitConversion.ToBigInteger((decimal)amount, tokenResult.Decimals));

                    // Spend gas necessary for transaction execution
                    sb.SpendGas(senderAddress);

                    // Finalize and get raw bytecode for the transaction script
                    script = sb.EndScript();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Error] Could not built transaction scripts {e}");
                    return;
                }

                StartCoroutine(api.SignAndSendTransaction(keys, nexus, script, "main", "example5-tx-payload",
                    // Callback on success
                    (txHash, encodedTx) =>
                    {
                        if (!string.IsNullOrEmpty(txHash))
                        {
                            Debug.Log($"Transaction was sent, hash: {txHash}. Check transaction status using GetTransaction() call");

                            // Start polling to track transaction execution status on-chain
                            StartCoroutine(Example06_CheckTransactionState.CheckTxStateLoop(txHash, null));

                            return;
                        }
                        else
                        {
                            Debug.LogError("[Error] Failed to send transaction");
                        }
                    },
                    // Callback for RPC errors (invalid token, network error, etc.)
                    (errorCode, errorMessage) =>
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
