using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NaughtyAttributes;

public class GameController : MonoBehaviour
{
    [BoxGroup("Notebooks")]
    public int notebookMax;
    [BoxGroup("Notebooks")]
    public TextMeshProUGUI counter;
    [BoxGroup("Notebooks")]
    public AudioClip notebookPickup;
    [BoxGroup("References")]
    public CursorController cursorController;
    [BoxGroup("References")]
    public Animator notebookAnimator;
    [BoxGroup("References")]
    private ManagedAudioSource managedAudio;

    private int _notebookCount;
    public int NotebookCount => _notebookCount;
    public bool endlessMode;

    void Start()
    {
        UpdateCounter();
        cursorController.LockCursor();
        managedAudio = gameObject.GetComponentInChildren<ManagedAudioSource>();
    }

    public void CollectNotebook()
    {
        _notebookCount += 1;
        UpdateCounter();
        if (notebookAnimator != null)
        {
            notebookAnimator.Play("NotebookSpin", -1, 0f);
            managedAudio.SetClipAndPlay(notebookPickup);
        }
    }

    private void UpdateCounter()
    {
        counter.text = $"{_notebookCount}/{notebookMax}";
    }
}