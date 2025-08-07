using PhantasmaPhoenix.Cryptography;
using UnityEngine;

// Unity MonoBehaviour used to demonstrate how to extract a public key from a private key
public class Example02_PublicKeyFromPrivate : MonoBehaviour
{
    // Entry point of example
    public void Run()
    {
        // Get reference to the scene-wide manager that stores API config and global variables
        var manager = FindObjectOfType<CoreExampleManager>();

        // Loaded private key - must be set in the Unity inspector before extracting public key
        var keys = manager.keys;

        // Abort if private key is missing - cannot derive public key
        if (keys == null)
        {
            Debug.LogWarning("[PublicKey] No keys loaded");
            return;
        }

        // Log the address and corresponding public key in base16 format
        Debug.Log($"[PublicKey] Address: {keys.Address.Text}, Public Key: {Base16.Encode(keys.PublicKey)}");
    }
}
