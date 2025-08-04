using PhantasmaPhoenix.Cryptography;
using UnityEngine;

public class Example02_PublicKeyFromPrivate : MonoBehaviour
{
    public void Run()
    {
        var manager = FindObjectOfType<CoreExampleManager>();
        var keys = manager.keys;
        if (keys == null)
        {
            Debug.LogWarning("[PublicKey] No keys loaded");
            return;
        }

        Debug.Log($"[PublicKey] Address: {keys.Address.Text}, Public Key: {Base16.Encode(keys.PublicKey)}");
    }
}
