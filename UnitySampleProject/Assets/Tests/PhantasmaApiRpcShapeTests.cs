using System;
using System.Collections;
using NUnit.Framework;
using PhantasmaPhoenix.RPC.Models;
using PhantasmaPhoenix.RPC.Types;
using PhantasmaPhoenix.Unity.Core;

public class PhantasmaApiRpcShapeTests
{
    private sealed class CapturingPhantasmaApi : PhantasmaAPI
    {
        public string LastMethod { get; private set; }
        public object[] LastParameters { get; private set; }
        public int LastTimeout { get; private set; }
        public int LastRetries { get; private set; }
        public object NextResult { get; set; }

        public CapturingPhantasmaApi() : base("http://127.0.0.1:1/rpc")
        {
        }

        protected override IEnumerator RpcRequest<T>(string method, Action<T> callback, Action<EPHANTASMA_SDK_ERROR_TYPE, string> errorHandlingCallback = null, int timeout = WebClient.DefaultTimeout, int retries = WebClient.DefaultRetries, params object[] parameters)
        {
            LastMethod = method;
            LastParameters = parameters ?? Array.Empty<object>();
            LastTimeout = timeout;
            LastRetries = retries;

            if (callback != null)
            {
                var result = NextResult is T typed ? typed : default;
                callback(result);
            }

            yield break;
        }
    }

    private static void RunCoroutine(IEnumerator coroutine)
    {
        while (coroutine.MoveNext())
        {
            if (coroutine.Current is IEnumerator nested)
            {
                RunCoroutine(nested);
            }
        }
    }

    private static void AssertCall(CapturingPhantasmaApi api, string expectedMethod, params object[] expectedParameters)
    {
        Assert.That(api.LastMethod, Is.EqualTo(expectedMethod));
        Assert.That(api.LastParameters, Is.EqualTo(expectedParameters));
    }

    [Test]
    public void GetAccount_WithAddressTypeAndValidationFlags_UsesExpandedSignature()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetAccount(
            "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff",
            true,
            false,
            RpcAddressType.Carbon,
            _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getAccount", "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff", true, false, RpcAddressType.Carbon);
    }

    [Test]
    public void GetAccounts_WithAddressTypeAndValidationFlags_JoinsAddressesAndUsesExpandedSignature()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetAccounts(
            new[]
            {
                "addr-1",
                "addr-2"
            },
            true,
            false,
            RpcAddressType.Carbon,
            _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getAccounts", "addr-1,addr-2", true, false, RpcAddressType.Carbon);
    }

    [Test]
    public void GetAccountFungibleTokens_WithAddressType_UsesCursorPayload()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetAccountFungibleTokens(
            "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff",
            "SOUL",
            0,
            25,
            "cursor-1",
            false,
            RpcAddressType.Carbon,
            _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getAccountFungibleTokens", "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff", "SOUL", 0UL, 25U, "cursor-1", false, RpcAddressType.Carbon);
    }

    [Test]
    public void GetAccountNfts_WithAddressType_UsesCursorPayload()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetAccountNFTs(
            "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff",
            "CROWN",
            17,
            3,
            25,
            "cursor-2",
            true,
            false,
            RpcAddressType.Carbon,
            _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getAccountNFTs", "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff", "CROWN", 17UL, 3U, 25U, "cursor-2", true, false, RpcAddressType.Carbon);
    }

    [Test]
    public void GetAccountOwnedTokens_WithAddressType_UsesCursorPayload()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetAccountOwnedTokens(
            "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff",
            "CROWN",
            17,
            25,
            "cursor-3",
            false,
            RpcAddressType.Carbon,
            _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getAccountOwnedTokens", "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff", "CROWN", 17UL, 25U, "cursor-3", false, RpcAddressType.Carbon);
    }

    [Test]
    public void GetAccountOwnedTokenSeries_WithAddressType_UsesCursorPayload()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetAccountOwnedTokenSeries(
            "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff",
            "CROWN",
            17,
            25,
            "cursor-4",
            false,
            RpcAddressType.Carbon,
            _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getAccountOwnedTokenSeries", "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff", "CROWN", 17UL, 25U, "cursor-4", false, RpcAddressType.Carbon);
    }

    [Test]
    public void GetContractByAddress_UsesChainAndAddress()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetContractByAddress("main", "P2KcontractAddress", _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getContractByAddress", "main", "P2KcontractAddress");
    }

    [Test]
    public void GetToken_WithExtendedAndCarbonId_UsesExpandedSignature()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetToken("TESTN", true, 111, _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getToken", "TESTN", true, 111UL);
    }

    [Test]
    public void GetTokens_WithExtendedOwnerAndAddressType_UsesCarbonPayload()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetTokens(
            true,
            "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff",
            RpcAddressType.Carbon,
            _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getTokens", true, "00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff", RpcAddressType.Carbon);
    }

    [Test]
    public void GetTokenSeries_UsesCarbonCursorPayload()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetTokenSeries("CROWN", 17, 25, "cursor-5", _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getTokenSeries", "CROWN", 17UL, 25U, "cursor-5");
    }

    [Test]
    public void GetTokenSeriesById_UsesBothSeriesIdentifiers()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetTokenSeriesById("CROWN", 17, "series-alpha", 3, _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getTokenSeriesById", "CROWN", 17UL, "series-alpha", 3U);
    }

    [Test]
    public void GetTokenNfts_UsesExtendedCursorPayload()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetTokenNFTs(17, 3, 25, "cursor-6", true, "series-beta", _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getTokenNFTs", 17UL, 3U, 25U, "cursor-6", true, "series-beta");
    }

    [Test]
    public void GetNfts_JoinsIdsBeforeCallingCarbonRpc()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetNFTs("CROWN", new[] { "1", "2", "3" }, true, _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getNFTs", "CROWN", "1,2,3", true);
    }

    [Test]
    public void GetTokenBalance_WithAddressType_UsesExpandedSignature()
    {
        var api = new CapturingPhantasmaApi();
        var callbackInvoked = false;

        RunCoroutine(api.GetTokenBalance("P2K...", "SOUL", "main", false, RpcAddressType.Carbon, _ => callbackInvoked = true));

        Assert.That(callbackInvoked, Is.True);
        AssertCall(api, "getTokenBalance", "P2K...", "SOUL", "main", false, RpcAddressType.Carbon);
    }

    [Test]
    public void GetBlockTransactionCountByHash_WithChainParameter_UsesUpdatedSignature()
    {
        var api = new CapturingPhantasmaApi
        {
            NextResult = "7"
        };
        var callbackResult = -1;

        RunCoroutine(api.GetBlockTransactionCountByHash("main", "ABCDEF0123456789", result => callbackResult = result));

        Assert.That(callbackResult, Is.EqualTo(7));
        AssertCall(api, "getBlockTransactionCountByHash", "main", "ABCDEF0123456789");
    }

    [Test]
    public void GetBlockTransactionCountByHash_LegacySignatureStillTargetsRootChain()
    {
        var api = new CapturingPhantasmaApi
        {
            NextResult = "3"
        };
        var callbackResult = -1;

        RunCoroutine(api.GetBlockTransactionCountByHash("ABCDEF0123456789", result => callbackResult = result));

        Assert.That(callbackResult, Is.EqualTo(3));
        AssertCall(api, "getBlockTransactionCountByHash", "main", "ABCDEF0123456789");
    }
}
