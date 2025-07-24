using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Phantasma.Core.Cryptography;
using Phantasma.Core.Domain;
using Phantasma.Core.Numerics;
using PhantasmaPhoenix.Unity.Core.Logging;
using UnityEngine;
using Event = Phantasma.Core.Domain.Event;

namespace Phantasma.SDK
{
    public class PhantasmaAPI
    {
        /// <summary>
        /// Host needs to be an RPC call, i.e. http://127.0.0.1:7077/rpc
        /// </summary>
        public readonly string Host;

        public PhantasmaAPI(string host)
        {
            this.Host = host;
        }

        #region Account
        /// <summary>
        /// Returns the account name and balance of given address.
        /// </summary>
        /// <param name="addressText"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAccount(string addressText, Action<Account> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Account>(Host, "getAccount", WebClient.DefaultTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, addressText);
        }
        
        /// <summary>
        /// Returns the account name and balance of given address.
        /// </summary>
        /// <param name="addressText"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAccounts(string[] addresses, Action<Account[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            if (addresses == null || addresses.Length == 0)
            {
                callback(new Account[0]);
                yield break;
            }
            
            yield return WebClient.RPCRequest<Account[]>(Host, "getAccounts", WebClient.DefaultTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, String.Join(",",addresses));
        }

        /// <summary>
        /// Returns the address that owns a given name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator LookUpName(string name, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "lookUpName", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, name);
        }
        #endregion
        
        #region Auction
        /// <summary>
        /// Returns the number of active auctions.
        /// </summary>
        /// <param name="chainAddressOrName"></param>
        /// <param name="symbol"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAuctionsCount(string chainAddressOrName, string symbol, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "getAuctionsCount", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(int.Parse(result));
            }, chainAddressOrName, symbol);
        }

        /// <summary>
        /// Returns the auctions available in the market.
        /// </summary>
        /// <param name="chainAddressOrName"></param>
        /// <param name="symbol"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAuctions(string chainAddressOrName, string symbol, uint page, uint pageSize, Action<Auction[], uint, uint, uint> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<PaginatedResult<Auction[]>>(Host, "getAuctions", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result.Result, result.Page, result.Total, result.TotalPages);
            }, chainAddressOrName, symbol, page, pageSize);
        }


        /// <summary>
        /// Returns the auction for a specific token and ID
        /// </summary>
        /// <param name="chainAddressOrName"></param>
        /// <param name="symbol"></param>
        /// <param name="IDtext"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAuction(string chainAddressOrName, string symbol, string IDtext, Action<Auction> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Auction>(Host, "getAuction", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, chainAddressOrName, symbol, IDtext);
        }
        #endregion

        #region Block
        /// <summary>
        /// Returns the height of a chain.
        /// </summary>
        /// <param name="chainInput"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetBlockHeight(string chainInput, Action<long> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "getBlockHeight", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(long.Parse(result));
            }, chainInput);
        }


        /// <summary>
        /// Returns the number of transactions of given block hash or error if given hash is invalid or is not found.
        /// </summary>
        /// <param name="blockHash"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetBlockTransactionCountByHash(string blockHash, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "getBlockTransactionCountByHash", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(int.Parse(result));
            }, blockHash);
        }

        /// <summary>
        /// Returns information about a block by hash.
        /// </summary>
        /// <param name="blockHash"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetBlockByHash(string blockHash, Action<Block> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "getBlockByHash", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) =>
            {
                var block = JsonUtility.FromJson<Block>(result);
                callback(block);
            }, blockHash);
        }

        /// <summary>
        /// Returns information about a block by height and chain.
        /// </summary>
        /// <param name="chainInput"></param>
        /// <param name="height"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetBlockByHeight(string chainInput, uint height, Action<Block> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "getBlockByHeight", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                var block = JsonUtility.FromJson<Block>(result);
                callback(block);
            }, chainInput, height);
        }
        
        /// <summary>
        /// Returns information about a block by hash.
        /// </summary>
        /// <param name="blockHash"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetLatestBlock(string chainInput, Action<Block> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "getLatestBlock", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) =>
            {
                var block = JsonUtility.FromJson<Block>(result);
                callback(block);
            }, chainInput);
        }

        /// <summary>
        /// Returns the information about a transaction requested by a block hash and transaction index.
        /// </summary>
        /// <param name="blockHash"></param>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTransactionByBlockHashAndIndex(string blockHash, int index, Action<Transaction> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "getTransactionByBlockHashAndIndex", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                var tx = JsonUtility.FromJson<Transaction>(result);
                callback(tx);
            }, blockHash, index);
        }
        #endregion

        #region Chain
        /// <summary>
        /// Returns an array of all chains deployed in Phantasma.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetChains(Action<Chain[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Chain[]>(Host, "getChains", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            });
        }
        #endregion

        #region Contract
        /// <summary>
        /// Returns the ContractData
        /// </summary>
        /// <param name="contractName"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetContract(string contractName, Action<Contract> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Contract>(Host, "getContract", WebClient.DefaultTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, DomainSettings.RootChainName, contractName);
        }
        
        /// <summary>
        /// Returns an array of ContractData
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetContracts(Action<Contract[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Contract[]>(Host, "getContracts", WebClient.DefaultTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, DomainSettings.RootChainName);
        }
        #endregion
        
        #region Leaderboard
        /// <summary>
        /// Return the Leaderboard for a specific address
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetLeaderboard(string name, Action<Leaderboard> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Leaderboard>(Host, "getLeaderboard", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, name);
        }
        #endregion
        
        #region Nexus
        /// <summary>
        /// Returns an array of all chains deployed in Phantasma.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetNexus(Action<Nexus> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Nexus>(Host, "getNexus", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            });
        }
        #endregion
        
        #region Organization
        /// <summary>
        /// Returns the organization with the given ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetOrganization(string ID, Action<Organization> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Organization>(Host, "getOrganization", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, ID);
        }
        
        /// <summary>
        /// Returns the organization with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetOrganizationByName(string name, Action<Organization> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Organization>(Host, "getOrganizationByName", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, name);
        }
        
        /// <summary>
        /// Returns all organizations
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetOrganizations(Action<Organization[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Organization[]>(Host, "getOrganizations", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            });
        }
        #endregion
        
        #region Token
        private int tokensLoadedSimultaneously = 0;

        /// <summary>
        /// Returns info about a specific token deployed in Phantasma.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetToken(string symbol, Action<Token> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Token>(Host, "getToken", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, symbol);
        }

        /// <summary>
        /// Returns an array of tokens deployed in Phantasma.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTokens(Action<Token[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Token[]>(Host, "getTokens", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            });
        }

        /// <summary>
        /// Returns data of a non-fungible token, in hexadecimal format.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="IDtext"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTokenData(string symbol, string IDtext, Action<TokenData> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            while (tokensLoadedSimultaneously > 5)
            {
                yield return null;
            }
            tokensLoadedSimultaneously++;

            yield return WebClient.RPCRequest<TokenData>(Host, "getTokenData", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, symbol, IDtext);

            tokensLoadedSimultaneously--;
        }

        /// <summary>
        /// Returns data of a non-fungible token, in hexadecimal format.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="IDtext"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetNFT(string symbol, string IDtext, Action<TokenData> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            while (tokensLoadedSimultaneously > 5)
            {
                yield return null;
            }
            tokensLoadedSimultaneously++;

            yield return WebClient.RPCRequest<TokenData>(Host, "getNFT", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, symbol, IDtext);

            tokensLoadedSimultaneously--;
        }

        /// <summary>
        /// Returns data of a non-fungible tokens, in hexadecimal format.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="IDtext"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetNFTs(string symbol, string[] IDtext, Action<TokenData[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            while (tokensLoadedSimultaneously > 5)
            {
                yield return null;
            }

            tokensLoadedSimultaneously++;

            yield return WebClient.RPCRequest<TokenData[]>(Host, "getNFTs", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, symbol, IDtext);

            tokensLoadedSimultaneously--;
        }

        /// <summary>
        /// Returns the token balance for a specific address and token symbol
        /// </summary>
        /// <param name="account"></param>
        /// <param name="tokenSymbol"></param>
        /// <param name="chainInput"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTokenBalance(string account, string tokenSymbol, string chainInput = "main", Action<Balance> callback = null, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Balance>(Host, "getTokenBalance", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, account, tokenSymbol, chainInput);
        }
        #endregion
        
        #region Transaction
        /// <summary>
        /// Returns last X transactions of given address.
        /// This api call is paginated, multiple calls might be required to obtain a complete result 
        /// </summary>
        /// <param name="addressText"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAddressTransactions(string addressText, uint page, uint pageSize, Action<Account, uint, uint> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<PaginatedResult<Account>>(Host, "getAddressTransactions", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result.Result, result.Page, result.TotalPages);
            }, addressText, page, pageSize);
        }

        /// <summary>
        /// Get number of transactions in a specific address and chain
        /// </summary>
        /// <param name="addressText"></param>
        /// <param name="chainInput"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAddressTransactionCount(string addressText, string chainInput, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "getAddressTransactionCount", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(int.Parse(result));
            }, addressText, chainInput);
        }

        /// <summary>
        /// Allows to broadcast a signed operation on the network, but it&apos;s required to build it manually.
        /// </summary>
        /// <param name="txData"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator SendRawTransaction(string txData, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "sendRawTransaction", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, txData);
        }

        /// <summary>
        /// Allows to invoke script based on network state, without state changes.
        /// </summary>
        /// <param name="chainInput"></param>
        /// <param name="scriptData"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator InvokeRawScript(string chainInput, string scriptData, Action<Script> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Script>(Host, "invokeRawScript", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, chainInput, scriptData);
        }

        /// <summary>
        /// Returns information about a transaction by hash.
        /// </summary>
        /// <param name="hashText"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTransaction(string hashText, Action<Transaction> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Transaction>(Host, "getTransaction", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, hashText);
        }
        #endregion

        #region Storage
        /// <summary>
        /// Returns info about a specific archive.
        /// </summary>
        /// <param name="hashText"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetArchive(string hashText, Action<Archive> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<Archive>(Host, "getArchive", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, hashText);
        }

        /// <summary>
        /// Writes the contents of an incomplete archive.
        /// </summary>
        /// <param name="hashText"></param>
        /// <param name="blockIndex"></param>
        /// <param name="blockContent"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator WriteArchive(string hashText, int blockIndex, string blockContent, Action<Boolean> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "writeArchive", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(Boolean.Parse(result));
            }, hashText, blockIndex, blockContent);
        }

        /// <summary>
        /// Returns the archive info
        /// </summary>
        /// <param name="hashText"></param>
        /// <param name="blockIndex"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator ReadArchive(string hashText, int blockIndex, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            yield return WebClient.RPCRequest<string>(Host, "readArchive", WebClient.NoTimeout, WebClient.DefaultRetries, errorHandlingCallback, (result) => {
                callback(result);
            }, hashText, blockIndex);
        }
        #endregion
        
        #region Other Transaction Methods
        /// <summary>
        /// Sign and send a transaction with the Payload
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="nexus"></param>
        /// <param name="script"></param>
        /// <param name="chain"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator SignAndSendTransaction(PhantasmaKeys keys, string nexus, byte[] script, string chain, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            return SignAndSendTransactionWithPayload(keys, nexus, script, chain, new byte[0], callback, errorHandlingCallback);
        }

        /// <summary>
        /// Sign and send a transaction with the Payload
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="nexus"></param>
        /// <param name="script"></param>
        /// <param name="chain"></param>
        /// <param name="payload"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator SignAndSendTransactionWithPayload(PhantasmaKeys keys, string nexus, byte[] script, string chain, string payload, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null)
        {
            return SignAndSendTransactionWithPayload(keys, nexus, script, chain, Encoding.UTF8.GetBytes(payload), callback, errorHandlingCallback);
        }

        /// <summary>
        /// Sign and send a transaction with the Payload
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="nexus"></param>
        /// <param name="script"></param>
        /// <param name="chain"></param>
        /// <param name="payload"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <param name="customSignFunction"></param>
        /// <returns></returns>
        public IEnumerator SignAndSendTransactionWithPayload(IKeyPair keys, string nexus, byte[] script, string chain, byte[] payload, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, Func<byte[], byte[], byte[], byte[]> customSignFunction = null)
        {
            Log.Write("Sending transaction...");

            var tx = new Core.Domain.Transaction(nexus, chain, script, DateTime.UtcNow + TimeSpan.FromMinutes(20), payload);
            tx.Sign(keys, customSignFunction);

            yield return SendRawTransaction(Base16.Encode(tx.ToByteArray(true)), callback, errorHandlingCallback);
        }
        #endregion

        /// <summary>
        /// Returns if it's a valid private key
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsValidPrivateKey(string address)
        {
            return (address.StartsWith("L", false, CultureInfo.InvariantCulture) ||
                    address.StartsWith("K", false, CultureInfo.InvariantCulture)) && address.Length == 52;
        }

        /// <summary>
        /// Returns if it's a valid address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsValidAddress(string address)
        {
            return address.StartsWith("P", false, CultureInfo.InvariantCulture) && address.Length == 45;
        }
    }
}