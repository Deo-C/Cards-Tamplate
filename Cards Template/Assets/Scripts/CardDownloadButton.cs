using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Kartı indirme butonu
/// Ekranda bulunan mevcut kartı otomatik olarak tespit eder ve indirir.
/// </summary>
public class CardDownloadButton : MonoBehaviour
{
    [SerializeField] private Button downloadButton;
    [SerializeField] private string downloadFileName = "Card_";

    private DragCard currentCard; // Ekrandaki mevcut kart

    private void Start()
    {
        if (downloadButton == null)
        {
            downloadButton = GetComponent<Button>();
        }

        if (downloadButton != null)
        {
            downloadButton.onClick.AddListener(OnDownloadButtonClicked);
        }

        // Sahnede aktif kartı bul (giriş animasyonu yaparken ekrana gelecek)
        UpdateCurrentCard();
    }

    private void Update()
    {
        // Her frame'de mevcut kartı güncelle (kartlar değişirken)
        UpdateCurrentCard();
    }

    /// <summary>
    /// Ekrandaki aktif DragCard'ı otomatik olarak tespit et
    /// </summary>
    private void UpdateCurrentCard()
    {
        DragCard[] allCards = FindObjectsByType<DragCard>(FindObjectsSortMode.None);
        
        if (allCards.Length > 0)
        {
            // En son spawn edilen kartı al (sonuncusu aktif kart)
            currentCard = allCards[allCards.Length - 1];
        }
        else
        {
            currentCard = null;
        }
    }

    /// <summary>
    /// İndirme butonuna tıklandığında çalışır
    /// </summary>
    private void OnDownloadButtonClicked()
    {
        if (currentCard == null)
        {
            Debug.LogError("[CardDownloadButton] Ekranda kart bulunamadı!");
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.ShowError("Kart bulunamadı!");
            else
                Debug.LogWarning("[CardDownloadButton] NotificationManager yok; bildirim gösterilemedi.");
            return;
        }

        // Mevcut kartın Image bileşenini al
        Image cardImage = currentCard.GetComponent<Image>();
        if (cardImage == null)
        {
            Debug.LogError("[CardDownloadButton] Kart Image bileşeni bulunamadı!");
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.ShowError("Kart görseli bulunamadı!");
            else
                Debug.LogWarning("[CardDownloadButton] NotificationManager yok; bildirim gösterilemedi.");
            return;
        }

        // Kartı indir
        DownloadCard(cardImage);
    }

    /// <summary>
    /// Kartı indir ve dosyaya kaydet
    /// </summary>
    private void DownloadCard(Image cardImage)
    {
        try
        {
            // Kartın RectTransform'unu al
            RectTransform cardRect = cardImage.GetComponent<RectTransform>();
            if (cardRect == null)
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowError("Kart indirilemedi!");
                else
                    Debug.LogWarning("[CardDownloadButton] NotificationManager yok; bildirim gösterilemedi.");
                return;
            }

            // Screenshot al
            Texture2D cardTexture = CaptureRectTransformToTexture(cardRect);
            if (cardTexture == null)
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowError("Ekran görüntüsü alınamadı!");
                else
                    Debug.LogWarning("[CardDownloadButton] NotificationManager yok; bildirim gösterilemedi.");
                return;
            }

            // PNG olarak kaydet
            byte[] pngData = cardTexture.EncodeToPNG();
            string fileName = downloadFileName + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);

            System.IO.File.WriteAllBytes(filePath, pngData);
            Destroy(cardTexture);

            Debug.Log($"[CardDownloadButton] Kart indirildi: {filePath}");
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.ShowSuccess($" Kart kaydedildi ");
            else
                Debug.LogWarning("[CardDownloadButton] NotificationManager yok; başarılı bildirim gösterilemedi.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CardDownloadButton] İndirme hatası: {ex.Message}");
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.ShowError("İndirme başarısız oldu!");
            else
                Debug.LogWarning("[CardDownloadButton] NotificationManager yok; hata bildirimi gösterilemedi.");
        }

        #if UNITY_ANDROID && !UNITY_EDITOR
        AddImageToGallery(filePath);
        #endif
    }

    private void AddImageToGallery(string filePath)
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
            {
                mediaScanner.CallStatic(
                    "scanFile",
                    activity,
                    new string[] { filePath },
                    null,
                    null
                );
            }

            Debug.Log("[Gallery] Görsel galeriye eklendi.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Gallery] Galeriye ekleme hatası: " + e.Message);
        }
    }

    /// <summary>
    /// RectTransform'u texture olarak yakala
    /// </summary>
    private Texture2D CaptureRectTransformToTexture(RectTransform rect)
    {
        // Rect boyutlarını al
        Vector2 size = rect.rect.size;
        int width = (int)size.x;
        int height = (int)size.y;

        if (width <= 0 || height <= 0)
            return null;

        // RenderTexture oluştur
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        Graphics.SetRenderTarget(renderTexture);
        GL.Clear(true, true, Color.clear);

        // Canvas'ı render et
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.worldCamera != null)
        {
            canvas.worldCamera.targetTexture = renderTexture;
            canvas.worldCamera.Render();
            canvas.worldCamera.targetTexture = null;
        }

        // RenderTexture'ı Texture2D'ye dönüştür
        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        RenderTexture.active = null;
        renderTexture.Release();
        Destroy(renderTexture);

        return texture;
    }
}
