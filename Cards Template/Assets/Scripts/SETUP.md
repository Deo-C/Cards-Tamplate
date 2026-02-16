# Kurulum ve Kullanım — Mobil Ana Kart Oyunu

## 1) Sahne Kurulumu
- Yeni bir `Canvas` oluşturun (Render Mode: Screen Space - Overlay).
- `Canvas` içinde bir `RectTransform` boş obje oluşturun; bu `spawnPoint` olacak (ör. `CardSpawnPoint`).
- `Canvas` üzerine bir `EventSystem` varsa yoksa ekleyin (Unity otomatik ekler genelde).
- `Canvas` öğesine `Canvas Scaler` ekleyin: UI Scale Mode = `Scale With Screen Size`, referans çözünürlük olarak 1080x1920 seçin.

## 2) Kart Prefab'leri Oluşturma

Her kart için şu adımları takip edin:

### Ana Yapı
- Yeni bir UI -> Image oluşturun (ana kart görüntüsü)
- Image objesinin içine şu alt elemanları ekleyin:
  - `TextMeshProUGUI`: Kart sorusu/başlığı
  - `TextMeshProUGUI` (isteğe bağlı): Sağ tarafta "EVET" metni (başlangıç alpha: 0)
  - `TextMeshProUGUI` (isteğe bağlı): Sol tarafta "HAYIR" metni (başlangıç alpha: 0)

### DragCard Script Ayarları
- `Image` objesine `DragCard` script'ini ekleyin (Assets/Scripts/DragCard.cs)
- Inspector'da `DragCard` component'ini ayarlayın:
  - `Card Type`: Kart türü seçin (StartGame, AnaCard, Pause, EndGame, Cheat)
  - `Yes Text`: Sağ taraf "EVET" Text objesini sürükle (varsa)
  - `No Text`: Sol taraf "HAYIR" Text objesini sürükle (varsa)
  - `Swipe Threshold`: 150 px (ihtiyaca göre değiştir)
  - `Swipe Speed`: 1000 px/s (çıkış animasyon hızı)
  - `Return Animation Duration`: 0.5 s (geri dönüş süresi)
  - `Exit Animation Duration`: 0.6 s (çıkış animasyon süresi)
  - `Entrance Animation Duration`: 0.6 s (giriş animasyon süresi)

### Boyutlandırma
- `Image` objesine `RectTransform` ile uygun boyut verin (ör. 800x450 px)
- Kart tasarımını verin (arka plan, köşe radius, renkler vb.)

### Prefab Haline Getirme
- Tamamlanan kartı `Assets/Prefabs` klasörüne sürükle
- Örnek isimler: `Card_StartGame`, `Card_Ana_1`, `Card_Ana_2`, `Card_EndGame`

## 3) Oyun Kontrolcüsü Kurulumu

- Sahneye boş bir GameObject ekleyin, adına `GameController` deyin
- `GameController` objesine `CardGameController` script'ini ekleyin (Assets/Scripts/CardGameController.cs)
- Inspector'da `CardGameController` component'ini doldurun:
  
  **Ana Ayarlar:**
  - `Start Card Prefab`: Oyun başlama kartı prefab'ı (zorunlu)
  - `Ana Card Prefabs`: Oyun kartları listesi (sırayla eklenir)
  - `End Card Prefab`: Oyun bitişi kartı
  
  **İsteğe Bağlı Kartlar:**
  - `Pause Card Prefabs`: Durdurma kartları listesi
  - `Cheat Card Prefabs`: Hile kartları listesi
  
  **Spawn Noktası:**
  - `Spawn Point`: CardSpawnPoint RectTransform'unu atayın

## 4) Kart Türleri ve Özellikleri

### StartGame (Oyun Başlama Kartı)
- Oyun başlangıcında gösterilir
- Kaydırıldığında ilk ana kartı tetikler
- Sadece bir kez spawn edilir (duplicate yok)

### AnaCard (Ana Kartlar)
- Oyun kartlarıdır
- `Ana Card Prefabs` listesinde sırayla eklenir
- Kaydırıldığında sonraki kartı tetikler
- **Giriş Animasyonu**: Aşağıdan yukarıya slide + fade in
- **Çıkış Animasyonu**: Momentum tabanlı (hızlı sürüklenirse hızlı çıkar)
- **Metin Animasyonu**: Sağa/sola kaydırırken ilgili metin fade in olur

### Pause (Durdurma Kartı)
- Oyun sırasında `SpawnPauseCard()` metoduyla tetiklenir
- Oyunu duraklatmak için kullanılabilir

### Cheat (Hile Kartı)
- Oyun sırasında `SpawnCheatCard()` metoduyla tetiklenir
- Özel aksiyonlar için (bonus vb.)

### EndGame (Oyun Bitişi Kartı)
- Tüm ana kartlar bitince gösterilir
- Oyun sonlandırma görseli olabilir

## 5) Animasyon Sistemi

### Entrance (Giriş) Animasyonu
- Yeni kartlar aşağıdan başlayıp yukarıya doğru slide ediyor
- Aynı zamanda alpha 0'dan 1'e fade in
- Süre: `Entrance Animation Duration` (öntanımlı 0.6 s)
- Animasyon sırasında kartı sürükleyemezsiniz

### Drag (Sürükleme) Animasyonları
- **Metin Fade**: Kartı sağ/sol tarafta kaydırırken ilgili metin fade in
- **Rotation**: Kartın rotasyonu sürükleme yönüne göre değişir
- **Threshold**: Eşik değer (150 px) aşılmazsa geri döner

### Exit (Çıkış) Animasyonu
- **Hızlı Sürükleme**: Kartlar hızlı slide out + fade out
- **Momentum**: Sürükleme hızına göre çıkış hızı ayarlanır
- **Doğal Hareket**: Ease-out cubic kullanılıyor

### Return (Geri Dönüş) Animasyonu
- Eşik değer aşılmadığında kart orijinal konumuna döner
- **Rotasyon Reset**: Kart düzleşir
- **Metin Fade Out**: Metinler kaybolur
- **Yumuşak Hareket**: Ease-out quad ile smooth dönüş
- Süre: `Return Animation Duration` (öntanımlı 0.5 s)

## 6) Kod İçinde Özel Kartları Tetikleme (C# Örneği)

```csharp
// Durdurma kartı spawn etmek
GameController gameController = GetComponent<CardGameController>();
gameController.SpawnPauseCard();

// Hile kartı spawn etmek
gameController.SpawnCheatCard();
```

## 7) Dokunmatik / Mobil İçin Notlar
- `DragCard` UI EventSystem tabanlı çalışır; mobilde dokunma olaylarını otomatik destekler
- `swipeThreshold` değerini oyunda test ederek değiştirin (farklı cihaz boyutları için)
- Portrait (dikey) modunda test etmeniz önerilir
- Geniş ekranlarda `entranceAnimationDuration` ve `exitAnimationDuration` arttırabilirsiniz

## 8) Build Ayarları (Android/iOS)
- `File -> Build Settings` → Hedef platform seçin (Android/iOS)
- Sahne(ler)i `Scenes In Build` listesine ekleyin
- `Player Settings` ayarları:
  - `Package Name`: com.company.game
  - `Orientation`: Portrait (tavsiye edilir)
  - `Target Minimum API Level`: Android 9.0+

## 9) Hızlı Test Adımları
1. Sahneyi Play yapın
2. Başlama kartının aşağıdan yukarıya geldiğini gözlemleyin
3. Kartı sağa kaydırın → "EVET" metni fade in
4. Kartı sola kaydırın → "HAYIR" metni fade in
5. Tam kaydırma → kart slide out + fade out
6. Kısmi kaydırma (eşik altında) → kart geri döner
7. Sonraki kartlar sırayla gelir

## 10) Dosya Yapısı
```
Assets/
├── Audio/                    (Ses dosyaları)
│   ├── CardTap.wav
│   ├── Swipe.wav
│   ├── CardRelease.wav
│   └── BackgroundMusic.mp3
├── Scripts/
│   ├── DragCard.cs           (Kart sürükleme ve animasyonları)
│   ├── CardGameController.cs (Oyun yönetimi ve kart spawn'u)
│   └── AudioManager.cs       (Ses sistemi yönetimi)
└── Prefabs/                  (Kart prefab'leri)
    ├── Card_StartGame
    ├── Card_Ana_1
    ├── Card_Ana_2
    ├── ...
    └── Card_EndGame
```

## 11) Ses Sistemi Kurulumu

### AudioManager Script Ayarları
- Sahneye boş bir GameObject ekleyin, adına `AudioManager` deyin
- `AudioManager` objesine `AudioManager` script'ini ekleyin (Assets/Scripts/AudioManager.cs)
- Inspector'da `AudioManager` component'ini ayarlayın:

  **Ses Efektleri (Sound Effects) - Varyantlarla**
  - `Sound Effects` listesine 3 grup ekleyin (+ butonuna basın):
    
    **1. CardTap Grubu:**
    - **Sound Name:** `CardTap`
    - **Clips:** 3-5 farklı tıklatma sesi ekle
      - cardtap_1.wav
      - cardtap_2.wav
      - cardtap_3.wav
    - **Volume:** 0.6
    - **Pitch Variation:** 0.15 (her ses biraz farklı tonlamada çalması için)
    
    **2. Swipe Grubu:**
    - **Sound Name:** `Swipe`
    - **Clips:** 3-4 farklı kaydırma sesi ekle
      - swipe_1.wav
      - swipe_2.wav
      - swipe_3.wav
    - **Volume:** 0.7
    - **Pitch Variation:** 0.1
    
    **3. CardRelease Grubu:**
    - **Sound Name:** `CardRelease`
    - **Clips:** 3-5 farklı çıkış sesi ekle
      - cardrelease_1.wav
      - cardrelease_2.wav
      - cardrelease_3.wav
    - **Volume:** 0.8
    - **Pitch Variation:** 0.12

  **Arka Plan Müzik (Background Music List):**
  - `Background Music List` listesine istediğin kadar müzik ekle (+ butonuna basın)
  - Her müzik için:
    - **Clip:** Müzik dosyası (AudioClip)
    - **Delay Between Tracks:** Müzik bittikten sonra bekleme süresi (saniye)
      - Örnek: 0.5 saniye bekleme sonra sonraki müzik başlasın
  - **Background Music Volume:** Genel müzik yüksekliği (0.3-0.5 önerilir)
  - **Randomize Music:** Müzikleri rastgele sırada çal (true = rastgele, false = sırayla)

### Ses Varyantları Nasıl Çalışır?
- Her ses grubu birden fazla clip içerir
- `PlaySound("CardTap")` çağrılınca, CardTap grubundan **rastgele** bir ses seçilir
- Aynı eylem yapılsa da her seferinde **farklı ses** duyulabilir
- Pitch Variation ek olarak her sesinin tonlamasını hafifçe değiştirir (daha doğal sesler)

### Ses Dosyası İçe Aktarma
1. Ses dosyalarını (MP3, WAV, OGG) `Assets/Audio` klasörüne kopyala
2. Unity, dosyaları otomatik olarak AudioClip'e dönüştürecek
3. AudioClip'ler Inspector'da AudioManager'ın listelerine sürükleyebilirsin

### Otomatik Ses Tetikleme
Ses sistemi artık otomatik olarak çalışıyor:
- **Kartı tuttuğunda:** `CardTap` grubundan rastgele ses oynatılır
- **Kartı sürüklerken:** Eşik aşıldığında `Swipe` grubundan rastgele ses oynatılır
- **Kartı serbest bıraktığında:** `CardRelease` grubundan rastgele ses oynatılır
- **Oyun başında:** Arka plan müzikler sırayla çalır (veya rastgele, ayarlara göre)

### Ses Kontrol (Opsiyonel - C# Kodda)
```csharp
// Arka plan müzikleri başlat
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayBackgroundMusic();

// Arka plan müzik durdur
if (AudioManager.Instance != null)
    AudioManager.Instance.StopBackgroundMusic();

// Belirli bir müziği oynat (index ile, 0'dan başlar)
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayMusicByIndex(1); // İkinci müziği çal

// Rastgele müzik modunu aç/kapat
if (AudioManager.Instance != null)
    AudioManager.Instance.SetRandomizeMusic(true); // Rastgele çal

// Ses seviyesi değiştir
if (AudioManager.Instance != null)
{
    AudioManager.Instance.SetSFXVolume(0.7f);      // Efekt sesleri %70
    AudioManager.Instance.SetMusicVolume(0.4f);    // Müzik %40
}
```

## 12) Bildirim Sistemi (Toast Notifications)

### NotificationManager Kurulumu
- Sahneye boş bir GameObject ekleyin, adına `NotificationManager` deyin
- `NotificationManager` objesine `NotificationManager` script'ini ekleyin (Assets/Scripts/NotificationManager.cs)
- Inspector'da `NotificationManager` component'ini ayarlayın:

  **Ayarlar:**
  - `Toast Notification Prefab`: Toast Notification Prefab'ını atayın (aşağıda oluşturacaksın)
  - `Notification Container`: Canvas objesini seç (bildirimler buraya spawn edilecek)
  - `Max Notifications On Screen`: Ekranda aynı anda gösterilecek max bildirim sayısı (3 önerilen)
  - `Vertical Spacing`: Bildirimler arasında boşluk (10px önerilen)

### Toast Notification Prefab'ı Oluşturma

1. Sahnede yeni bir UI Panel oluştur (Canvas içinde):
   - Adı: `Toast_Template`
   - Boyut: 400x80 (ihtiyaca göre ayarla)

2. İçine TextMeshProUGUI ekle:
   - Adı: `MessageText`
   - Yazı ayarla (görülebilir olsun)

3. Panel'e Image ekle (arka plan):
  - Adı: `Background`
  - Renk: Yeşil — HEX `#2AA24A` (alpha: 0.9)

4. `Toast_Template` (Panel) objesine script'leri ekle:
   - `ToastNotification` script

5. Panel'i `Assets/Prefabs/Toast_Template.prefab` olarak kaydet

6. Sahneden sil

### Bildirim Kullanımı (C# Kodda)

```csharp
// Basit bildirim (gri arka plan)
NotificationManager.Instance.ShowNotification("Merhaba!", 3f);

// Başarı bildirim (yeşil)
NotificationManager.Instance.ShowSuccess("✓ Kart indirildi!");

// Hata bildirim (kırmızı)
NotificationManager.Instance.ShowError("✗ Bir hata oluştu!");

// Bilgi bildirim (mavi)
NotificationManager.Instance.ShowInfo("ℹ Oyun başladı!");

// Uyarı bildirim (turuncu)
NotificationManager.Instance.ShowWarning("⚠ Dikkat!");
```

### Bildirim Renkleri
- **Başarı**: Yeşil — HEX `#2AA24A` (alpha: 0.9)
- **Hata**: Kırmızı (0.9, 0.2, 0.2)
- **Bilgi**: Mavi (0.2, 0.6, 0.9)
- **Uyarı**: Turuncu (1, 0.6, 0.2)

---

## 13) Kart İndirme Sistemi

### CardDownloadButton Kurulumu
- Oyun ekranının üst bölümüne yeni bir Button ekle:
  - Adı: `DownloadButton`
  - Text: "📥 İndir" veya "Download"

- Button objesine `CardDownloadButton` script'ini ekle (Assets/Scripts/CardDownloadButton.cs)

- Inspector'da ayarla:
  - `Download Button`: Button bileşeni (otomatik eklenmiş olabilir)
  - `Download File Name`: Dosya adı prefix'i (örn. "Card_")

### İndirme Sistemi Nasıl Çalışır?

1. Oyun başladığında, CardDownloadButton ekrandaki mevcut kartı otomatik olarak tespit eder
2. Kartlar değiştikçe (biri gidiyor, diğeri geliyor), buton otomatik olarak yeni kartı takip eder
3. Oyuncu "İndir" butonuna tıklar
4. Ekranda bulunan mevcut kartın screenshot'ı alınır
5. PNG dosyası olarak `Application.persistentDataPath` klasörüne kaydedilir
6. Bildirim sistem aracılığıyla başarı/hata mesajı gösterilir

### Otomatik Kart Tespiti

CardDownloadButton her frame'de sahnadaki tüm DragCard bileşenlerini tarar:
- Ekranda hangi kart varsa (giriş animasyonunu tamamlayan), o otomatik tespit edilir
- Kartlar değiştikçe buton otomatik güncellenir
- **Inspector'dan manuel ayarlama gerekmez!**

### Kaydedilen Dosya Konumu

**Android:** `/data/data/com.company.game/files/`
**iOS:** App Documents klasörü
**Windows/Editor:** `C:\Users\[User]\AppData\LocalLow\[Company]\[Game]\`

Dosya adı: `Card_2025-01-18_14-30-45.png` formatında

---

## 14) Ayarlar Menüsü (Settings Menu)

### SettingsManager Kurulumu
- Sahneye boş bir GameObject ekleyin, adına `SettingsManager` deyin
- `SettingsManager` objesine `SettingsManager` script'ini ekleyin (Assets/Scripts/SettingsManager.cs)

### Ayarlar Paneli Oluşturma

1. Sahnede yeni bir Panel oluştur (Canvas içinde):
   - Adı: `SettingsPanel`
   - Boyut: 600x700 (fullscreen benzeri)
   - Position: Center
   - Renk: Yarı-saydam koyu (RGBA: 0, 0, 0, 0.7)

2. Panel içine şu alt öğeleri ekle:

   **Başlık (Title):**
   - TextMeshProUGUI: "⚙️ AYARLAR"
   - Font boyutu: 60
   - Konumu: Üst merkez

   **Müzik Slider:**
   - TextMeshProUGUI: "Müzik Seviyesi" (label)
   - Slider: 0-1 arası
   - TextMeshProUGUI: Müzik Volume Text (değeri göstermek için)

   **SFX Slider:**
   - TextMeshProUGUI: "Efekt Sesleri" (label)
   - Slider: 0-1 arası
   - TextMeshProUGUI: SFX Volume Text (değeri göstermek için)

   **Kapat Butonu:**
   - Button: "Kapat"
   - Renk: Kırmızı (#CC0000)

3. `SettingsPanel` objesine `SettingsManager` script referanslarını ata:
   - `Settings Panel`: Bu panel objesini seç
   - `Music Volume Slider`: Müzik slider'ı seç
   - `SFX Volume Slider`: SFX slider'ı seç
   - `Music Volume Text`: Müzik tekst objesini seç
   - `SFX Volume Text`: SFX tekst objesini seç
   - `Close Button`: Kapat butonunu seç

4. Başta paneli gizle (Active = false)

### Ayarlar Menüsünü Açma

**Butondan açmak (UI Button):**
```csharp
// Button üzerine OnClick listener ekle:
SettingsManager.Instance.OpenSettings();
```

**Kod üzerinden açmak:**
```csharp
SettingsManager.Instance.OpenSettings();   // Açmak
SettingsManager.Instance.CloseSettings();  // Kapatmak
```

### Ayarlar Özellikleri
- **Müzik Seviyesi**: AudioManager ile entegre; slider 0-100% arasında kontrol eder
- **Efekt Sesleri**: Ses efektlerinin yüksekliği; slider ile ayarlanır
- **Otomatik Kaydetme**: Ayarlar `PlayerPrefs` ile kaydedilir; oyunu kapatıp açsanız ayarlar korunur
- **Sıfırla**: `ResetSettings()` metodu çağrılarak ayarlar varsayılana döndürülebilir

### Özel Ayarlama (C# Kodda)

```csharp
// Ayarları programlı olarak değiştir
if (AudioManager.Instance != null)
{
    AudioManager.Instance.SetMusicVolume(0.8f);  // %80 müzik
    AudioManager.Instance.SetSFXVolume(0.6f);    // %60 efekt sesleri
}

// Ayarları sıfırla
SettingsManager.Instance.ResetSettings();
```

---

## 15) İleri Özellik Önerileri
- Skor sistemi (evet/hayır sayısı)
- Zamanlayıcı (süre sınırı)
- Parçacık efektleri (çıkış animasyonu sırasında)
- Zoom animasyonları (kartlar biraz büyüyebilir)
- Sosyal medya paylaşımı
- Leaderboard sistemi
- Dil seçimi (Türkçe/İngilizce)
- Ekran parlaklığı kontrolü

