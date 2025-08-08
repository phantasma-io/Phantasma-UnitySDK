using UnityEngine;
using UnityEngine.UI;

public class CoreExampleUI : MonoBehaviour
{
    public Button btnExample01;
    public Button btnExample02;
    public Button btnExample03;
    public Button btnExample04;
    public Button btnExample05;
    public Button btnExample06;
    public Button btnExample07;
    public Button btnExample08;
    public Button btnExample09;
    public Button btnExample10;

    private Example01_GenerateKey example01GameObject;
    private Example02_PublicKeyFromPrivate example02GameObject;
    private Example03_GetAddressBalances example03GameObject;
    private Example04_GetAddressTokenBalance example04GameObject;
    private Example05_SendToken example05GameObject;
    private Example06_CheckTransactionState example06GameObject;
    private Example07_StakeSoul example07GameObject;
    private Example08_UnstakeSoul example08GameObject;
    private Example09_ClaimKcal example09GameObject;
    private Example10_WaitIncomingTx_ReadBlocks example10GameObject;

    private void Start()
    {
        example01GameObject = gameObject.AddComponent<Example01_GenerateKey>();
        example02GameObject = gameObject.AddComponent<Example02_PublicKeyFromPrivate>();
        example03GameObject = gameObject.AddComponent<Example03_GetAddressBalances>();
        example04GameObject = gameObject.AddComponent<Example04_GetAddressTokenBalance>();
        example05GameObject = gameObject.AddComponent<Example05_SendToken>();
        example06GameObject = gameObject.AddComponent<Example06_CheckTransactionState>();
        example07GameObject = gameObject.AddComponent<Example07_StakeSoul>();
        example08GameObject = gameObject.AddComponent<Example08_UnstakeSoul>();
        example09GameObject = gameObject.AddComponent<Example09_ClaimKcal>();
        example10GameObject = gameObject.AddComponent<Example10_WaitIncomingTx_ReadBlocks>();

        btnExample01.onClick.AddListener(example01GameObject.Run);
        btnExample02.onClick.AddListener(example02GameObject.Run);
        btnExample03.onClick.AddListener(example03GameObject.Run);
        btnExample04.onClick.AddListener(example04GameObject.Run);
        btnExample05.onClick.AddListener(example05GameObject.Run);
        btnExample06.onClick.AddListener(example06GameObject.Run);
        btnExample07.onClick.AddListener(example07GameObject.Run);
        btnExample08.onClick.AddListener(example08GameObject.Run);
        btnExample09.onClick.AddListener(example09GameObject.Run);
        btnExample10.onClick.AddListener(example10GameObject.Run);
    }
}
