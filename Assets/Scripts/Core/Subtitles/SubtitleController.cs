using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleController : MonoBehaviour
{
    [Header("References")]
    public RectTransform rectTransform;
    public TMP_Text text;
    public Image bg;

    private SubtitleManager subMan;
    private ManagedAudioSource sourceAudMan;
    private Transform soundTran;
    private Transform camTran;

    private float duration;
    private bool hidden;
    private bool hasPosition;

    private Vector3 anchoredPos;
    private Vector3 localScale;
    private float radius = 115f;

    public void Initialize(string content, ManagedAudioSource source, Transform soundTransform, SubtitleManager manager)
    {
        text.text = content;
        text.color = source.subtitleColor;

        sourceAudMan = source;
        soundTran = soundTransform;
        subMan = manager;
        duration = source.subDuration;

        if (Camera.main != null)
            camTran = Camera.main.transform;

        hasPosition = source.positional && soundTran != null && camTran != null;

        if (hasPosition)
            PositionDirectional();
        else
            PositionBottom();

        StartCoroutine(Die());
    }

    private void Update()
    {
        if (sourceAudMan == null)
        {
            Destroy(gameObject);
            return;
        }

        bool shouldHide = !SubtitleManager.Instance.subtitlesEnabled
                       || !sourceAudMan.gameObject.activeInHierarchy
                       || AudioListener.pause;

        Hide(shouldHide);

        if (hasPosition)
            PositionDirectional();
        else
            PositionBottom();
    }

    private void PositionDirectional()
    {
        if (soundTran == null || camTran == null) return;

        float angle = Mathf.Atan2(
            camTran.position.z - soundTran.position.z,
            camTran.position.x - soundTran.position.x
        ) * Mathf.Rad2Deg + camTran.eulerAngles.y + 180f;

        float rad = angle * Mathf.Deg2Rad;
        anchoredPos.x = Mathf.Cos(rad) * radius;
        anchoredPos.y = Mathf.Sin(rad) * radius;
        anchoredPos.z = 0f;
        rectTransform.anchoredPosition = anchoredPos;

        float scale = sourceAudMan.GetSubtitleScale(camTran);
        localScale.x = scale;
        localScale.y = scale;
        localScale.z = 1f;
        rectTransform.localScale = localScale;
    }

    private void PositionBottom()
    {
        anchoredPos.x = 0f;
        anchoredPos.y = -100f;
        anchoredPos.z = 0f;
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.localScale = Vector3.one;
    }

    private void Hide(bool hide)
    {
        if (hide == hidden) return;
        hidden = hide;
        text.enabled = !hide;
        bg.enabled = !hide;
    }

    private IEnumerator Die()
    {
        while (duration > 0f)
        {
            duration -= Time.deltaTime;
            yield return null;
        }
        subMan.Remove(this);
        Destroy(gameObject);
    }
}