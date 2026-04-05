using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotebookScript : MonoBehaviour, IInteractable
{
    [SerializeField] Sprite[] sprites = new Sprite[0];
    [SerializeField] SpriteRenderer notebookSprite;
    public float InteractionDistance => 10f;
    public GameController gc;

    public bool CanInteract(GameObject interactor)
    {
        return true;
    }

    public void Interact(GameObject interactor)
    {
        gameObject.SetActive(false);
        gc.CollectNotebook();
    }

    private void Start()
    {
        notebookSprite = GetComponentInChildren<SpriteRenderer>();
        notebookSprite.sprite = sprites[Random.Range(0, sprites.Length)];
    }
}