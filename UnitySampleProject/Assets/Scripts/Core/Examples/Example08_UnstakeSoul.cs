using UnityEngine;
using System;
using PhantasmaPhoenix.VM;
using PhantasmaPhoenix.Cryptography;
using PhantasmaPhoenix.Core;
using PhantasmaPhoenix.Protocol;

// Unity MonoBehaviour used to demonstrate how to unstake SOUL tokens
public class Example08_UnstakeSoul : MonoBehaviour
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
        
        // Amount to unstake - configured in the Unity inspector
        var amount = manager.TokenAmount;

        // Not used right now, use as is
        var feePrice = 100000; // TODO: Adapt to new fee model.
        var feeLimit = 21000; // TODO: Adapt to new fee model.

        byte[] script;

        try
        {
            // ScriptBuilder is used to create a serialized transaction script
            var sb = new ScriptBuilder();
            // Instruction to allow gas fees for the transaction - required by all transaction scripts
            sb.AllowGas(senderAddress, Address.Null, feePrice, feeLimit);

            // Call the 'Unstake' method in the 'stake' contract with sender address and converted to big integer token amount
            sb.CallContract("stake", "Unstake", senderAddress, UnitConversion.ToBigInteger((decimal)amount, DomainSettings.StakingTokenDecimals));

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

        // Sign and send the transaction using the generated script and optional payload comment
        StartCoroutine(api.SignAndSendTransactionWithPayload(keys, nexus, script, "main", "example8-tx-payload",
            // Callback on success
            (hashText, encodedTx, txHash) =>
            {
                if (!string.IsNullOrEmpty(hashText))
                {
                    Debug.Log($"Transaction was sent, hash: {hashText}. Check transaction status using GetTransaction() call");

                    // Start polling to track transaction execution status on-chain
                    StartCoroutine(Example06_CheckTransactionState.CheckTxStateLoop(hashText, null));
                    return;
                }
                else
                {
                    Debug.LogError("[Error] Empty transaction hash returned, but no explicit error");
                }
            },
            // Callback for RPC errors (invalid token, network error, etc.)
            (errorCode, errorMessage) =>
            {
                Debug.LogError($"[Error][{errorCode}] Failed to send transaction: {errorMessage}");
            }));
    }
}
