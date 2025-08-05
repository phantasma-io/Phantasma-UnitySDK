using System;
using System.Collections.Generic;
using System.Numerics;
using NativeWebSocket;
using UnityEngine;
using UnityEngine.Events;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;
using PhantasmaPhoenix.Cryptography;
using PhantasmaPhoenix.Core;

public class PhantasmaLinkClient : MonoBehaviour
{
    public struct Balance
    {
        public readonly string symbol;
        public readonly BigInteger value;
        public readonly int decimals;
        public readonly string[] ids;

        public Balance(string symbol, BigInteger value, int decimals, string[] ids)
        {
            this.symbol = symbol;
            this.value = value;
            this.decimals = decimals;
            this.ids = ids;
        }
    }

    public static PhantasmaLinkClient Instance { get; private set; }

    [Header("Connection Version")]
    [Tooltip("Strongly recommend to use the version 2")]
    public int Version = 2;

    [SerializeField]
    [Tooltip("simnet -> for Local node, testnet -> for Testnet node, mainnet -> for the Mainnet node")]
    private string _nexus = "simnet";

    [Header("Dapp Name")]
    [Tooltip("Here is the contract name for the desired Dapp, i.e. Pharming")]
    public string DappID = "demo";

    [Header("Wallet Endpoint")]
    [Tooltip("Default value = localhost:7090 (don't change it)")]
    public string Host = "localhost:7090";

    [Header("Platform and Signature")]
    [Tooltip("This is used to sign transactions, for Phantasma blockchain use, (PlatformKind.Phantasma) and SignatureKind.ED25519 \n for Ethereum blockchain use, (PlatformKind.Ethereum) and SignatureKind.ECDSA")]
    public PlatformKind Platform = PlatformKind.Phantasma;
    public SignatureKind Signature = SignatureKind.Ed25519;

    [Space]
    [Header("Gas Setup")]
    public int GasPrice = 100000;
    public int GasLimit = 100000;

    private WebSocket websocket;

    public bool Ready { get; private set; }

    public bool Enabled { get; private set; }

    public bool Busy { get; private set; }

    public string Nexus
    {
        get { return _nexus; }
        private set { _nexus = value; }
    }
    public string Wallet { get; private set; }
    public string Token { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public bool IsLogged { get; private set; }

    public Texture2D Avatar { get; private set; }

    public IEnumerable<string> Assets => _balanceMap.Keys;

    private Dictionary<int, Action<JObject>> _requestCallbacks = new Dictionary<int, Action<JObject>>();

    private Dictionary<string, Balance> _balanceMap = new Dictionary<string, Balance>();
    private Dictionary<string, Balance> _ownershipMap = new Dictionary<string, Balance>();

    #region Events
    public static UnityEvent<bool, string> OnLogin;
    public static UnityEvent<string> OnInfo = new UnityEvent<string>();
    #endregion

    static T GetValueOrDefault<T>(JObject obj, string field, T fallback = default)
    {
        return obj.TryGetValue(field, out var token) && token.Type != JTokenType.Null
            ? token.Value<T>()
            : fallback;
    }

    /// <summary>
    /// On Awake make it Singleton
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetMessage("Loading...");
    }


    /// <summary>
    /// Run on Start
    /// </summary>
    private void Start()
    {
        this.Enable();
    }


    /// <summary>
    /// Update method
    /// </summary>
    void Update()
    {
        if (!Enabled)
        {
            return;
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    /// <summary>
    /// Set the Message to the OnInfo
    /// </summary>
    /// <param name="txt"></param>
    private void SetMessage(string txt) => OnInfo?.Invoke(txt);

    #region Requests Functions
    /// <summary>
    /// Fetch account info
    /// </summary>
    /// <param name="callback"></param>
    private void FetchAccount(Action<bool, string> callback)
    {
        SetMessage("Authorized, obtaining account info...");

        SendLinkRequest($"getAccount/{Platform}", (result) =>
        {
            var success = GetValueOrDefault<bool>(result, "success");
            if (success)
            {
                this.Name = GetValueOrDefault<string>(result, "name");
                this.Address = GetValueOrDefault<string>(result, "address");
                this.IsLogged = true;

                _balanceMap.Clear();
                _ownershipMap.Clear();

                var balances = result["balances"] as JArray;
                if (balances != null)
                {
                    foreach (var child in balances.Children<JObject>())
                    {
                        var symbol = GetValueOrDefault<string>(child, "symbol");
                        var value = GetValueOrDefault<string>(child, "value");
                        var amount = BigInteger.Parse(value);
                        var decimals = GetValueOrDefault<Int32>(child, "decimals");

                        string[] ids = child["ids"] is JArray arr
                            ? arr.Select(x => x?.Value<string>() ?? "").ToArray()
                            : Array.Empty<string>();

                        _balanceMap[symbol] = new Balance(symbol, amount, decimals, ids);
                        if (ids.Length > 0)
                            _ownershipMap[symbol] = new Balance(symbol, amount, decimals, ids);
                    }
                }

                callback?.Invoke(true, "Logged with success!");
                OnLogin?.Invoke(true, "Logged with success!");
            }
            else
            {
                callback?.Invoke(false, "could not obtain account");
                OnLogin?.Invoke(false, "could not obtain account");

            }
        });
    }

    private int requestID;

    /// <summary>
    /// Send Link Request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="callback"></param>
    private async void SendLinkRequest(string request, Action<JObject> callback)
    {
        if (!request.Contains("authorize"))
        {
            if (this.Token != null)
            {
                request = request + '/' + this.DappID + '/' + this.Token;
            }
        }


        Debug.Log("Sending Phantasma Link Request: " + request);

        requestID++;

        request = $"{requestID},{request}";

        _requestCallbacks[requestID] = callback;
        Debug.Log("Request=>" + request);
#if UNITY_ANDROID
        await PhantasmaLinkClientPluginManager.Instance.SendTransaction(request);
#endif
        await websocket.SendText(request);
    }

    /*async void SendWebSocketMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Sending bytes
            await websocket.Send(new byte[] { 10, 20, 30 });

            // Sending plain text
            await websocket.SendText("plain text message");
        }
    }*/
    #endregion   

    #region Application Defaults
    /// <summary>
    /// On Quiting the Apliction, close the websocket
    /// </summary>
    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    /// <summary>
    /// On Enable the PhantasmaLink, Create the websocket to connect to the Poltergeist Wallet.
    /// </summary>
    public async void Enable()
    {
        if (Enabled)
        {
            return;
        }

        this.Enabled = true;

        this.Wallet = "Unknown";
        this.Token = null;
        this.Nexus = _nexus;

        websocket = new WebSocket($"ws://{Host}/phantasma");

        Debug.LogWarning("Creating");

        websocket.OnOpen += () =>
        {
            this.Ready = true;
            Debug.Log("Connection open!");

        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);

        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");

        };

        websocket.OnMessage += (bytes) =>
        {
            // getting the message as a json string
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + json);

            var node = JObject.Parse(json);

            var reqID = GetValueOrDefault<Int32>(node, "id");
            if (_requestCallbacks.ContainsKey(reqID))
            {
                var callback = _requestCallbacks[reqID];
                _requestCallbacks.Remove(reqID);

                callback?.Invoke(node);
            }
            else
            {
                Debug.LogWarning("Got weird request with id " + reqID);
            }
        };

        // waiting for messages
        await websocket.Connect();
    }
    #endregion

    #region PUBLIC INTERFACE
    /// <summary>
    /// Get Balance for specific symbol
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public decimal GetBalance(string symbol)
    {
        if (_balanceMap.ContainsKey(symbol))
        {
            var temp = _balanceMap[symbol];
            return UnitConversion.ToDecimal(temp.value, temp.decimals);
        }

        return 0;
    }

    /// <summary>
    /// Returns the NFTs IDs for a specific symbol
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public string[] GetNFTs(string symbol)
    {
        if (_ownershipMap.ContainsKey(symbol))
        {
            var temp = _ownershipMap[symbol];
            return temp.ids;
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Login to the Dapp
    /// </summary>
    /// <param name="callback"></param>
    public void Login(Action<bool, string> callback = null)
    {
        if (string.IsNullOrEmpty(this.Nexus))
        {
            SetMessage("Nexus is not setup correctly...");
            return;
        }

        SetMessage("Connection established, authorizing...");

        SendLinkRequest($"authorize/{DappID}/{Version}", (result) =>
        {
            var success = GetValueOrDefault<bool>(result, "success");
            if (success)
            {
                var connectedNexus = GetValueOrDefault<string>(result, "nexus");

                if (connectedNexus != this.Nexus)
                {
                    this.IsLogged = false;
                    callback?.Invoke(false, $"invalid nexus: got {connectedNexus} but expected {this.Nexus}");
                }
                else
                {
                    this.Wallet = GetValueOrDefault<string>(result, "wallet");
                    this.Token = GetValueOrDefault<string>(result, "token");
                    this.IsLogged = true;

                    FetchAccount(callback);
                }
            }
            else
            {
                callback?.Invoke(false, "connection failed (or rejected)");
                OnLogin?.Invoke(false, "connection failed (or rejected)");
            }
        });
    }

    /// <summary>
    /// To Reload the account info
    /// </summary>
    /// <param name="callback"></param>
    public void ReloadAccount(Action<bool, string> callback = null)
    {
        FetchAccount(callback);
    }

    /// <summary>
    /// Logout from the Dapp
    /// </summary>
    /// <param name="callback">returns a bool</param>
    public void Logout()
    {
        this.Nexus = null;
        this.Wallet = "";
        this.Token = "";
        this.Avatar = null;
        this.Name = "";
        this.Address = "";
        this.IsLogged = false;
        _balanceMap.Clear();
    }

    /// <summary>
    /// Send Transaction.
    /// </summary>
    /// <param name="chain"></param>
    /// <param name="script"></param>
    /// <param name="payload"></param>
    /// <param name="callback"></param>
    public void SendTransaction(string chain, byte[] script, byte[] payload, Action<Hash, string> callback = null, PlatformKind platform = PlatformKind.Phantasma, SignatureKind signature = SignatureKind.Ed25519)
    {
        SetMessage("Relaying transaction...");

        if (script.Length >= 8192)
        {
            callback?.Invoke(Hash.Null, "script too big");
            return;
        }

        var hexScript = Base16.Encode(script);
        var hexPayload = payload != null && payload.Length > 0 ? Base16.Encode(payload) : ""; // is empty string for payload ok?
        var requestStr = $"{chain}/{hexScript}/{hexPayload}";
        if (Version >= 2)
        {
            requestStr = $"{requestStr}/{signature}/{platform}";
        }
        else
        {
            requestStr = $"{this.Nexus}/{requestStr}";
        }

        SendLinkRequest($"signTx/{requestStr}", (result) =>
        {
            var success = GetValueOrDefault<bool>(result, "success");
            if (success)
            {
                var hashStr = GetValueOrDefault<string>(result, "hash");
                var hash = Hash.Parse(hashStr);
                callback?.Invoke(hash, null);
            }
            else
            {
                var msg = GetValueOrDefault<string>(result, "message");
                callback?.Invoke(Hash.Null, "transaction rejected: " + msg);
            }
        });
    }

    /// <summary>
    /// To signed some type of data
    /// </summary>
    /// <param name="data">String with the data you want to sign</param>
    /// <param name="callback"></param>
    /// <param name="platform"></param>
    /// <param name="signature"></param>
    public void SignData(string data, Action<bool, string, string, string> callback = null, PlatformKind platform = PlatformKind.Phantasma, SignatureKind signature = SignatureKind.Ed25519)
    {
        if (!Enabled)
        {
            callback?.Invoke(false, "Not logged in", "", "");
            return;
        }
        if (data == null)
        {
            callback?.Invoke(false, "Invalid data", "", "");
            return;
        }
        if (data.Length >= 1024)
        {
            callback?.Invoke(false, "Data too big", "", "");
            return;
        }

        var dataConverted = Base16.Encode(Encoding.UTF8.GetBytes(data));

        SendLinkRequest($"signData/{dataConverted}/{signature}/{platform}", (result) =>
        {
            var success = GetValueOrDefault<bool>(result, "success");
            if (success)
            {
                var random = GetValueOrDefault<string>(result, "random");
                var signedData = GetValueOrDefault<string>(result, "signature");
                callback?.Invoke(true, signedData, random, dataConverted);
            }
            else
            {
                var msg = GetValueOrDefault<string>(result, "message");
                callback?.Invoke(false, "Failed to sign data: " + msg, "", "");
            }
        });
    }

    /// <summary>
    /// Reset WebSocket Connection
    /// </summary>
    public async void resetWebSocket()
    {
        if (websocket != null)
        {
            this.Ready = false;
            this.IsLogged = false;
            this.Enabled = false;
            await websocket.Close();
            websocket = null;
        }
    }

    public enum PlatformKind
    {
        None = 0x0,
        Phantasma = 0x1,
        Neo = 0x2,
        Ethereum = 0x4,
        BSC = 0x8,
    }
    #endregion
}
