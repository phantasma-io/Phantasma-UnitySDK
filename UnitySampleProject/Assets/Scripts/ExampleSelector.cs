using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleSelector : MonoBehaviour
{
    public void LaunchLinkClient()
    {
        SceneManager.LoadScene("TestDesktop");
    }

    public void LaunchCore()
    {
        SceneManager.LoadScene("CoreExamples");
    }
}
