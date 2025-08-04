using Newtonsoft.Json;
using UnityEngine;

public class Example03_GetBalance : MonoBehaviour
{
    public void Run()
    {
        var manager = FindObjectOfType<CoreExampleManager>();
        var api = manager.phantasmaAPI;

        var address = manager.TestAddress;

        StartCoroutine(api.GetAccount(address, (accountResult) =>
            {
                var json = JsonConvert.SerializeObject(accountResult, Formatting.Indented);
                Debug.Log($"[Balance] balances for {address}: {json}");
            }
        ));
    }
}
