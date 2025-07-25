using System;
using Org.BouncyCastle.Math;
using PhantasmaPhoenix.Core;
using PhantasmaPhoenix.Cryptography;
using PhantasmaPhoenix.RPC.Models;
using PhantasmaPhoenix.Unity.Core;
using PhantasmaPhoenix.VM;
using UnityEngine;

public class WalletInteractions : MonoBehaviour
{
    public static string PhantasmaRpc = "https://testnet.phantasma.io/rpc";
    public static string RecipientAddress;
    public static string Symbol;
    public static int? Decimals;
    public static decimal Amount;
    public static string Chain;
    public static string Payload;
    public static string TokenId;
    public static string DataToSign;
    #region Events
    public event Action<string, bool> OnLoginEvent;
    #endregion
    
    /// <summary>
    /// Get the balances of the logged account.
    /// </summary>
    public void GetBalances()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged) return;
        
        PhantasmaAPI api = new PhantasmaAPI(PhantasmaRpc);
        StartCoroutine(api.GetAccount(PhantasmaLinkClient.Instance.Address, account =>
        {
            Debug.Log(account.Balances.Length);
        }));
    }

    /// <summary>
    /// Send a Raw Transaction to the blockchain - this method is use to send a transaction to the blockchain.
    /// </summary>
    public void SendRawTransaction()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(RecipientAddress))
        {
            throw new ArgumentException("Recipient address is required", nameof(RecipientAddress));
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }
        if (Decimals == null)
        {
            throw new ArgumentException("Decimals is required", nameof(Decimals));
        }
        if (Amount <= 0)
        {
            throw new ArgumentException("Amount is required", nameof(Amount));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var toAddress = Address.Parse(RecipientAddress);
        var symbol = Symbol;
        var amount = UnitConversion.ToBigInteger(Amount, Decimals.Value);
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.TransferTokens", userAddress, toAddress, symbol, amount).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }

    /// <summary>
    /// Get the transaction by hash. 
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="callback"></param>
    public void GetTransaction(string hash, Action<TransactionResult> callback)
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        
        PhantasmaAPI api = new PhantasmaAPI(PhantasmaRpc);
        StartCoroutine(api.GetTransaction(hash, callback));
    }

    /// <summary>
    /// Get NFT
    /// </summary>
    public void GetNFT()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }

        PhantasmaAPI api = new PhantasmaAPI(PhantasmaRpc);
        var symbol = Symbol;
        var ID = "";
        StartCoroutine(api.GetNFT(symbol, ID, (nft) =>
        {
            Debug.Log(nft);
        }));
    }

    /// <summary>
    /// Get Multiple NFTs in one go.
    /// </summary>
    public void GetNFTs()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }

        PhantasmaAPI api = new PhantasmaAPI(PhantasmaRpc);
        var symbol = Symbol;
        var IDs = PhantasmaLinkClient.Instance.GetNFTs(symbol);
        StartCoroutine(api.GetNFTs(symbol, IDs, (nfts) =>
        {
            Debug.Log(nfts.Length);
        }));
    }


    /// <summary>
    /// Invoke Raw Script, this method is to call the blockchain directly to get information.
    /// </summary>
    public void InvokeRawScript()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(RecipientAddress))
        {
            throw new ArgumentException("Recipient address is required", nameof(RecipientAddress));
        }

        PhantasmaAPI api = new PhantasmaAPI(PhantasmaRpc);
        var toAddress = Address.Parse(RecipientAddress);
        ScriptBuilder sb = new ScriptBuilder();
        var script = sb.
            CallContract("stake", "getStake", toAddress).
            EndScript();
        var scriptEncoded = Base16.Encode(script);
        StartCoroutine(api.InvokeRawScript("main", scriptEncoded, scriptResult =>
        {
            Debug.Log(scriptResult.Results.Length);
        }));
    }

    /// <summary>
    /// Mint an NFT - this method is use to mint an NFT.
    /// </summary>
    public void MintNFT()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(RecipientAddress))
        {
            throw new ArgumentException("Recipient address is required", nameof(RecipientAddress));
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var toAddress = Address.Parse(RecipientAddress);
        var symbol = Symbol;
        var rom = new byte[0];
        var ram = new byte[0];
        var series = new BigInteger("0");
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.MintToken", userAddress, toAddress, symbol, rom, ram, series).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }
    
    /// <summary>
    /// Update an NFT RAM - this method is use to update the RAM of an NFT.
    /// </summary>
    public void UpdateNFT()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var symbol = Symbol;
        var ram = new byte[0];
        var tokenID = new BigInteger(TokenId);
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.WriteToken", userAddress, symbol, tokenID, ram).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }

    /// <summary>
    /// Burn NFT - this method is use to burn an NFT.
    /// </summary>
    public void BurnNFT()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var symbol = Symbol;
        var tokenID = new BigInteger(TokenId);
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.BurnToken", userAddress, symbol, tokenID).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }

    /// <summary>
    /// Send NFT - this method is use to send an NFT.
    /// </summary>
    public void SendNFT()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(RecipientAddress))
        {
            throw new ArgumentException("Recipient address is required", nameof(RecipientAddress));
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var toAddress = Address.Parse(RecipientAddress);
        var symbol = Symbol;
        var tokenID = new BigInteger(TokenId);
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.TransferToken", userAddress, toAddress, symbol, tokenID).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }

    /// <summary>
    /// Infuse an NFT - this method is use to infuse an NFT.
    /// </summary>
    public void InfuseToken()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }
        if (Decimals == null)
        {
            throw new ArgumentException("Decimals is required", nameof(Decimals));
        }
        if (Amount <= 0)
        {
            throw new ArgumentException("Amount is required", nameof(Amount));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var symbol = Symbol;
        var tokenID = new BigInteger(TokenId);
        var infuseSymbol = Symbol; // IT could be an NFT
        var infuseAmount = UnitConversion.ToBigInteger(Amount, Decimals.Value);
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.InfuseToken", userAddress, symbol, tokenID, infuseSymbol, infuseAmount).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }

    /// <summary>
    /// Mint Tokens - this method is use to mint tokens.
    /// </summary>
    public void MintTokens()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(RecipientAddress))
        {
            throw new ArgumentException("Recipient address is required", nameof(RecipientAddress));
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }
        if (Decimals == null)
        {
            throw new ArgumentException("Decimals is required", nameof(Decimals));
        }
        if (Amount <= 0)
        {
            throw new ArgumentException("Amount is required", nameof(Amount));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var toAddress = Address.Parse(RecipientAddress);
        var symbol = Symbol;
        var amount = UnitConversion.ToBigInteger(Amount, Decimals.Value);
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.MintTokens", userAddress, toAddress, symbol, amount).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }

    /// <summary>
    /// Burn Tokens - this method is use to burn tokens.
    /// </summary>
    public void BurnTokens()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }
        if (Decimals == null)
        {
            throw new ArgumentException("Decimals is required", nameof(Decimals));
        }
        if (Amount <= 0)
        {
            throw new ArgumentException("Amount is required", nameof(Amount));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var symbol = Symbol;
        var amount = UnitConversion.ToBigInteger(Amount, Decimals.Value);
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.BurnTokens", userAddress, symbol, amount).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }

    /// <summary>
    /// Transfer Tokens to another address.
    /// </summary>
    public void TransferTokens()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(RecipientAddress))
        {
            throw new ArgumentException("Recipient address is required", nameof(RecipientAddress));
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }
        if (Decimals == null)
        {
            throw new ArgumentException("Decimals is required", nameof(Decimals));
        }
        if (Amount <= 0)
        {
            throw new ArgumentException("Amount is required", nameof(Amount));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var toAddress = Address.Parse(RecipientAddress);
        var symbol = Symbol;
        var amount = UnitConversion.ToBigInteger(Amount, Decimals.Value);
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.TransferTokens", userAddress, toAddress, symbol, amount).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }

    /// <summary>
    /// Transfer the whole balance of a token to another address.
    /// </summary>
    public void TransferBalance()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(RecipientAddress))
        {
            throw new ArgumentException("Recipient address is required", nameof(RecipientAddress));
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var toAddress = Address.Parse(RecipientAddress);
        var symbol = Symbol;
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.TransferBalance", userAddress, toAddress, symbol).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }

    /// <summary>
    /// Sign data with the user's private key.
    /// </summary>
    public void SignData()
    {
        if (string.IsNullOrWhiteSpace(DataToSign))
        {
            throw new ArgumentException("Data to sign is required", nameof(DataToSign));
        }
        PhantasmaLinkClient.Instance.SignData(DataToSign, (success, signed, random, data) =>
        {
            if (success)
            {
                Debug.Log($"Successfully signed data: signature: {signed}, random: {random}");
            }
            else
            {
                Debug.Log("Failed to sign data");
            }
        } );
    }

    /// <summary>
    /// MultiSig transaction.
    /// </summary>
    public void MultiSig()
    {
        if (!PhantasmaLinkClient.Instance.IsLogged)
        {
            throw new InvalidOperationException("User must be logged in before performing this action");
        }
        if (string.IsNullOrWhiteSpace(RecipientAddress))
        {
            throw new ArgumentException("Recipient address is required", nameof(RecipientAddress));
        }
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            throw new ArgumentException("Symbol is required", nameof(Symbol));
        }
        if (Decimals == null)
        {
            throw new ArgumentException("Decimals is required", nameof(Decimals));
        }
        if (Amount <= 0)
        {
            throw new ArgumentException("Amount is required", nameof(Amount));
        }

        ScriptBuilder sb = new ScriptBuilder();
        var userAddress = Address.Parse(PhantasmaLinkClient.Instance.Address);
        var toAddress = Address.Parse(RecipientAddress);
        var symbol = Symbol;
        var amount = UnitConversion.ToBigInteger(Amount, Decimals.Value);
        var payload = string.IsNullOrWhiteSpace(Payload) ? null : System.Text.Encoding.UTF8.GetBytes(Payload);
        var script = sb.AllowGas(userAddress, Address.Null, PhantasmaLinkClient.Instance.GasPrice, PhantasmaLinkClient.Instance.GasLimit ).
            CallInterop("Runtime.TransferTokens", userAddress, toAddress, symbol, amount).
            SpendGas(userAddress).
            EndScript();
        
        PhantasmaLinkClient.Instance.SendTransaction("main", script, payload, (hash, s) =>
        {
            if ( hash.IsNull )
            {
                Debug.Log("Transaction failed: " + s);
                return;
            }
            
            Debug.Log("Transaction sent: " + hash);
        });
    }


    /// <summary>
    /// Method used to connect to the wallet.
    /// </summary>
    public void OnLogin()
    {
        if (PhantasmaLinkClient.Instance.Ready)
        {
            if (!PhantasmaLinkClient.Instance.IsLogged)
                PhantasmaLinkClient.Instance.Login((result, msg) =>
                {
                    if (result)
                    {
                        // Call event to Handle Login
                        OnLoginEvent?.Invoke("Logged In", false);
                        Debug.LogWarning("Phantasma Link authorization logged");
                    }
                    else
                    {
                        OnLoginEvent?.Invoke("Phantasma Link authorization failed", true);
                        Debug.LogWarning("Phantasma Link authorization failed");
                    }
                });
            else
                OnLoginEvent?.Invoke("Logged In", false);
        }
        else
        {
            Debug.LogWarning("Phantasma Link connection is not ready");
            OnLoginEvent?.Invoke("Phantasma Link connection is not ready", true);
            PhantasmaLinkClient.Instance.Enable();
        }
    }

}
