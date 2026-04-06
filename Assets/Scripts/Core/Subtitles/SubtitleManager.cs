using System.Collections.Generic;
using UnityEngine;

public class SubtitleManager : Singleton<SubtitleManager>
{
    public GameObject subtitlePrefab;
    public Canvas subtitleCanvas;
    public bool subtitlesEnabled = true;

    private List<SubtitleController> active = new List<SubtitleController>();
    private Dictionary<ManagedAudioSource, SubtitleController> sourceMap = new Dictionary<ManagedAudioSource, SubtitleController>();

    public static new SubtitleManager Instance
    {
        get
        {
            if (Exist) return (SubtitleManager)(object)Singleton<SubtitleManager>.Instance;
            GameObject prefab = Resources.Load<GameObject>("SubtitleManager");
            if (prefab == null)
            {
                Debug.LogError("SubtitleManager prefab not found in Resources!");
                return null;
            }
            GameObject go = Instantiate(prefab);
            go.name = "SubtitleManager";
            DontDestroyOnLoad(go);
            return go.GetComponent<SubtitleManager>();
        }
    }

    public void SpawnSubtitle(ManagedAudioSource source, Transform soundTransform, string key)
    {
        if (!subtitlesEnabled) return;
        if (string.IsNullOrEmpty(key)) return;

        if (sourceMap.TryGetValue(source, out SubtitleController existing) && existing != null)
        {
            active.Remove(existing);
            Destroy(existing.gameObject);
        }

        string content = LocalizationManager.Instance.Get(key);
        GameObject go = Instantiate(subtitlePrefab, subtitleCanvas.transform);
        SubtitleController sub = go.GetComponent<SubtitleController>();
        sub.Initialize(content, source, soundTransform, this);

        active.Add(sub);
        sourceMap[source] = sub;
    }

    public void Remove(SubtitleController sub)
    {
        active.Remove(sub);
        ManagedAudioSource toRemove = null;
        foreach (var kvp in sourceMap)
        {
            if (kvp.Value == sub)
            {
                toRemove = kvp.Key;
                break;
            }
        }
        if (toRemove != null)
            sourceMap.Remove(toRemove);
    }
}