using System;
using System.Collections;
using System.Globalization;
using System.Text;
using PhantasmaPhoenix.Cryptography;
using PhantasmaPhoenix.Protocol.Carbon;
using PhantasmaPhoenix.Protocol.Carbon.Blockchain;
using PhantasmaPhoenix.RPC.Models;
using PhantasmaPhoenix.RPC.Types;
using PhantasmaPhoenix.Unity.Core.Logging;
using UnityEngine;

namespace PhantasmaPhoenix.Unity.Core
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
        /// Gets account information, including balances, for the specified address
        /// </summary>
        /// <param name="addressText"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAccount(string addressText, Action<AccountResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<AccountResult>(Host, "getAccount", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, addressText);
        }
        
        /// <summary>
        /// Gets account information for multiple addresses
        /// </summary>
        /// <param name="addressText"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAccounts(string[] addresses, Action<AccountResult[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            if (addresses == null || addresses.Length == 0)
            {
                callback(new AccountResult[0]);
                yield break;
            }
            
            yield return WebClient.RPCRequest<AccountResult[]>(Host, "getAccounts", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, String.Join(",",addresses));
        }

        /// <summary>
        /// Looks up an address by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator LookUpName(string name, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<string>(Host, "lookUpName", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, name);
        }
        #endregion
        
        #region Auction
        /// <summary>
        /// Gets the number of auctions currently available in the market contract for a given token
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="chainAddressOrName"></param>
        /// <param name="symbol"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAuctionsCount(string chainAddressOrName, string symbol, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<string>(Host, "getAuctionsCount", timeout, retries, errorHandlingCallback, (result) => {
                callback(int.Parse(result));
            }, chainAddressOrName, symbol);
        }

        /// <summary>
        /// Gets all auctions currently available in the market contract for a given token, with pagination
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="chainAddressOrName"></param>
        /// <param name="symbol"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAuctions(string chainAddressOrName, string symbol, uint page, uint pageSize, Action<AuctionResult[], uint, uint, uint> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<PaginatedResult<AuctionResult[]>>(Host, "getAuctions", timeout, retries, errorHandlingCallback, (result) => {
                callback(result.Result, result.Page, result.Total, result.TotalPages);
            }, chainAddressOrName, symbol, page, pageSize);
        }


        /// <summary>
        /// Gets a single auction by symbol and auction id
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="chainAddressOrName"></param>
        /// <param name="symbol"></param>
        /// <param name="IDtext"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAuction(string chainAddressOrName, string symbol, string IDtext, Action<AuctionResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<AuctionResult>(Host, "getAuction", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, chainAddressOrName, symbol, IDtext);
        }
        #endregion

        #region Block
        /// <summary>
        /// Gets the latest block height for a chain
        /// </summary>
        /// <param name="chainInput"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetBlockHeight(string chainInput, Action<long> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<string>(Host, "getBlockHeight", timeout, retries, errorHandlingCallback, (result) => {
                callback(long.Parse(result));
            }, chainInput);
        }


        /// <summary>
        /// Gets the number of transactions in a block by block hash
        /// </summary>
        /// <param name="blockHash"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetBlockTransactionCountByHash(string blockHash, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<string>(Host, "getBlockTransactionCountByHash", timeout, retries, errorHandlingCallback, (result) => {
                callback(int.Parse(result));
            }, blockHash);
        }

        /// <summary>
        /// Gets a block by its hash
        /// </summary>
        /// <param name="blockHash"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetBlockByHash(string blockHash, Action<BlockResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<BlockResult>(Host, "getBlockByHash", timeout, retries, errorHandlingCallback, (result) =>
            {
                callback(result);
            }, blockHash);
        }

        /// <summary>
        /// Gets a block by chain and height
        /// </summary>
        /// <param name="chainInput"></param>
        /// <param name="height"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetBlockByHeight(string chainInput, long height, Action<BlockResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<BlockResult>(Host, "getBlockByHeight", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, chainInput, height.ToString());
        }
        
        /// <summary>
        /// Gets the latest block for a chain
        /// </summary>
        /// <param name="blockHash"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetLatestBlock(string chainInput, Action<BlockResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<BlockResult>(Host, "getLatestBlock", timeout, retries, errorHandlingCallback, (result) =>
            {
                callback(result);
            }, chainInput);
        }

        /// <summary>
        /// Gets a transaction by block hash and transaction index
        /// </summary>
        /// <param name="blockHash"></param>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTransactionByBlockHashAndIndex(string blockHash, int index, Action<TransactionResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<TransactionResult>(Host, "getTransactionByBlockHashAndIndex", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, blockHash, index);
        }
        #endregion

        #region Chain
        /// <summary>
        /// Gets an array of all chains deployed on Phantasma
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetChains(Action<ChainResult[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<ChainResult[]>(Host, "getChains", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            });
        }
        #endregion

        #region Contract
        /// <summary>
        /// Gets contract metadata by name from the main chain
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="contractName"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetContract(string contractName, Action<ContractResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<ContractResult>(Host, "getContract", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, PhantasmaPhoenix.Protocol.DomainSettings.RootChainName, contractName);
        }
        
        /// <summary>
        /// Gets all contracts deployed on the main chain
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetContracts(Action<ContractResult[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<ContractResult[]>(Host, "getContracts", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, PhantasmaPhoenix.Protocol.DomainSettings.RootChainName);
        }
        #endregion
        
        #region Leaderboard
        /// <summary>
        /// Gets a leaderboard by name
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetLeaderboard(string name, Action<LeaderboardResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<LeaderboardResult>(Host, "getLeaderboard", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, name);
        }
        #endregion
        
        #region Nexus
        /// <summary>
        /// Gets nexus metadata including an array of all chains deployed on Phantasma
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetNexus(Action<NexusResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<NexusResult>(Host, "getNexus", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            });
        }
        #endregion
        
        #region Organization
        /// <summary>
        /// Gets organization data by id
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetOrganization(string ID, Action<OrganizationResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<OrganizationResult>(Host, "getOrganization", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, ID);
        }
        
        /// <summary>
        /// Gets organization data by name
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetOrganizationByName(string name, Action<OrganizationResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<OrganizationResult>(Host, "getOrganizationByName", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, name);
        }
        
        /// <summary>
        /// Gets all organizations deployed on Phantasma
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetOrganizations(Action<OrganizationResult[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<OrganizationResult[]>(Host, "getOrganizations", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            });
        }
        #endregion
        
        #region Token
        private int tokensLoadedSimultaneously = 0;

        /// <summary>
        /// Gets token metadata by symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetToken(string symbol, Action<TokenResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<TokenResult>(Host, "getToken", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, symbol);
        }

        /// <summary>
        /// Gets an array of all tokens deployed on Phantasma
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTokens(Action<TokenResult[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<TokenResult[]>(Host, "getTokens", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            });
        }

        /// <summary>
        /// Gets token data for a specific token id
        /// <para><b>⚠️ This functionality is only partially implemented - some features may be missing or incomplete. See the roadmap for planned updates: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="IDtext"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTokenData(string symbol, string IDtext, Action<TokenDataResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            while (tokensLoadedSimultaneously > 5)
            {
                yield return null;
            }
            tokensLoadedSimultaneously++;

            yield return WebClient.RPCRequest<TokenDataResult>(Host, "getTokenData", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, symbol, IDtext);

            tokensLoadedSimultaneously--;
        }

        /// <summary>
        /// Gets NFT data and optionally loads properties
        /// <para><b>⚠️ This functionality is only partially implemented - some features may be missing or incomplete. See the roadmap for planned updates: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="IDtext"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetNFT(string symbol, string IDtext, bool loadProperties, Action<TokenDataResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            while (tokensLoadedSimultaneously > 5)
            {
                yield return null;
            }
            tokensLoadedSimultaneously++;

            yield return WebClient.RPCRequest<TokenDataResult>(Host, "getNFT", timeout, retries, errorHandlingCallback, (result) => {
                // TODO remove later, check if still required
                if (string.IsNullOrEmpty(result.Id))
                {
                    result.Id = IDtext;
                }

                callback(result);
            }, symbol, IDtext, loadProperties);

            tokensLoadedSimultaneously--;
        }

        /// <summary>
        /// Gets NFT data for multiple token ids
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="IDtext"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetNFTs(string symbol, string[] IDtext, Action<TokenDataResult[]> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            while (tokensLoadedSimultaneously > 5)
            {
                yield return null;
            }

            tokensLoadedSimultaneously++;

            yield return WebClient.RPCRequest<TokenDataResult[]>(Host, "getNFTs", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, symbol, IDtext);

            tokensLoadedSimultaneously--;
        }

        /// <summary>
        /// Gets the token balance for a given address and token symbol
        /// </summary>
        /// <param name="account"></param>
        /// <param name="tokenSymbol"></param>
        /// <param name="chainInput"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTokenBalance(string account, string tokenSymbol, string chainInput = "main", Action<BalanceResult> callback = null, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<BalanceResult>(Host, "getTokenBalance", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, account, tokenSymbol, chainInput);
        }
        #endregion
        
        #region Transaction
        /// <summary>
        /// Gets address transactions with pagination
        /// This api call is paginated, multiple calls might be required to obtain a complete result 
        /// </summary>
        /// <param name="addressText"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAddressTransactions(string addressText, uint page, uint pageSize, Action<AccountTransactionsResult, uint, uint> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<PaginatedResult<AccountTransactionsResult>>(Host, "getAddressTransactions", timeout, retries, errorHandlingCallback, (result) => {
                callback(result.Result, result.Page, result.TotalPages);
            }, addressText, page, pageSize);
        }

        /// <summary>
        /// Gets the number of transactions for an address on a chain
        /// </summary>
        /// <param name="addressText"></param>
        /// <param name="chainInput"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetAddressTransactionCount(string addressText, string chainInput, Action<int> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<string>(Host, "getAddressTransactionCount", timeout, retries, errorHandlingCallback, (result) => {
                callback(int.Parse(result));
            }, addressText, chainInput);
        }

        /// <summary>
        /// Broadcasts a transaction in hexadecimal encoding
        /// </summary>
        /// <param name="txData"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator SendRawTransaction(string txData, Hash txHash, Action<string, string, Hash> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<string>(Host, "sendRawTransaction", timeout, retries, errorHandlingCallback, (result) =>
            {
                callback(result, txData, txHash);
            }, txData);
        }

        /// <summary>
        /// Broadcasts a carbon transaction in hexadecimal encoding
        /// </summary>
        /// <param name="txData"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator SendCarbonTransaction(string txData, Action<string, string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<string>(Host, "sendCarbonTransaction", timeout, retries, errorHandlingCallback, (result) =>
            {
                callback(result, txData);
            }, txData);
        }

        /// <summary>
        /// Invokes a VM script without state changes and returns its result
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="chainInput"></param>
        /// <param name="scriptData"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator InvokeRawScript(string chainInput, string scriptData, Action<ScriptResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<ScriptResult>(Host, "invokeRawScript", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, chainInput, scriptData);
        }

        /// <summary>
        /// Gets a transaction by its hash if available
        /// </summary>
        /// <param name="hashText"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetTransaction(string hashText, Action<TransactionResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<TransactionResult>(Host, "getTransaction", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, hashText);
        }
        #endregion

        #region Storage
        /// <summary>
        /// Gets archive metadata by its hash
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="hashText"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator GetArchive(string hashText, Action<ArchiveResult> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<ArchiveResult>(Host, "getArchive", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, hashText);
        }

        /// <summary>
        /// Writes a single archive block
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="hashText"></param>
        /// <param name="blockIndex"></param>
        /// <param name="blockContent"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator WriteArchive(string hashText, int blockIndex, byte[] blockContent, Action<Boolean> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<string>(Host, "writeArchive", timeout, retries, errorHandlingCallback, (result) =>
            {
                callback(Boolean.Parse(result));
            }, hashText, blockIndex, Convert.ToBase64String(blockContent));
        }

        /// <summary>
        /// Reads a single archive block as a base64 string
        /// <para><b>⚠️ Currently disabled - this functionality is not available and will be re-enabled according to the roadmap: https://phantasma.info/blockchain#roadmap</b></para>
        /// </summary>
        /// <param name="hashText"></param>
        /// <param name="blockIndex"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator ReadArchive(string hashText, int blockIndex, Action<string> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            yield return WebClient.RPCRequest<string>(Host, "readArchive", timeout, retries, errorHandlingCallback, (result) => {
                callback(result);
            }, hashText, blockIndex);
        }
        #endregion
        
        #region Other Transaction Methods
        /// <summary>
        /// Sign and send a transaction with the payload
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="nexus"></param>
        /// <param name="script"></param>
        /// <param name="chain"></param>
        /// <param name="payload"></param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <returns></returns>
        public IEnumerator SignAndSendTransaction(IKeyPair keys, string nexus, byte[] script, string chain, string payload, Action<string /*tx hash*/, string /*encoded tx*/> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            return SignAndSendTransaction(keys, nexus, script, chain, Encoding.UTF8.GetBytes(payload), callback, errorHandlingCallback, null, timeout, retries);
        }

        /// <summary>
        /// Sign and send a transaction with the payload
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
        public IEnumerator SignAndSendTransaction(IKeyPair keys, string nexus, byte[] script, string chain, byte[] payload, Action<string /*tx hash*/, string /*encoded tx*/> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, Func<byte[], byte[], byte[], byte[]> customSignFunction = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            Log.Write("Sending transaction... script size: " + script.Length);

            var tx = new PhantasmaPhoenix.Protocol.Transaction(nexus, chain, script, DateTime.UtcNow + TimeSpan.FromMinutes(20), payload);

            // Local hash we expect to see on the node
            Hash txHash = tx.Sign(keys, customSignFunction);

            var encoded = Base16.Encode(tx.ToByteArray(true));

            // Wrap user callback to validate RPC hash before signaling success
            Action<string, string, Hash> wrappedCallback = (hashText, encodedTx, rpcHash) =>
            {
                // Prefer comparing Hash structs if both are available
                if (rpcHash != txHash)
                {
                    // Treat mismatch as an error and do not invoke success callback
                    errorHandlingCallback?.Invoke(EPHANTASMA_SDK_ERROR_TYPE.API_ERROR, $"RPC returned different hash {rpcHash}, expected {txHash}");
                    return;
                }

                // Hashes match - forward original callback
                callback?.Invoke(hashText, encodedTx);
            };

            // Send to network and validate on callback
            yield return SendRawTransaction(encoded, txHash, wrappedCallback, errorHandlingCallback, timeout, retries);
        }

        /// <summary>
        /// Signs, serializes and broadcasts a carbon transaction
        /// </summary>
        /// <param name="keys">Key pair used to sign a transaction</param>
        /// <param name="txMsg">Carbon TxMsg struct</param>
        /// <param name="callback"></param>
        /// <param name="errorHandlingCallback"></param>
        /// <param name="customSignFunction"></param>
        /// <returns></returns>
        public IEnumerator SignAndSendCarbonTransaction(IKeyPair keys, TxMsg txMsg, Action<string /*tx hash*/, string /*encoded tx*/> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries)
        {
            Log.Write("Sending carbon transaction...");

            var signedTxMsg = new SignedTxMsg
            {
                msg = txMsg,
                witnesses = new Witness[] {
                new Witness
                {
                    address = new Bytes32(keys.PublicKey),
                    signature = new Bytes64(Ed25519.Sign(CarbonBlob.Serialize(txMsg), keys.PrivateKey))
                }
            }
            };

            var signedTxBytes = CarbonBlob.Serialize(signedTxMsg);
            var encoded = Base16.Encode(signedTxBytes);

            // Send to network and validate on callback
            yield return SendCarbonTransaction(encoded, callback, errorHandlingCallback, timeout, retries);
        }
        #endregion

        /// <summary>
        /// Validates a WIF-formatted private key string
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsValidPrivateKey(string key)
        {
            return (key.StartsWith("L", false, CultureInfo.InvariantCulture) ||
                    key.StartsWith("K", false, CultureInfo.InvariantCulture)) && key.Length == 52;
        }

        /// <summary>
        /// Validates the format of a Phantasma address string
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsValidAddress(string address)
        {
            return address.StartsWith("P", false, CultureInfo.InvariantCulture) && address.Length == 45;
        }
    }
}