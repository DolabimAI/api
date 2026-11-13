# DolabımAI · Veritabanı betikleri

Şemanın tek kaynağı bu klasördür. Tablolar SQL ile kurulur, C# entity'leri bu şemadan **üretilir** (scaffold). EF migration kullanılmaz.

## Çalıştırma sırası

| Dosya | Ne yapar | Kaç kez |
|---|---|---|
| `001_schema.sql` | 27 tablo, `wardrobe_summary` görünümü, kısıtlar, tetikleyiciler, RLS | Boş veritabanında bir kez |
| `002_seed_lookups.sql` | 16 sabit liste (kategori, renk, sonuçlar, keşif modları ...) | Tekrar çalıştırılabilir |
| `003_seed_subtypes.sql` | 35 alt tür ve varsayılanları | Tekrar çalıştırılabilir |
| `004_seed_locations.sql` | 81 il, 973 ilçe ve koordinatları | Tekrar çalıştırılabilir |
| `099_kontrol.sql` | Yalnız okur: kod listesi dökümü ve kapsama kontrolleri | İstendiğinde |

Supabase'de: SQL Editor → dosyanın içeriğini yapıştır → Run. Ya da:

```bash
psql "$DB_URL" -v ON_ERROR_STOP=1 -f 001_schema.sql -f 002_seed_lookups.sql -f 003_seed_subtypes.sql -f 004_seed_locations.sql
```

Her dosya tek işlemdir: bir satır bile hatalıysa hiçbir şey yazılmaz. Seed'ler sonda satır sayısını kontrol eder. 003, yanlış yazılmış kodu (`gunlk` gibi) adıyla birlikte bildirir.

**Seed'ler satırları value'ya göre günceller.** id'ler değişmez; listeden çıkarılan satır tablodan silinmez. 7 Aralık'ta hepsi dondurulur.

## Şema değişince

1. Yeni bir dosya eklenir: `005_<kısa_ad>.sql`. Eski dosyalar değiştirilmez.
2. Entity'ler yeniden üretilir:

`dotnet ef dbcontext scaffold "Name=ConnectionStrings:Db" Npgsql.EntityFrameworkCore.PostgreSQL --project src/DolApp.Infrastructure --startup-project src/DolApp.Api --schema public --context AppDbContext --context-dir Data --context-namespace DolApp.Infrastructure.Data --output-dir ../DolApp.Core/Entities --namespace DolApp.Core.Entities --no-onconfiguring --force`

`--schema public` şart; yoksa Supabase'in `auth` ve `storage` tabloları da gelir. Üretilen dosyalar elle düzenlenmez. Ek kod `partial class` ile ayrı dosyaya yazılır.

## Veritabanının kendisinin koruduğu kurallar

- Liste tablolarında `value` yalnız `a-z 0-9 _` içerir (`result_statuses`'ta büyük harf).
- Kullanıcının Şehrim'inde ve kombin oturumunda ilçe, seçilen ile ait olmak zorunda. İlçe varsa il de olmalı.
- `model_predictions` güncellenemez; taslak silinince tahmini de silinir.
- `label_changes` ve `outfit_changes` güncellenemez ve silinemez. Geliştirmede test verisini temizlemek için `TRUNCATE` kullanılır.
- `users.updated_at` ve `clothes.updated_at` her güncellemede otomatik yazılır.
- Tekil kısıtlar: `comparisons (session_id, sequence_no)`, `outfit_changes (comparison_id, order_no)`, `districts (city_id, value)`. İhlalde Postgres `23505` döner; C# bunu 409'a çevirir.
- Bütün tablolarda RLS açık ve politika yok: Supabase'in otomatik REST API'si hiçbir tabloya erişemez. C# `postgres` rolüyle bağlandığı için etkilenmez.

## Bilerek C#'a bırakılanlar

- Liste sütunlarındaki id'lerin geçerliliği (`style_ids`, `rule_outfit` ...)
- `comparisons.left_method_id` yalnız `kural` ya da `ogrenen`; tek kartta `kural`
- `target_date`: bugün ile 6 gün sonrası arası
- Onay için tür, renk ve tonun dolu olması
- `users.id` Supabase giriş hesabına yabancı anahtarla bağlı değil: hesap silinince giriş hesabı silinir, satır kimliksiz kalır.

## 9 Kasım kararları (belgelere işlenecek)

- `learned_pick_modes`: `id, value, display_name, is_random` (kesif ve soguk_baslangic `true`)
- `label_fields` ekran adları onay ekranıyla aynı: Tür, Renk, Ton, Desen, Tarz, Kalınlık, Mevsim
- `label_change_stages`: onay → "Onay ekranı", sonradan → "Parça detayı"
- `weather_conditions.api_codes`: yalnız Open-Meteo'nun kullandığı gerçek kodlar
- `subtypes`: Ortak Kod Listesi'ndeki 7 Kasım değerleri (Onur D5'te değiştirirse 003 yeniden çalıştırılır)

## İl ve ilçe verisi (004)

`tools/build_locations.py` ile üretildi. Elle düzenlenmez; yeniden üretilir.

- İl ve ilçe listesi ile il merkezleri: [turkiye-api](https://github.com/ubeydeozdmr/turkiye-api) (MIT), 2025 verisi
- İlçe merkezi: adı ilçeyle aynı olan ve ilçe sınırının içinde kalan GeoNames noktası (848 ilçe). GeoNames, CC BY 4.0.
- Bulunamayan 125 ilçe için ilçe sınırının iç noktası. Sınırlar [geoBoundaries](https://www.geoboundaries.org) TUR ADM2'den alındı; kaynağı OpenStreetMap, lisansı ODbL.

Tezde ve README'de bu kaynakların gösterilmesi lisans şartıdır.
