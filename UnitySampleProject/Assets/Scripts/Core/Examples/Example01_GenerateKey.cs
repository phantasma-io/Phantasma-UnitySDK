using PhantasmaPhoenix.Cryptography;
using UnityEngine;

// Unity MonoBehaviour used to demonstrate how to generate a new private/public key pair
public class Example01_GenerateKey : MonoBehaviour
{
    // Entry point of example
    public void Run()
    {
        // Get reference to the scene-wide manager that stores API config and global variables
        var manager = FindObjectOfType<CoreExampleManager>();

        // Generate a new random private key and derive corresponding address and public key
        var key = PhantasmaKeys.Generate();

        // Save the generated key to the scene-wide manager for use in other examples
        manager.SetKey(key);

        // Log key data including address, private key in HEX, and WIF format for debugging or export
        Debug.Log($"[GenerateKey] Address: {key.Address.Text}, HEX: {Base16.Encode(key.PrivateKey)}, WIF: {key.ToWIF()}");
    }
}
