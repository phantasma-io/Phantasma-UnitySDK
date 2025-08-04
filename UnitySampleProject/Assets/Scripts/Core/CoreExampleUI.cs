using UnityEngine;
using UnityEngine.UI;

public class CoreExampleUI : MonoBehaviour
{
    public Button btnExample01;
    public Button btnExample02;
    public Button btnExample03;

    private Example01_GenerateKey example01GameObject;
    private Example02_PublicKeyFromPrivate example02GameObject;
    private Example03_GetBalance example03GameObject;

    private void Start()
    {
        example01GameObject = gameObject.AddComponent<Example01_GenerateKey>();
        example02GameObject = gameObject.AddComponent<Example02_PublicKeyFromPrivate>();
        example03GameObject = gameObject.AddComponent<Example03_GetBalance>();

        btnExample01.onClick.AddListener(example01GameObject.Run);
        btnExample02.onClick.AddListener(example02GameObject.Run);
        btnExample03.onClick.AddListener(example03GameObject.Run);
    }
}
