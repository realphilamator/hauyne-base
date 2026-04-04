using UnityEngine;
using UnityEngine.SceneManagement;

public static class Dither
{
    // Basic transition with a callback at midpoint
    public static void Do(System.Action onMidpoint)
    {
        DitherTransition.Instance.Transition(onMidpoint);
    }

    // Load a scene by name
    public static void LoadScene(string sceneName)
    {
        DitherTransition.Instance.Transition(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    // Load a scene by index
    public static void LoadScene(int sceneIndex)
    {
        DitherTransition.Instance.Transition(() =>
        {
            SceneManager.LoadScene(sceneIndex);
        });
    }

    // Teleport a transform
    public static void Teleport(Transform target, Vector3 position, Quaternion rotation)
    {
        DitherTransition.Instance.Transition(() =>
        {
            target.position = position;
            target.rotation = rotation;
        });
    }

    // Swap active GameObjects (like switching menus)
    public static void SwapUI(GameObject hide, GameObject show)
    {
        DitherTransition.Instance.Transition(() =>
        {
            hide.SetActive(false);
            show.SetActive(true);
        });
    }

    // Teleport + swap UI at the same time
    public static void TeleportAndSwapUI(Transform target, Vector3 position, Quaternion rotation, GameObject hide, GameObject show)
    {
        DitherTransition.Instance.Transition(() =>
        {
            target.position = position;
            target.rotation = rotation;
            hide.SetActive(false);
            show.SetActive(true);
        });
    }
}