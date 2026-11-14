# C# API ↔ Python servisi sözleşmesi (A3)

Bu belge, C# API ile Python servisinin (`ml` deposu) birbirine **ne gönderip ne aldığını** tanımlar.
Uygulama ↔ C# biçimleri ayrı bir belgede, "DolabımAI C# API Uçları"nda durur.
Bu belgedeki örneklerin hepsi `examples/` klasöründe JSON dosyası olarak da var. Mock'larda ve testlerde doğrudan kullanılabilir.

**Durum:** Taslak, 14 Kasım 2026. Beş kişinin onayından sonra sabitlenir (aşağıdaki "Onay" bölümü).
Sabitlendikten sonra değişiklik yalnızca ilgili herkesin onayıyla yapılır.

## İçindekiler

1. [Ortak kurallar](#1-ortak-kurallar)
2. [Parça nesnesi](#2-parça-nesnesi)
3. [Bağlam nesnesi](#3-bağlam-nesnesi)
4. [GET /health](#4-get-health)
5. [POST /analyze](#5-post-analyze)
6. [POST /recommend](#6-post-recommend)
7. [POST /swap](#7-post-swap)
8. [Hatalar](#8-hatalar)
9. [Verilen kararlar](#9-verilen-kararlar)
10. [Onay](#10-onay)

---

## 1. Ortak kurallar

| Konu | Kural |
|---|---|
| Adres | C# Python'a `appsettings.json > Python:BaseUrl` adresinden ulaşır (geliştirmede `http://localhost:8000`). Python servisi yalnızca C#'tan istek alır; uygulama Python'a doğrudan gitmez. |
| Alan adları | JSON alanları **camelCase**: `feelsLikeTemp`, `isPatterned`. Python iç kodunda snake_case kullanmaya devam eder; dönüşüm yalnızca servis katmanında yapılır (Pydantic `alias_generator=to_camel`, `populate_by_name=True`). |
| Liste değerleri | Alt tür, kategori, renk, ton, tarz, mevsim, hava ve durum **kodla** gider ve gelir: `"gomlek"`, `"beyaz"`, `"yagmurlu"`. Kodlar veritabanındaki `value` sütunlarıdır ve ortak kod listesiyle birebir aynıdır. id ↔ kod çevirisini C# yapar. |
| Kimlikler | Kıyafetin kendi id'si (`clothes.id`) ve karşılaştırma id'si (`comparisons.id`) **sayı** olarak gider. Kullanıcı id'si **metin (UUID)**. (Python'un iç `ComparisonDecision` modeli `comparison_id`'yi metin bekliyor; servis katmanı `str()` ile çevirir.) |
| Tarih ve saat | ISO 8601, UTC, sonunda `Z`: `"2026-11-23T07:42:10Z"`. |
| Sıcaklık | °C, ondalıklı sayı (`9.4`). |
| Zaman aşımı | C# her isteği en fazla **30 saniye** bekler (`Python:TimeoutSeconds`). Süre aşılırsa istek başarısız sayılır. |
| Karakter kodlaması | UTF-8. Türkçe karakterler JSON'da olduğu gibi yazılır. |

---

## 2. Parça nesnesi

`/recommend` ve `/swap` istekleri dolaptaki ve geçmişteki kıyafetleri bu biçimde gönderir.
Python'un `ClothingItem` modeliyle alan alan aynıdır; yalnızca adlar camelCase'tir.

```json
{
  "id": 1,
  "subtype": "tisort",
  "category": "ust",
  "color": "beyaz",
  "tone": "acik",
  "isPatterned": false,
  "styles": ["gunluk", "spor"],
  "minTemp": 5,
  "maxTemp": 40,
  "avoidWhenWet": false,
  "preferredWhenWet": false,
  "preferredWhenHot": false,
  "status": "onayli"
}
```

| Alan | Nereden gelir (C#) | Not |
|---|---|---|
| `id` | `clothes.id` | |
| `subtype`, `category`, `color`, `tone` | Kıyafetin **güncel** etiketleri (`clothes`) | Kodla |
| `isPatterned` | `clothes.is_patterned` | |
| `styles` | Kıyafetin **güncel** tarzları | Kullanıcının seçtiği tarzlar; alt türün varsayılanları değil |
| `minTemp`, `maxTemp` | `clothes.min_temp`, `clothes.max_temp` | Alt türden kopyalanmış değerler |
| `avoidWhenWet`, `preferredWhenWet`, `preferredWhenHot` | Kıyafetin alt türü (`subtypes`) | |
| `status` | `onayli` ya da `silindi` | Dolapta (`wardrobe`) yalnızca `onayli` gider. Geçmişte (`history`) silinmiş parçalar `silindi` olarak gelir. |

**Aksesuarlar** de aynı biçimdedir (`category: "aksesuar"`). Dolapta onaylı aksesuarlar da gönderilir; Python onları aday üretimine katmaz, yalnızca aksesuar seçiminde kullanır.

---

## 3. Bağlam nesnesi

Kombinin hangi koşullar için istendiğini anlatır.

```json
{
  "feelsLikeTemp": 9.4,
  "weatherCondition": "yagmurlu",
  "timeOfDay": "aksam",
  "isDaytime": false,
  "eventStyle": "gunluk",
  "wantsAccessories": true
}
```

| Alan | Nereden gelir (C#) |
|---|---|
| `feelsLikeTemp` | Hava servisi (vaktin saatlerindeki ortalama hissedilen sıcaklık, 1 ondalık) |
| `weatherCondition` | Vaktin en ağır hava durumu (`weather_conditions.value`) |
| `timeOfDay` | `times_of_day.value`: `sabah`, `ogle`, `aksam`, `gece` |
| `isDaytime` | `times_of_day.is_daytime`. Aksesuar kuralı (güneş gözlüğü, şapka) için |
| `eventStyle` | Formda seçilen etkinlik (`styles.value`) |
| `wantsAccessories` | Formdaki "aksesuar istiyorum" seçimi |

Geçmiş kararlarda (`history`) bağlamın kısa hali gönderilir: `feelsLikeTemp`, `weatherCondition`, `eventStyle`. Bunlar **karar anındaki** değerlerdir (`comparison_sessions`).

---

## 4. GET /health

Servisin ayakta olup olmadığını gösterir. C# açılışta ve sorun ararken kullanır.

**Cevap `200`:**

```json
{ "status": "ok" }
```

Örnek: `examples/health-response.json`

---

## 5. POST /analyze

Fotoğrafı işler: arka planı temizler, görseli depoya yazar ve alt tür, renk, ton, desen tahmini döner.
**Sorumlu:** Onur. **C# tarafı:** `POST /api/clothes/analyze` (C3).

### İstek

`multipart/form-data`

| Alan | Tür | Açıklama |
|---|---|---|
| `image` | dosya | Kullanıcının çektiği ya da seçtiği fotoğraf. Dosya türü ve boyutu C#'ta önceden kontrol edilir. |
| `userId` | metin (UUID) | Görselin depodaki klasörü için |

### Python'un yaptığı

1. Fotoğrafı okur. Okuyamazsa `422 image_unreadable` döner ve hiçbir şey yazmaz.
2. Arka planı temizler ve görseli `gorseller` bucket'ına **`<userId>/<rastgele-ad>.webp`** yoluna yazar (en fazla 1 MB, `image/webp`).
3. Alt türü (FashionCLIP), ana rengi, tonu ve deseni tahmin eder.
4. Görsel yazıldıktan sonra bir adım başarısız olursa **yazdığı görseli siler**, sonra hata döner.

### Cevap `200`

```json
{
  "imagePath": "178eaf32-7cf1-4399-8957-6532e4c7fc91/0b7e3c1a9f4d4e2c.webp",
  "subtype": "gomlek",
  "subtypeConfidence": 0.873,
  "color": "beyaz",
  "tone": "acik",
  "isPatterned": false,
  "rawColor": "#F2F0EB"
}
```

| Alan | Açıklama | Veritabanında |
|---|---|---|
| `imagePath` | Görselin bucket içindeki yolu | `clothes.image_path` |
| `subtype` | En olası alt tür (kod) | `model_predictions.subtype_id` |
| `subtypeConfidence` | 0 ile 1 arası, 3 ondalık | `model_predictions.subtype_confidence` |
| `color` | Ana renk (13 renkten biri, kod) | `model_predictions.main_color_id` |
| `tone` | `acik`, `orta`, `koyu` | `model_predictions.tone_id` |
| `isPatterned` | Desenli mi | `model_predictions.is_patterned` |
| `rawColor` | Ölçülen ham renk, `#RRGGBB` | `model_predictions.raw_color` |

Python güven ne olursa olsun **en olası** alt türü döner. Eşik kararını C# verir: güven `Model:ConfidenceThreshold` (0,40) altındaysa uygulamaya alt tür boş gider ve kullanıcı "Seç" ile kendisi seçer. Tahmin `model_predictions` tablosuna değiştirilmeden yazılır; modelin doğruluğu buradan ölçülür.

**C# sonrasında başarısız olursa:** Python'un cevabından sonra C#'ta bir adım başarısız olursa (örneğin veritabanına yazılamazsa) görseli **C# siler**.

### Hatalar

| Durum | Python | C#'ın uygulamaya döndüğü |
|---|---|---|
| Fotoğraf okunamadı | `422` `{ "code": "image_unreadable", ... }` | `400 image_unreadable`: "Fotoğraf okunamadı. Lütfen tekrar çeker misin?" |
| Diğer hatalar, 30 sn aşımı | `500` ya da cevap yok | `502 analysis_failed` |

Örnekler: `examples/analyze-response.json`, `examples/analyze-error-image-unreadable.json`

---

## 6. POST /recommend

Bir karşılaştırma için **iki kartı birlikte** üretir: Kural Kartı ve Öğrenen Kart. İki kart **aynı aday havuzundan** seçilir (E1 bir kez çalışır). Aksesuar istendiyse ikisine aynı aksesuarlar eklenir.
**Sorumlu:** Ömer. **C# tarafı:** `POST /api/comparisons/sessions` (C5) ve `POST /api/comparisons/{id}/reject-both` (C6).

### İstek

```json
{
  "userId": "178eaf32-7cf1-4399-8957-6532e4c7fc91",
  "context": {
    "feelsLikeTemp": 9.4,
    "weatherCondition": "yagmurlu",
    "timeOfDay": "aksam",
    "isDaytime": false,
    "eventStyle": "gunluk",
    "wantsAccessories": true
  },
  "wardrobe": [ { "id": 1, "subtype": "tisort", "...": "parça nesnesi" } ],
  "rejectedOutfits": [],
  "history": [
    {
      "comparisonId": 4826,
      "resultKey": "OD",
      "completedAt": "2026-11-23T16:05:44Z",
      "context": { "feelsLikeTemp": 8.1, "weatherCondition": "yagmurlu", "eventStyle": "is" },
      "ruleOutfit": { "template": "ust_alt", "items": [ "parça nesneleri" ] },
      "learnedOutfit": { "template": "ust_alt", "items": [ "parça nesneleri" ] },
      "finalOutfit": { "template": "ust_alt", "items": [ "parça nesneleri" ] }
    }
  ]
}
```

| Alan | Açıklama |
|---|---|
| `userId` | Öğrenen yöntemin yalnızca bu kullanıcının geçmişinden öğrenmesi için |
| `context` | [Bağlam nesnesi](#3-bağlam-nesnesi) |
| `wardrobe` | Kullanıcının **onaylı** bütün parçaları, aksesuarlar dahil ([parça nesnesi](#2-parça-nesnesi)). Dolap şartı C#'ta kontrol edilmiştir. |
| `rejectedOutfits` | **Yalnızca bu oturumda** "İkisi de olmaz" ile reddedilen kombinler. Her biri ana parça id'lerinin listesidir (dış giyim dahil, aksesuar hariç): `[[1, 10, 17, 24], [5, 9, 17, 24]]`. İlk istekte boş. Geçmiş oturumların redleri **eklenmez**. |
| `history` | Kullanıcının **ilk günden itibaren bütün** öğrenmeye giren kararları, tamamlanma zamanına göre eskiden yeniye |

**`history` kuralları:**

- Yalnızca şu sonuçlar gönderilir: `KT`, `KD`, `KR`, `OT`, `OD`, `OR`, `HB`. Diğerleri (`BK`, `KS`, `OS`, `AT`, `AD`, `AR`, `TP`) öğrenmeye girmediği için gönderilmez.
- Yalnızca son birkaç karar **yetmez**: E4'ün sayaçları (10 karar eşiği, kural sınırı) baştan sayılır.
- Kıyafet etiketleri **güncel** etiketlerdir (karar 3). Kullanıcı kararı verirken etiketi değil fotoğrafı görür; düzeltilmiş etiket gerçekte neyi seçtiğini daha doğru anlatır.
- Bağlam (`feelsLikeTemp`, `weatherCondition`, `eventStyle`) **karar anındaki** değerlerdir.
- O karardan sonra **silinmiş** parçalar da gönderilir (`status: "silindi"`). Geçmiş, bugünkü dolaptan yeniden kurulmaz.
- `template`: Kombinde tek parça varsa `tek_parca`, yoksa `ust_alt`. C# hesaplar.
- `finalOutfit`: Son karar değişiklikle verildiyse (`KD`, `OD`) ve kombinin son hali kaydedildiyse dolu, yoksa `null`. İçinde aksesuar varsa da gönderilir; Python aksesuarları öğrenmeye katmaz.
- `completedAt`: Kararın tamamlandığı an (`comparisons.completed_at`).

### Cevap `200`: kartlar üretildi

```json
{
  "status": "ok",
  "ruleOutfit": { "template": "ust_alt", "itemIds": [1, 10, 17, 24] },
  "learnedOutfit": { "template": "ust_alt", "itemIds": [5, 9, 17, 24] },
  "isSameOutfit": false,
  "learnedPickMode": "model",
  "accessoryIds": [34]
}
```

| Alan | Açıklama | C#'ın kullanımı |
|---|---|---|
| `ruleOutfit.itemIds` | Kural Kartının ana parçaları (dış giyim dahil) | `comparisons.rule_outfit` |
| `learnedOutfit.itemIds` | Öğrenen Kartın ana parçaları | `comparisons.learned_outfit` |
| `isSameOutfit` | İki yöntem aynı ana parça kümesini seçtiyse `true` | Uygulamaya **tek kart** gider (`isSingleCard`) |
| `learnedPickMode` | `soguk_baslangic`, `kesif` ya da `model` | `comparisons.learned_pick_mode_id`. **Uygulamaya gönderilmez** (yöntem gizliliği) |
| `accessoryIds` | İki karta da eklenecek aksesuarlar. `wantsAccessories: false` ise ya da uygun aksesuar yoksa boş liste | `comparisons.accessories` |

Kartların hangisinin solda duracağına **C# karar verir** (rastgele); Python'un bilmesine gerek yok.

### Cevap `200`: aday yok

```json
{ "status": "no_candidates" }
```

Hava, etkinlik ve oturumdaki redlerden sonra hiç uygun kombin kalmadıysa döner. Bu bir hata değil, beklenen bir sonuçtur.
C#: İlk istekte uygulamaya `422 no_candidates` ("Bu koşullara uygun kombin bulamadık") döner. "İkisi de olmaz" sonrasında `{ "noMoreCandidates": true }` döner.

### Hatalar

| Durum | C#'ın uygulamaya döndüğü |
|---|---|
| `422 invalid_request` (istek biçimi bozuk, bilinmeyen kod) | `502 recommendation_failed`. C# tarafında bir hata demektir; loga yazılır |
| `500`, 30 sn aşımı | `502 recommendation_failed` |

Örnekler: `examples/recommend-request.json`, `examples/recommend-response.json`, `examples/recommend-response-same-outfit.json`, `examples/recommend-response-no-candidates.json`

---

## 7. POST /swap

Kullanıcı seçtiği kombinde bir parçayı ya da bir aksesuarı değiştirmek istediğinde **tek bir yeni parça** önerir.
**Sorumlu:** Ömer (E5). **C# tarafı:** `POST /api/comparisons/{id}/swap` (C8).
Aksesuarı **kaldırmak** Python'a gitmez; onu C# kendisi yapar.

### İstek

```json
{
  "userId": "178eaf32-7cf1-4399-8957-6532e4c7fc91",
  "method": "kural",
  "context": { "feelsLikeTemp": 9.4, "weatherCondition": "yagmurlu", "timeOfDay": "aksam", "isDaytime": false, "eventStyle": "gunluk", "wantsAccessories": true },
  "currentOutfit": { "template": "ust_alt", "itemIds": [1, 10, 17, 24], "accessoryIds": [34] },
  "targetCategory": "ust",
  "targetAccessoryId": null,
  "excludedItemIds": [5],
  "wardrobe": [ "parça nesneleri" ],
  "history": []
}
```

| Alan | Açıklama |
|---|---|
| `method` | `kural`, `ogrenen` ya da `ortak_kural`. Ana parça değişiminde seçilen kartın yöntemi; tek kartta `kural`. Aksesuar değişiminde `ortak_kural`. |
| `currentOutfit` | Kombinin **o anki** hali (önceki değişiklikler uygulanmış) |
| `targetCategory` | Değişecek ana parçanın kategorisi: `ust`, `alt`, `tek_parca`, `dis`, `ayakkabi`. Aksesuar değişiminde `null` |
| `targetAccessoryId` | Değişecek aksesuarın id'si. Ana parça değişiminde `null` |
| `excludedItemIds` | Aday olmayacak parçalar: şu anki parça ve **bu kartta** daha önce değiştirilmiş (reddedilmiş) parçalar (`outfit_changes.old_item_id`) |
| `wardrobe` | Onaylı bütün parçalar ([parça nesnesi](#2-parça-nesnesi)) |
| `history` | `method: "ogrenen"` ise `/recommend`'deki gibi bütün geçmiş; diğer yöntemlerde boş liste |

**Python'un uyguladığı kurallar (E5):** Ortak ön filtre (sıcaklık, etkinlik, ıslak ve sıcak hava) geçerlidir. `kural`: kalan parçalarla en uyumlu parça. `ogrenen`: aynı modelle o yer için en iyi parça. `ortak_kural`: kombinin o anki haline göre sıradaki en uyumlu aksesuar.

### Cevap `200`

```json
{ "status": "ok", "newItemId": 6 }
```

```json
{ "status": "no_alternative" }
```

`no_alternative`: Uygun başka parça kalmadı. Bu bir hata değil. C# hiçbir şey yazmaz ve uygulamaya `{ "newItem": null, "noAlternative": true }` döner; uygulama o parçanın "Değiştir" düğmesini pasif yapar.

### Hatalar

`/recommend` ile aynı: `422 invalid_request`, `500` ya da 30 sn aşımı → C# `502 recommendation_failed`.

Örnekler: `examples/swap-request.json`, `examples/swap-response.json`, `examples/swap-response-no-alternative.json`

---

## 8. Hatalar

Python'un bütün hata cevapları aynı biçimdedir:

```json
{ "code": "internal_error", "message": "Beklenmeyen bir hata oluştu." }
```

| HTTP | `code` | Ne zaman |
|---|---|---|
| `422` | `image_unreadable` | Yalnızca `/analyze`: fotoğraf okunamadı |
| `422` | `invalid_request` | İstek biçimi bozuk ya da bilinmeyen bir kod geldi. FastAPI'nin varsayılan doğrulama cevabı (`{"detail": [...]}`) bu biçime çevrilir. |
| `500` | `internal_error` | Beklenmeyen hata |

Bu mesajlar kullanıcıya **gösterilmez**; C# kendi hata kodunu ve mesajını döner. Python'un mesajı yalnızca C#'ın loguna yazılır.
"Aday yok" (`no_candidates`) ve "uygun parça yok" (`no_alternative`) hata değil, `200` cevaplarıdır.

Örnek: `examples/error.json`

---

## 9. Verilen kararlar

| # | Konu | Karar | Kimin kararı |
|---|---|---|---|
| 1 | JSON alan adları | camelCase; Python servis katmanında dönüştürür | Zehra |
| 2 | Liste değerleri | Kodla (`value`); kıyafet ve karşılaştırma id'leri sayı | Zehra |
| 3 | Geçmiş kararlardaki etiketler | Güncel etiketler; bağlam karar anındaki değerler; silinmiş parçalar dahil | Ömer |
| 4 | `/analyze` hata durumu | Tek hata: `image_unreadable`; 30 sn yeterli | Onur |
| 5 | `/swap` ve aksesuarlar | Şimdiden sözleşmede | Zehra |
| 6 | Belge biçimi | Markdown + `examples/` JSON dosyaları | Onur, Ömer |
| 7 | Geçmişin kapsamı | Her istekte ilk günden itibaren `KT, KD, KR, OT, OD, OR, HB` kararlarının hepsi | Ömer |

**C# API Uçları belgesine eklenecek:** `400 image_unreadable` ("Fotoğraf okunamadı. Lütfen tekrar çeker misin?").

---

## 10. Onay

Plana göre A3, bu belge api deposunda yazılı olduğunda ve beş kişi onayladığında biter.

| Kişi | Sorumlu olduğu kısım | Onay |
|---|---|---|
| Zehra | Belgenin tamamı, C# tarafı | ☐ |
| Onur | `/analyze`, `/health` | ☐ |
| Ömer | `/recommend`, `/swap`, `history` | ☐ |
| Bilge | Uygulamaya yansıyan sonuçlar (tek kart, aday yok, uygun parça yok, hata mesajları) | ☐ |
| Sude | Aynı | ☐ |

Onay, bu belgeyi ekleyen PR'da "Approve" ile verilir.
