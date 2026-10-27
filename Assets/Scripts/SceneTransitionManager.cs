
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Pub Loading Scene")]
    [SerializeField] private string loadingSceneName = "Loading";
    [SerializeField] private float minimumLoadingTime = 1f;

    private string citySceneName;
    private Vector3 cityReturnPosition;

    private bool insideInterior;
    private bool transitioning;
    private bool startingNewRun;

    public bool IsTransitioning => transitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }

    public void EnterInterior(string sceneName, Vector3 returnPosition)
    {
        if (transitioning || insideInterior)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Interior scene name is missing!");
            return;
        }

        citySceneName = SceneManager.GetActiveScene().name;
        cityReturnPosition = returnPosition;

        RunConsciousnessTimer.Pause(this);
        StartCoroutine(TransitionTo(sceneName, true));
    }

    public void ExitInterior()
    {
        if (transitioning || !insideInterior)
            return;

        if (string.IsNullOrWhiteSpace(citySceneName))
        {
            Debug.LogError("City return scene is missing!");
            return;
        }

        RunConsciousnessTimer.Pause(this);
        StartCoroutine(TransitionTo(citySceneName, false));
    }

    public void StartRunFromPub(string destinationCityScene)
    {
        if (transitioning)
            return;

        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager missing!");
            return;
        }

        if (string.IsNullOrWhiteSpace(destinationCityScene))
        {
            Debug.LogError("City scene name missing!");
            return;
        }

        startingNewRun = true;
        insideInterior = false;
        citySceneName = destinationCityScene;

        RunManager.Instance.BeginNewRun();

        RunConsciousnessTimer.Pause(this);
        StartCoroutine(PubToCityTransition(destinationCityScene));
    }

    private IEnumerator PubToCityTransition(string destinationCityScene)
    {
        transitioning = true;

        // Fade the pub to black.
        yield return Fade(1f);

        // Show the Loading scene.
        if (!string.IsNullOrWhiteSpace(loadingSceneName))
        {
            yield return LoadScene(loadingSceneName);

            // Reveal the Loading scene.
            yield return Fade(0f);

            // Keep it visible briefly.
            yield return new WaitForSecondsRealtime(
                Mathf.Max(0f, minimumLoadingTime)
            );

            // Fade back to black before loading City.
            yield return Fade(1f);
        }

        yield return LoadScene(destinationCityScene);

        PlacePlayerAtCityStart();

        startingNewRun = false;
        insideInterior = false;

        yield return null;
        yield return Fade(0f);

        transitioning = false;
        RunConsciousnessTimer.Resume(this);
    }

    private IEnumerator TransitionTo(string sceneName, bool entering)
    {
        transitioning = true;

        yield return Fade(1f);

        yield return LoadScene(sceneName);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            if (entering)
            {
                InteriorSpawnPoint spawn =
                    FindFirstObjectByType<InteriorSpawnPoint>();

                if (spawn != null)
                    PlacePlayer(player, spawn.transform.position);
                else
                    Debug.LogWarning("InteriorSpawnPoint not found!");
            }
            else
            {
                if (startingNewRun)
                {
                    CityStartPoint spawn =
                        FindFirstObjectByType<CityStartPoint>();

                    if (spawn != null)
                        PlacePlayer(player, spawn.transform.position);

                    startingNewRun = false;
                }
                else
                {
                    PlacePlayer(player, cityReturnPosition);
                }
            }
        }
        else
        {
            Debug.LogWarning("No Player found in destination scene!");
        }

        insideInterior = entering;

        yield return null;
        yield return Fade(0f);

        transitioning = false;
        RunConsciousnessTimer.Resume(this);
    }

    private IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Single
            );

        if (operation == null)
        {
            Debug.LogError("Failed to load scene: " + sceneName);
            yield break;
        }

        yield return operation;
    }

    private void PlacePlayerAtCityStart()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Player missing in City!");
            return;
        }

        CityStartPoint spawn =
            FindFirstObjectByType<CityStartPoint>();

        if (spawn != null)
            PlacePlayer(player, spawn.transform.position);
        else
            Debug.LogWarning("CityStartPoint not found!");
    }

    private void PlacePlayer(GameObject player, Vector3 position)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.position = new Vector2(position.x, position.y);
            rb.linearVelocity = Vector2.zero;
        }

        player.transform.position = position;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.blocksRaycasts = true;

        float startAlpha = fadeCanvasGroup.alpha;
        float duration = Mathf.Max(0.01f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            fadeCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f)
            fadeCanvasGroup.blocksRaycasts = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            RunConsciousnessTimer.Resume(this);
            Instance = null;
        }
    }
}
