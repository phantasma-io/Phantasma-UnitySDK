using System;
using System.Collections;
using System.Collections.Generic;
using PhantasmaPhoenix.Core;
using PhantasmaPhoenix.Cryptography;
using PhantasmaPhoenix.Protocol;
using PhantasmaPhoenix.RPC.Models;
using UnityEngine;

// Unity MonoBehaviour used to demonstrate how to monitor incoming transactions 
// by sequentially reading new blocks from the chain and filtering for TokenReceive events
public class Example10_WaitIncomingTx_ReadBlocks : MonoBehaviour
{
    // In-memory cache mapping token symbol to its decimals
    private readonly Dictionary<string, uint> _tokenDecimals = new(StringComparer.OrdinalIgnoreCase);

    // Entry point of example
    public void Run()
    {
        // Get reference to the scene-wide manager that stores API config and global variables
        var manager = FindObjectOfType<CoreExampleManager>();

        // Address to monitor - configured in the Unity inspector
        var address = manager.TestAddress;

        // Start coroutine to wait for incoming transfers by scanning new blocks
        StartCoroutine(WaitIncomingTransfers(address));
    }

    // Coroutine that scans blocks by height and processes TokenReceive events for the specified address
    private IEnumerator WaitIncomingTransfers(string address)
    {
        var manager = FindObjectOfType<CoreExampleManager>();
        var api = manager.phantasmaAPI;

        const string chain = "main";

        long height = -1;
        bool gotHeight = false;

        // --- Step 1: Initialize starting block height ---
        while (!gotHeight)
        {
            // Fetch latest block height for the chain
            yield return api.GetBlockHeight(chain,
                h => { height = h; gotHeight = true; },
                // Callback for RPC errors (network issue, chain not found, etc.)
                (code, msg) => { Debug.LogError($"[Error][{code}] GetBlockHeight failed: {msg}"); });

            if (!gotHeight)
                yield return new WaitForSeconds(1f); // Retry delay before trying again
        }

        Debug.Log($"[Init] Starting from block #{height}");

        // --- Step 2: Continuous block scanning loop ---
        while (true)
        {
            // Fetch block data for the current height
            BlockResult block = null;
            yield return api.GetBlockByHeight(chain, height.ToString(),
                b => block = b,
                // Callback for RPC errors (invalid height, network error, etc.)
                (code, msg) => Debug.LogError($"[Error][{code}] GetBlockByHeight({height}) failed: {msg}"));

            if (block?.Txs != null)
            {
                foreach (var tx in block.Txs)
                {
                    // Only process successful transactions
                    if (tx.State != ExecutionState.Halt)
                        continue;

                    if (tx.Events == null) continue;

                    foreach (var e in tx.Events)
                    {
                        // Expect TokenReceive for the monitored address
                        var eventKind = Enum.Parse<EventKind>(e.Kind);
                        if (eventKind != EventKind.TokenReceive || !string.Equals(e.Address, address, StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Decode TokenEventData from event payload
                        var dataBytes = Base16.Decode(e.Data);
                        var data = Serialization.Unserialize<TokenEventData>(dataBytes);

                        // Get decimals from cache or fetch from API
                        uint decimals;
                        if (!_tokenDecimals.TryGetValue(data.Symbol, out decimals))
                        {
                            bool fetched = false;
                            yield return api.GetToken(data.Symbol,
                                t =>
                                {
                                    decimals = t.Decimals;
                                    _tokenDecimals[data.Symbol] = decimals; // cache for reuse
                                    fetched = true;
                                },
                                // Callback for RPC errors (invalid token, network error, etc.)
                                (code, msg) =>
                                {
                                    Debug.LogError($"[Error][{code}] GetToken({data.Symbol}) failed: {msg}");
                                    decimals = 0; // fallback to 0 to continue
                                    fetched = true;
                                });
                            while (!fetched) yield return null;
                        }

                        // Convert chain amount to human-readable format
                        var human = UnitConversion.ToDecimal(data.Value, decimals);

                        Debug.Log($"Address {e.Address} received {human} {data.Symbol}");
                    }
                }
            }

            // --- Step 3: Wait until a new block is produced ---
            while (true)
            {
                long newHeight = height;
                bool got = false;

                // Fetch latest block height again
                yield return api.GetBlockHeight(chain,
                    h => { newHeight = h; got = true; },
                    // Callback for RPC errors (network issue, etc.)
                    (code, msg) => { Debug.LogError($"[Error][{code}] GetBlockHeight wait failed: {msg}"); got = true; });

                // If chain height increased, move to next block
                if (got && newHeight > height)
                {
                    height += 1;
                    break;
                }

                // Delay before re-checking block height
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
