using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public Button hostButton;
    public Button joinButton;
    public Button quitButton;

    private NetworkRunner networkRunner;

    void Start()
    {
        networkRunner = FindObjectOfType<NetworkRunner>();
        if (networkRunner == null)
        {
            GameObject go = new GameObject("NetworkRunner");
            networkRunner = go.AddComponent<NetworkRunner>();
        }

        hostButton.onClick.AddListener(() => StartGame(GameMode.Shared));
        //joinButton.onClick.AddListener(() => StartGame(GameMode.Client));
        quitButton.onClick.AddListener(QuitGame);
    }

    async void StartGame(GameMode mode)
    {
        if (string.IsNullOrEmpty(playerNameInput.text))
        {
            Debug.LogWarning("Please enter a username!");
            return;
        }

        networkRunner.ProvideInput = true;

        var result = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "MyGame",
            //Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex,
            PlayerCount = 4
        });

        if (result.Ok)
        {
            Debug.Log($"Started game as {mode}");
        }
        else
        {
            Debug.LogError($"Failed to start game: {result.ErrorMessage}");
        }
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
