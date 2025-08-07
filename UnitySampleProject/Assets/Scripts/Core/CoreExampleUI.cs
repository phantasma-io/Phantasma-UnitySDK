using UnityEngine;
using UnityEngine.UI;

public class CoreExampleUI : MonoBehaviour
{
    public Button btnExample01;
    public Button btnExample02;
    public Button btnExample03;
    public Button btnExample04;
    public Button btnExample05;

    private Example01_GenerateKey example01GameObject;
    private Example02_PublicKeyFromPrivate example02GameObject;
    private Example03_GetAddressBalances example03GameObject;
    private Example04_GetAddressTokenBalance example04GameObject;
    private Example05_SendToken example05GameObject;

    private void Start()
    {
        example01GameObject = gameObject.AddComponent<Example01_GenerateKey>();
        example02GameObject = gameObject.AddComponent<Example02_PublicKeyFromPrivate>();
        example03GameObject = gameObject.AddComponent<Example03_GetAddressBalances>();
        example04GameObject = gameObject.AddComponent<Example04_GetAddressTokenBalance>();
        example05GameObject = gameObject.AddComponent<Example05_SendToken>();

        btnExample01.onClick.AddListener(example01GameObject.Run);
        btnExample02.onClick.AddListener(example02GameObject.Run);
        btnExample03.onClick.AddListener(example03GameObject.Run);
        btnExample04.onClick.AddListener(example04GameObject.Run);
        btnExample05.onClick.AddListener(example05GameObject.Run);
    }
}
