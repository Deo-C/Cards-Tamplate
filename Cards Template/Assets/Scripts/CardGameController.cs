using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hile kartı yapısı - ID ve prefab içerir
/// </summary>
[System.Serializable]
public class CheatCard
{
    public int cheatId;
    public GameObject cardPrefab;
}

public class CardGameController : MonoBehaviour
{
    [Header("Kart Prefab'leri")]
    public GameObject startCardPrefab;
    public List<GameObject> anaCardPrefabs = new List<GameObject>();
    public GameObject endCardPrefab;

    [Header("Özel Kartlar")]
    public GameObject optionsCardPrefab; // Options (durdurma) kartı - tek prefab
    public List<CheatCard> cheatCardPrefabs = new List<CheatCard>(); // Hile kartları - numaralı liste

    [Header("Ayarlar")]
    public RectTransform spawnPoint;

    private Stack<GameObject> cardStack = new Stack<GameObject>();
    private int currentAnaCardIndex = 0;
    private bool gameStarted = false;

    void Start()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point atanmadı!");
            return;
        }
        // Arka plan müzik başlat
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBackgroundMusic();

        // Başlangıç kartını göster
        if (startCardPrefab != null)
        {
            SpawnCardOfType(startCardPrefab, CardType.StartGame);
        }
        else
        {
            Debug.LogWarning("Başlangıç kartı prefab'i atanmadı!");
        }
    }

    private void SpawnCardOfType(GameObject prefab, CardType cardType)
    {
        if (prefab == null) return;

        GameObject cardObj = Instantiate(prefab, spawnPoint, false);
        RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localRotation = Quaternion.identity;
        }

        DragCard dragCard = cardObj.GetComponent<DragCard>();
        if (dragCard != null)
        {
            dragCard.cardType = cardType;
            // Listener'ları temizle
            dragCard.onYes.RemoveAllListeners();
            dragCard.onNo.RemoveAllListeners();
            // Yeni listener'ları ekle
            dragCard.onYes.AddListener(() => OnCardSwiped(cardType, true));
            dragCard.onNo.AddListener(() => OnCardSwiped(cardType, false));
        }

        cardStack.Push(cardObj);
    }

    private void OnCardSwiped(CardType cardType, bool isYes)
    {
        Debug.Log($"Kart kaydırıldı: {cardType} - Yön: {(isYes ? "SAĞA (Evet)" : "SOLA (Hayır)")}");

        switch (cardType)
        {
            case CardType.StartGame:
                // Başlangıç kartı sadece bir kez tetiklenmeli
                if (!gameStarted && isYes)
                {
                    gameStarted = true;
                    
                    Debug.Log("Play kartı kabul edildi - UI güncelleştirildi");
                    ShowNextAnaCard();
                }
                break;

            case CardType.AnaCard:
                ShowNextAnaCard();
                break;

            case CardType.Pause:
                Debug.Log("Durdurma kartı kaydırıldı!");
                break;

            case CardType.Cheat:
                Debug.Log("Hile kartı kullanıldı!");
                break;

            case CardType.EndGame:
                Debug.Log("Oyun bitti!");
                break;

            default:
                break;
        }
    }

    private void ShowNextAnaCard()
    {
        if (currentAnaCardIndex < anaCardPrefabs.Count)
        {
            GameObject cardPrefab = anaCardPrefabs[currentAnaCardIndex];
            SpawnCardOfType(cardPrefab, CardType.AnaCard);
            currentAnaCardIndex++;
        }
        else
        {
            // Tüm kartlar bitti
            if (endCardPrefab != null)
            {
                SpawnCardOfType(endCardPrefab, CardType.EndGame);
            }
            else
            {
                Debug.Log("Oyun bitti! Son kart prefab'i atanmadı.");
            }
        }
    }

    // Options kartı spawn etmek için
    public void SpawnOptionsCard()
    {
        Debug.Log("SpawnOptionsCard called");
        if (optionsCardPrefab != null)
        {
            SpawnCardOfType(optionsCardPrefab, CardType.Pause);
        }
        else
        {
            Debug.LogWarning("SpawnOptionsCard: Options Card Prefab atanmadı!");
        }
    }

    // Hile kartı spawn etmek için - ID'ye göre seç
    public void SpawnCheatCard(int cheatId)
    {
        Debug.Log($"SpawnCheatCard called with ID: {cheatId}");
        
        CheatCard selectedCheat = cheatCardPrefabs.Find(c => c.cheatId == cheatId);
        
        if (selectedCheat != null && selectedCheat.cardPrefab != null)
        {
            SpawnCardOfType(selectedCheat.cardPrefab, CardType.Cheat);
            Debug.Log($"Hile kartı #{cheatId} spawn edildi");
        }
        else
        {
            Debug.LogWarning($"SpawnCheatCard: Hile kartı ID #{cheatId} bulunamadı!");
        }
    }


}
