using System.Collections;
using UnityEngine;

/// <summary>
/// UI animasyonları: Entrance (aşağıdan yukarı), Exit (momentum), Return (geri dön)
/// DragCard'taki animasyon sistemi herhangi bir UI elemanına uygulanabilir hale getirildi.
/// </summary>
public class UIAnimator : MonoBehaviour
{
    private RectTransform rt;
    private CanvasGroup canvasGroup;

    [Header("Otomatik Animasyonlar")]
    [SerializeField] private bool playEntranceOnStart = true;
    [SerializeField] private float entranceAnimationDuration = 0.6f;
    [SerializeField] private float entranceStartOffsetY = -300f;

    [Header("Manuel Animasyon Ayarları")]
    [SerializeField] private float manualAnimationDuration = 0.5f;
    [SerializeField] private Vector2 manualExitTargetPos = new Vector2(500f, 0f);
    [SerializeField] private float manualExitVelocity = 0f;
    [SerializeField] private Vector3 manualScaleTarget = Vector3.one;
    [SerializeField] private float manualFadeTarget = 0.5f;
    [SerializeField] private Vector2 manualMoveTargetPos = Vector2.zero;
    [SerializeField] private float manualRotationTarget = 0f;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Otomatik entrance animasyonu çal
        if (playEntranceOnStart)
        {
            PlayEntranceAnimation(entranceAnimationDuration, entranceStartOffsetY);
        }
    }
    public void PlayMnualEntrance()
    {
        PlayEntranceAnimation(manualAnimationDuration, entranceStartOffsetY);
    }

    // ========== İNSPECTOR'DAN ÇAĞIRILABILIR METODLAR ==========

    /// <summary>
    /// Entrance animasyonunu çal (Inspector'dan)
    /// </summary>
    [ContextMenu("Animasyon/Entrance Çal")]
    public void InspectorPlayEntrance()
    {
        PlayEntranceAnimation(entranceAnimationDuration, entranceStartOffsetY);
    }

    /// <summary>
    /// Exit animasyonunu çal (Inspector'dan, ayarlı değerlerle)
    /// </summary>
    [ContextMenu("Animasyon/Exit Çal")]
    public void InspectorPlayExit()
    {
        PlayExitAnimation(manualExitTargetPos, manualAnimationDuration, manualExitVelocity);
    }

    /// <summary>
    /// Return animasyonunu çal (Inspector'dan)
    /// </summary>
    [ContextMenu("Animasyon/Return Çal")]
    public void InspectorPlayReturn()
    {
        PlayReturnAnimation(manualAnimationDuration);
    }

    /// <summary>
    /// Rotation animasyonunu çal (Inspector'dan)
    /// </summary>
    [ContextMenu("Animasyon/Rotation Çal")]
    public void InspectorPlayRotation()
    {
        PlayRotationAnimation(manualRotationTarget, manualAnimationDuration);
    }

    /// <summary>
    /// Fade animasyonunu çal (Inspector'dan)
    /// </summary>
    [ContextMenu("Animasyon/Fade Çal")]
    public void InspectorPlayFade()
    {
        PlayFadeAnimation(manualFadeTarget, manualAnimationDuration);
    }

    /// <summary>
    /// Scale animasyonunu çal (Inspector'dan)
    /// </summary>
    [ContextMenu("Animasyon/Scale Çal")]
    public void InspectorPlayScale()
    {
        PlayScaleAnimation(manualScaleTarget, manualAnimationDuration);
    }

    /// <summary>
    /// Move animasyonunu çal (Inspector'dan)
    /// </summary>
    [ContextMenu("Animasyon/Move Çal")]
    public void InspectorPlayMove()
    {
        PlayMoveAnimation(manualMoveTargetPos, manualAnimationDuration);
    }

    /// <summary>
    /// Entrance Animasyonu: Aşağıdan yukarıya slide + fade in
    /// </summary>
    public void PlayEntranceAnimation(float duration = 0.6f, float startOffsetY = -300f)
    {
        StartCoroutine(EntranceAnimationCoroutine(duration, startOffsetY));
    }

    private IEnumerator EntranceAnimationCoroutine(float duration, float startOffsetY)
    {
        Vector2 startPos = rt.anchoredPosition;
        Vector2 bottomPos = startPos + new Vector2(0, startOffsetY);
        
        rt.anchoredPosition = bottomPos;
        canvasGroup.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutQuad(t);

            rt.anchoredPosition = Vector2.Lerp(bottomPos, startPos, eased);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);

            yield return null;
        }

        rt.anchoredPosition = startPos;
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Exit Animasyonu: Belirtilen pozisyona hızlı çıkış + fade out (momentum tabanlı)
    /// </summary>
    public void PlayExitAnimation(Vector2 targetPos, float duration = 0.6f, float velocity = 0f)
    {
        // Eğer velocity varsa, çıkış süresini hıza göre ayarla
        float adjustedDuration = Mathf.Max(0.2f, duration / (1f + Mathf.Abs(velocity) / 500f));
        StartCoroutine(ExitAnimationCoroutine(targetPos, adjustedDuration));
    }

    private IEnumerator ExitAnimationCoroutine(Vector2 targetPos, float duration)
    {
        Vector2 from = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutCubic(t);

            rt.anchoredPosition = Vector2.Lerp(from, targetPos, eased);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Return Animasyonu: Orijinal pozisyona yumuşak geri dönüş
    /// </summary>
    public void PlayReturnAnimation(float duration = 0.5f)
    {
        StartCoroutine(ReturnAnimationCoroutine(duration));
    }

    private IEnumerator ReturnAnimationCoroutine(float duration)
    {
        float t = 0f;
        Vector2 from = rt.anchoredPosition;
        Vector2 targetPos = Vector2.zero; // Origin pozisyonuna dön
        Quaternion fromRot = rt.localRotation;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = EaseOutQuad(Mathf.Clamp01(t));
            
            rt.anchoredPosition = Vector2.Lerp(from, targetPos, eased);
            rt.localRotation = Quaternion.Slerp(fromRot, Quaternion.identity, eased);

            yield return null;
        }

        rt.anchoredPosition = targetPos;
        rt.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Rotation Animasyonu: Belirtilen açıya döner
    /// </summary>
    public void PlayRotationAnimation(float targetRotation, float duration = 0.3f)
    {
        StartCoroutine(RotationAnimationCoroutine(targetRotation, duration));
    }

    private IEnumerator RotationAnimationCoroutine(float targetRotation, float duration)
    {
        Quaternion from = rt.localRotation;
        Quaternion to = Quaternion.Euler(0, 0, targetRotation);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutQuad(t);

            rt.localRotation = Quaternion.Lerp(from, to, eased);

            yield return null;
        }

        rt.localRotation = to;
    }

    /// <summary>
    /// Fade Animasyonu: Alpha değerini değiştirir
    /// </summary>
    public void PlayFadeAnimation(float targetAlpha, float duration = 0.3f)
    {
        StartCoroutine(FadeAnimationCoroutine(targetAlpha, duration));
    }

    private IEnumerator FadeAnimationCoroutine(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutQuad(t);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, eased);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    /// <summary>
    /// Scale Animasyonu: UI'ın boyutunu değiştirir
    /// </summary>
    public void PlayScaleAnimation(Vector3 targetScale, float duration = 0.3f)
    {
        StartCoroutine(ScaleAnimationCoroutine(targetScale, duration));
    }

    private IEnumerator ScaleAnimationCoroutine(Vector3 targetScale, float duration)
    {
        Vector3 startScale = rt.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutQuad(t);

            rt.localScale = Vector3.Lerp(startScale, targetScale, eased);

            yield return null;
        }

        rt.localScale = targetScale;
    }

    /// <summary>
    /// Position Animasyonu: Belirtilen konuma hareket eder
    /// </summary>
    public void PlayMoveAnimation(Vector2 targetPos, float duration = 0.3f)
    {
        StartCoroutine(MoveAnimationCoroutine(targetPos, duration));
    }

    private IEnumerator MoveAnimationCoroutine(Vector2 targetPos, float duration)
    {
        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutQuad(t);

            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased);

            yield return null;
        }

        rt.anchoredPosition = targetPos;
    }

    // Easing Functions
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}
