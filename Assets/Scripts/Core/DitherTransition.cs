using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/* call it like this
 * DitherTransition.Instance.Transition(() =>
 * {
 *     newMenu.SetActive(true);
 *     cam.transform.position = newPosition;
 *     cam.transform.rotation = newRotation;
 * });
 * you need to call it within a function in another script ;p
 */
public class DitherTransition : MonoBehaviour
{
    public static DitherTransition Instance { get; private set; }

    [Header("UI")]
    public Image ditherMask;
    public RawImage snapshotOverlay;
    [Header("Dither Frames")]
    public Sprite[] ditherFrames;
    public float frameDuration = 0.05f;
    public float pixelsPerUnitMultiplier = 1f;
    [Header("Camera")]
    public Camera mainCam;

    private bool transitionActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCam = Camera.main;
    }

    public void Transition(System.Action onMidpoint)
    {
        if (transitionActive) return;
        StartCoroutine(PlayTransition(onMidpoint));
    }

    IEnumerator PlayTransition(System.Action onMidpoint)
    {
        transitionActive = true;

        PixelCursor.Instance.Hide(true);

        // Wait for end of frame to capture screen
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        snapshotOverlay.texture = screenshot;

        // Force canvas to be visible BEFORE yielding again
        snapshotOverlay.gameObject.SetActive(true);
        ditherMask.gameObject.SetActive(true);

        // Show the FIRST dither frame (most transparent/open) and let it render
        ditherMask.sprite = ditherFrames[0];
        ditherMask.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;

        // Give Unity one frame to actually render the overlay before we do anything
        yield return null;

        // NOW fire the midpoint (scene load, teleport, etc.)
        onMidpoint?.Invoke();

        // Another frame pause so the scene change settles
        yield return null;

        // Animate dither frames forward (covered → revealed)
        for (int i = ditherFrames.Length - 1; i >= 0; i--)
        {
            ditherMask.sprite = ditherFrames[i];
            yield return new WaitForSeconds(frameDuration);
        }

        snapshotOverlay.gameObject.SetActive(false);
        ditherMask.gameObject.SetActive(false); // hide this too!
        Destroy(screenshot);
        PixelCursor.Instance.Hide(false);
        transitionActive = false;
    }
}