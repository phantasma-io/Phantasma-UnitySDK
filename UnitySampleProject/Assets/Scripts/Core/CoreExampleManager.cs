using UnityEngine;
using PhantasmaPhoenix.Unity.Core;
using PhantasmaPhoenix.Cryptography;

public class CoreExampleManager : MonoBehaviour
{
    [Header("Config")]
    public string RpcUrl = "https://testnet.phantasma.info/rpc";
    public string TokenSymbol = "SOUL";
    public string TestAddress = "P2K..."; // Set in Inspector

    [Header("State")]
    public string PrivateKeyHEX; // Optional: prefilled or generated


    [Header("State")]
    public string PrivateKeyWIF; // Optional: prefilled or generated

    [HideInInspector] public PhantasmaKeys keys;
    [HideInInspector] public PhantasmaAPI phantasmaAPI;

    private void Awake()
    {
        phantasmaAPI = new PhantasmaAPI(RpcUrl);
        if (!string.IsNullOrEmpty(PrivateKeyWIF))
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
