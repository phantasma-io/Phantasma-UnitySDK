using PhantasmaPhoenix.Cryptography;
using UnityEngine;

public class Example01_GenerateKey : MonoBehaviour
{
    public void Run()
    {
        var manager = FindObjectOfType<CoreExampleManager>();
        var key = PhantasmaKeys.Generate();
        manager.SetKey(key);
        Debug.Log($"[GenerateKey] Address: {key.Address.Text}, HEX: {Base16.Encode(key.PrivateKey)}, WIF: {key.ToWIF()}");
    }
}
