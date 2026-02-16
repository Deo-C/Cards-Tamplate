using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Bildirim sistemi yöneticisi (Singleton Pattern)
/// Toast notification prefab'larını spawn eder ve yönetir.
/// </summary>
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private GameObject toastNotificationPrefab;
    [SerializeField] private Transform notificationContainer; // Canvas içinde bildirim konumu
    [SerializeField] private int maxNotificationsOnScreen = 3;
    [SerializeField] private float verticalSpacing = 10f;
    [Header("Renk Ayarları")]
    [Tooltip("Başarı (success) bildirimi arka plan rengi (HTML hex, örn. #2AA24A)")]
    [SerializeField] private string successColorHex = "#287a3e";

    private Queue<GameObject> activeNotifications = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Eğer container belirtilmemişse, Canvas'i bul ve otomatik bir notification container oluştur
        if (notificationContainer == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform existing = canvas.transform.Find("NotificationContainer");
                if (existing != null)
                {
                    notificationContainer = existing;
                }
                else
                {
                    GameObject go = new GameObject("NotificationContainer", typeof(RectTransform));
                    go.transform.SetParent(canvas.transform, false);
                    RectTransform r = go.GetComponent<RectTransform>();
                    r.anchorMin = new Vector2(0.5f, 1f);
                    r.anchorMax = new Vector2(0.5f, 1f);
                    r.pivot = new Vector2(0.5f, 1f);
                    r.anchoredPosition = new Vector2(0f, -20f);

                    UnityEngine.UI.VerticalLayoutGroup v = go.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                    v.spacing = verticalSpacing;
                    v.childAlignment = TextAnchor.UpperCenter;
                    v.childControlHeight = true;
                    v.childControlWidth = false;

                    UnityEngine.UI.ContentSizeFitter csf = go.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                    csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

                    notificationContainer = go.transform;
                }
            }
        }
    }

    /// <summary>
    /// Bildirim göster
    /// </summary>
    public void ShowNotification(string message, float duration = 3f, Color? backgroundColor = null)
    {
        if (toastNotificationPrefab == null)
        {
            Debug.LogError("[NotificationManager] Toast Notification Prefab atanmadı!");
            return;
        }

        if (notificationContainer == null)
        {
            Debug.LogError("[NotificationManager] Notification Container atanmadı!");
            return;
        }

        // Prefab'ı spawn et (local transform korunarak)
        GameObject notificationObj = Instantiate(toastNotificationPrefab, notificationContainer, false);
        // Yeni bildirim en üstte gözüksün
        notificationObj.transform.SetAsFirstSibling();

        Debug.Log($"[NotificationManager] Showing notification: {message}");
        ToastNotification notification = notificationObj.GetComponent<ToastNotification>();

        if (notification != null)
        {
            notification.Initialize(message, duration, backgroundColor);
        }

        activeNotifications.Enqueue(notificationObj);

        // Maksimum bildirim sayısını aşarsak en eskisini sil
        if (activeNotifications.Count > maxNotificationsOnScreen)
        {
            GameObject oldestNotification = activeNotifications.Dequeue();
            Destroy(oldestNotification);
        }

        // Bildirim yok olduğunda kuyruktan çıkar
        StartCoroutine(RemoveNotificationAfterDuration(notificationObj, duration));
    }

    /// <summary>
    /// Başarı bildirim göster (yeşil)
    /// </summary>
    public void ShowSuccess(string message, float duration = 3f)
    {
        Color parsed;
        if (ColorUtility.TryParseHtmlString(successColorHex, out parsed))
        {
            parsed.a = 0.9f;
            ShowNotification(message, duration, parsed);
        }
        else
        {
            ShowNotification(message, duration, new Color(0.2f, 0.8f, 0.2f, 0.9f)); // fallback
        }
    }

    /// <summary>
    /// Hata bildirim göster (kırmızı)
    /// </summary>
    public void ShowError(string message, float duration = 3f)
    {
        ShowNotification(message, duration, new Color(0.9f, 0.2f, 0.2f, 0.9f)); // Kırmızı
    }

    /// <summary>
    /// Bilgi bildirim göster (mavi)
    /// </summary>
    public void ShowInfo(string message, float duration = 3f)
    {
        ShowNotification(message, duration, new Color(0.2f, 0.6f, 0.9f, 0.9f)); // Mavi
    }

    /// <summary>
    /// Uyarı bildirim göster (turuncu)
    /// </summary>
    public void ShowWarning(string message, float duration = 3f)
    {
        ShowNotification(message, duration, new Color(1f, 0.6f, 0.2f, 0.9f)); // Turuncu
    }

    private System.Collections.IEnumerator RemoveNotificationAfterDuration(GameObject notification, float duration)
    {
        yield return new WaitForSeconds(duration + 0.5f);

        if (notification != null)
            Destroy(notification);
    }
}
