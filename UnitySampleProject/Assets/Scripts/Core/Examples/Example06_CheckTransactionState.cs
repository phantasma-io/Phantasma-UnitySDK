using System;
using System.Collections;
using PhantasmaPhoenix.Protocol;
using UnityEngine;

// Unity MonoBehaviour used to demonstrate how to check the status of a blockchain transaction
public class Example06_CheckTransactionState : MonoBehaviour
{
    // Entry point of example
    public void Run()
    {
        // Get reference to the scene-wide manager that stores API config and global variables
        var manager = FindObjectOfType<CoreExampleManager>();

        // Access the initialized Phantasma API instance
        var api = manager.phantasmaAPI;

        // Transaction hash to monitor - must be set in CoreExampleManager
        var txHash = manager.TransactionHash;

        // Start coroutine that will keep checking the transaction status until it's complete or times out
        StartCoroutine(CheckTxStateLoop(txHash, null));
    }

    // Coroutine that monitors the state of a transaction; invokes callback with status and result when complete.
    // Callback is optional.
    // If we could not determine state of tx, null will be passed as first callback argument
    public static IEnumerator CheckTxStateLoop(string txHash, Action<ExecutionState?, string, string> callback)
    {
        var manager = FindObjectOfType<CoreExampleManager>();
        var api = manager.phantasmaAPI;

        // Flag to stop polling loop once the transaction is finalized
        bool done = false;

        // Counter for how many times we've polled for transaction status
        uint txStatusQueryAttempts = 0;

        // Counter for how many times we've attempted to get failure debug details, if needed
        uint failureDetailsQueryAttempts = 0;

        while (!done)
        {
            // Make RPC call to fetch transaction info from the chain
            yield return api.GetTransaction(txHash,
                (txResult) =>
                {
                    // Log the current execution state: Running, Halt (success), or other (failure)
                    Debug.Log($"Transaction state is: {txResult.State}");

                    switch (txResult.State)
                    {
                        case PhantasmaPhoenix.Protocol.ExecutionState.Running:
                            // Transaction is still being processed by the chain
                            Debug.Log("Transaction is still processing...");
                            break;

                        case PhantasmaPhoenix.Protocol.ExecutionState.Halt:
                            // Transaction completed successfully (execution halted without errors)

                            // Check if any result string is available (may be empty if not applicable)
                            if (string.IsNullOrEmpty(txResult.Result))
                            {
                                Debug.Log($"Transaction executed successfully, no result available.");
                            }
                            else
                            {
                                Debug.Log($"Transaction executed successfully with result '{txResult.Result}'.");
                            }
                            done = true;

                            // Notify success with result value and no error info
                            callback?.Invoke(txResult.State, txResult.Result, null);
                            break;

                        default:
                            // Transaction failed. We check if we have additional details about failure available.
                            // If failure details are not yet available, and we haven't tried too many times - wait and retry
                            if (txResult.DebugComment == null && failureDetailsQueryAttempts < 6)
                            {
                                // Inform user that we're retrying in case debug info hasn't yet been indexed on the node
                                Debug.Log($"Waiting for failure details... Attempt {failureDetailsQueryAttempts + 1}/6");
                                failureDetailsQueryAttempts++;
                                break;
                            }

                            // Final failure state reached, log failure details and return via callback
                            Debug.LogWarning($"Transaction failed with state: {txResult.State}. Result: {txResult.Result}. Details: {txResult.DebugComment}");
                            done = true;

                            // Notify failure with state, raw result, and debug comment if available
                            callback?.Invoke(txResult.State, txResult.Result, txResult.DebugComment);
                            break;
                    }
                },
                // Error handler for network or RPC-level errors
                (errorCode, errorMessage) =>
                {
                    // Log API error such as invalid hash, RPC timeout, etc
                    // "Transaction not found" also gets here
                    Debug.LogError($"[Error][{errorCode}] {errorMessage}");
                });

            // If still running (or DebugComment is unavailable), wait 1 second before checking again
            if (!done)
            {
                // Stop retrying if status check exceeded max allowed attempts
                if (txStatusQueryAttempts == 30)
                {
                    Debug.LogWarning($"Query attempts exhausted after {txStatusQueryAttempts} attempts, tx state could not be confirmed");
                    // Notify that the transaction status could not be confirmed at all (timeout case)
                    callback?.Invoke(null, null, null);
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                    txStatusQueryAttempts++;
                }
            }
        }
    }
}
