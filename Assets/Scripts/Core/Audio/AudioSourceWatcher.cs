using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AudioSourceWatcher
{
    static AudioSourceWatcher()
    {
        ObjectFactory.componentWasAdded += OnComponentAdded;
    }

    private static void OnComponentAdded(Component component)
    {
        if (component is AudioSource audioSource)
        {
            var go = audioSource.gameObject;

            if (go.GetComponent<ManagedAudioSource>() == null)
            {
                Undo.AddComponent<ManagedAudioSource>(go);
            }
        }
    }
}
