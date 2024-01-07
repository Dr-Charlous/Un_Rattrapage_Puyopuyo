using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private Scene scene;

    private void Start()
    {
        scene = SceneManager.GetActiveScene();
    }

    public void ButtonRetry()
    {
        SceneManager.LoadScene(scene.name);
    }

    public void ButtonNextLevel(SceneAsset scene)
    {
        SceneManager.LoadScene(scene.name);
    }
}
