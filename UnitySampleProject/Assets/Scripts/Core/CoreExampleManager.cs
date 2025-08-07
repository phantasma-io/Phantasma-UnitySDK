using UnityEngine;
using PhantasmaPhoenix.Unity.Core;
using PhantasmaPhoenix.Cryptography;

public class CoreExampleManager : MonoBehaviour
{
    [Header("Config")]
    public string RpcUrl = "https://testnet.phantasma.info/rpc";
    public string Nexus = "testnet"; // Use "mainnet" for mainnet
    public string TokenSymbol = "SOUL";
    public float TokenAmount = 0.01f;
    public string TestAddress = "P2K..."; // Set in Inspector
    public string TransactionHash = "9749DCDAA37A53397AFB4EA30547C40BBF6ACC5B89B0234737C7A5AF71B0D4F2";

    [Header("State")]
    public string PrivateKeyHEX; // Optional: prefilled or generated


    [Header("State")]
    public string PrivateKeyWIF; // Optional: prefilled or generated

    [HideInInspector] public PhantasmaKeys keys;
    [HideInInspector] public PhantasmaAPI phantasmaAPI;

    private void Awake()
    {
        phantasmaAPI = new PhantasmaAPI(RpcUrl);
        if (!string.IsNullOrEmpty(PrivateKeyWIF) || !string.IsNullOrEmpty(PrivateKeyHEX))
        {
            if (!string.IsNullOrEmpty(PrivateKeyWIF))
            {
                keys = PhantasmaKeys.FromWIF(PrivateKeyWIF);
                Debug.Log("Found private key in form of WIF");
            }
            else
            {
                keys = new PhantasmaKeys(Base16.Decode(PrivateKeyHEX));
                Debug.Log("Found private key endoded in HEX (Base16)");
            }
        }
    }

    public void SetKey(PhantasmaKeys keys)
    {
        this.keys = keys;
    }
}
