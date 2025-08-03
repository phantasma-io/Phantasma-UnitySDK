using TMPro;
using UnityEngine;

public class ExampleLogin : MonoBehaviour
{
    public Login login;

    public TMP_Text text;
    
    void Start()
    {
        login.OnLoginEvent += Login_OnLoginEvent;
    }
    
    private void Login_OnLoginEvent(string msg, bool error)
    {
        text.SetText(msg);

        if (error)
        {
            Debug.LogError(msg);
        }
        else
        {
            Debug.Log(msg);
        }
    }
}
