-- =====================================================================
-- DolabımAI · 002_seed_lookups.sql
-- subtypes ve il/ilçe dışındaki bütün sabit listeler.
-- Kaynak: Ortak Kod Listesi ve Sabit Tablolar belgesi + 9 Kasım kararları.
--
-- Tekrar çalıştırılabilir: satırlar value'ya göre güncellenir, id'ler
-- değişmez. Listeden bir satır çıkarılırsa tablodan SİLİNMEZ; elle silinir.
-- 7 Aralık'ta dondurulur.
-- =====================================================================

begin;

-- ---------- Kıyafet listeleri ----------

insert into categories (value, display_name) values
  ('ust',       'Üst'),
  ('alt',       'Alt'),
  ('tek_parca', 'Tek parça'),
  ('dis',       'Dış giyim'),
  ('ayakkabi',  'Ayakkabı'),
  ('aksesuar',  'Aksesuar')
on conflict (value) do update set display_name = excluded.display_name;

insert into styles (value, display_name) values
  ('gunluk', 'Günlük'),
  ('is',     'İş'),
  ('spor',   'Spor'),
  ('davet',  'Davet')
on conflict (value) do update set display_name = excluded.display_name;

insert into seasons (value, display_name) values
  ('ilkbahar', 'İlkbahar'),
  ('yaz',      'Yaz'),
  ('sonbahar', 'Sonbahar'),
  ('kis',      'Kış')
on conflict (value) do update set display_name = excluded.display_name;

insert into thicknesses (value, display_name) values
  ('ince',  'İnce'),
  ('orta',  'Orta'),
  ('kalin', 'Kalın')
on conflict (value) do update set display_name = excluded.display_name;

insert into colors (value, display_name) values
  ('siyah',      'Siyah'),
  ('beyaz',      'Beyaz'),
  ('gri',        'Gri'),
  ('bej',        'Bej'),
  ('lacivert',   'Lacivert'),
  ('kahverengi', 'Kahverengi'),
  ('kirmizi',    'Kırmızı'),
  ('turuncu',    'Turuncu'),
  ('sari',       'Sarı'),
  ('yesil',      'Yeşil'),
  ('mavi',       'Mavi'),
  ('mor',        'Mor'),
  ('pembe',      'Pembe')
on conflict (value) do update set display_name = excluded.display_name;

insert into tones (value, display_name) values
  ('acik', 'Açık'),
  ('orta', 'Orta'),
  ('koyu', 'Koyu')
on conflict (value) do update set display_name = excluded.display_name;

insert into clothing_statuses (value, display_name) values
  ('taslak', 'Taslak'),
  ('onayli', 'Onaylı'),
  ('silindi', 'Silindi')
on conflict (value) do update set display_name = excluded.display_name;

-- Ekran adları ve sıra, onay ekranı ve parça detayındaki etiketlerle aynı (9 Kasım)
insert into label_fields (value, display_name) values
  ('alt_tur',  'Tür'),
  ('ana_renk', 'Renk'),
  ('ton',      'Ton'),
  ('desen',    'Desen'),
  ('tarz',     'Tarz'),
  ('kalinlik', 'Kalınlık'),
  ('mevsim',   'Mevsim')
on conflict (value) do update set display_name = excluded.display_name;

-- Düzeltmenin yapıldığı ekran (9 Kasım)
insert into label_change_stages (value, display_name) values
  ('onay',     'Onay ekranı'),
  ('sonradan', 'Parça detayı')
on conflict (value) do update set display_name = excluded.display_name;

insert into outfit_templates (value, display_name) values
  ('ust_alt',   'Üst ve alt'),
  ('tek_parca', 'Tek parça')
on conflict (value) do update set display_name = excluded.display_name;

insert into template_requirements (template_id, category_id, min_required)
select t.id, c.id, v.min_required
from (values
  (1, 'ust_alt',   'ust',       6),
  (2, 'ust_alt',   'alt',       5),
  (3, 'ust_alt',   'ayakkabi',  3),
  (4, 'ust_alt',   'dis',       2),
  (5, 'tek_parca', 'tek_parca', 5),
  (6, 'tek_parca', 'ayakkabi',  3),
  (7, 'tek_parca', 'dis',       2)
) as v(ord, template, category, min_required)
join outfit_templates t on t.value = v.template
join categories       c on c.value = v.category
order by v.ord
on conflict (template_id, category_id) do update set min_required = excluded.min_required;

-- ---------- Bağlam listeleri ----------

insert into times_of_day (value, display_name, start_hour, end_hour, is_daytime) values
  ('sabah', 'Sabah',  6, 10, true),
  ('ogle',  'Öğle',  11, 16, true),
  ('aksam', 'Akşam', 17, 20, false),
  ('gece',  'Gece',  21, 23, false)
on conflict (value) do update set display_name = excluded.display_name,
  start_hour = excluded.start_hour, end_hour = excluded.end_hour, is_daytime = excluded.is_daytime;

-- Yalnız Open-Meteo'nun gerçekten kullandığı WMO kodları (9 Kasım)
insert into weather_conditions (value, display_name, severity, is_sunny, is_wet, api_codes) values
  ('gunesli',         'Güneşli',         1, true,  false, '{0,1}'),
  ('parcali_bulutlu', 'Parçalı bulutlu', 2, false, false, '{2}'),
  ('kapali',          'Kapalı',          3, false, false, '{3}'),
  ('sisli',           'Sisli',           4, false, false, '{45,48}'),
  ('yagmurlu',        'Yağmurlu',        5, false, true,  '{51,53,55,56,57,61,63,65,66,67,80,81,82}'),
  ('karli',           'Karlı',           6, false, true,  '{71,73,75,77,85,86}'),
  ('firtinali',       'Fırtınalı',       7, false, true,  '{95,96,99}')
on conflict (value) do update set display_name = excluded.display_name, severity = excluded.severity,
  is_sunny = excluded.is_sunny, is_wet = excluded.is_wet, api_codes = excluded.api_codes;

-- ---------- Karşılaştırma listeleri ----------

insert into methods (value, display_name, description) values
  ('kural',       'Kural tabanlı',          'Renk çemberi kurallarıyla puanlama'),
  ('ogrenen',     'Öğrenen',                'Kullanıcı seçimlerinden lojistik regresyonla puanlama'),
  ('ortak_kural', 'Ortak aksesuar kuralı',  'İki yöntemin dışında, aksesuar seçen sabit kural')
on conflict (value) do update set display_name = excluded.display_name, description = excluded.description;

-- HICBIRI: acceptance boş, in_acceptance_analysis false (genel plan kararı)
insert into result_statuses
  (value, result_key, selected, acceptance, in_preference_analysis, in_acceptance_analysis, description)
values
  ('BEKLIYOR',             'BK', null,      null,           false, false, 'Kartlar gösterildi, tepki bekleniyor'),
  ('KURAL_SECILDI',        'KS', 'kural',   null,           true,  false, 'Kural kartı seçildi, karar bekleniyor'),
  ('OGRENEN_SECILDI',      'OS', 'ogrenen', null,           true,  false, 'Öğrenen kartı seçildi, karar bekleniyor'),
  ('KURAL_TEMIZ',          'KT', 'kural',   'temiz',        true,  true,  'Kural kombini seçildi, değişmeden beğenildi'),
  ('KURAL_DEGISIKLIKLE',   'KD', 'kural',   'degisiklikle', true,  true,  'Kural kombini seçildi, parça değişince beğenildi'),
  ('KURAL_RED',            'KR', 'kural',   'red',          true,  true,  'Kural kombini seçildi, sonunda beğenilmedi'),
  ('OGRENEN_TEMIZ',        'OT', 'ogrenen', 'temiz',        true,  true,  'Öğrenen kombini seçildi, değişmeden beğenildi'),
  ('OGRENEN_DEGISIKLIKLE', 'OD', 'ogrenen', 'degisiklikle', true,  true,  'Öğrenen kombini seçildi, parça değişince beğenildi'),
  ('OGRENEN_RED',          'OR', 'ogrenen', 'red',          true,  true,  'Öğrenen kombini seçildi, sonunda beğenilmedi'),
  ('HICBIRI',              'HB', 'yok',     null,           true,  false, 'İki kombin de reddedildi'),
  ('AYNI_TEMIZ',           'AT', 'ikisi',   'temiz',        false, false, 'Aynı öneri, değişmeden beğenildi'),
  ('AYNI_DEGISIKLIKLE',    'AD', 'ikisi',   'degisiklikle', false, false, 'Aynı öneri, parça değişince beğenildi'),
  ('AYNI_RED',             'AR', 'ikisi',   'red',          false, false, 'Aynı öneri, beğenilmedi'),
  ('TEPKISIZ',             'TP', null,      null,           false, false, 'Tepki verilmeden çıkıldı ya da ayarlar değişti')
on conflict (value) do update set result_key = excluded.result_key, selected = excluded.selected,
  acceptance = excluded.acceptance, in_preference_analysis = excluded.in_preference_analysis,
  in_acceptance_analysis = excluded.in_acceptance_analysis, description = excluded.description;

-- is_random: analizde modelin seçtiği kartları rastgele kartlardan ayırır (9 Kasım)
insert into learned_pick_modes (value, display_name, is_random) values
  ('model',           'Model',           false),
  ('kesif',           'Keşif',           true),
  ('soguk_baslangic', 'Soğuk başlangıç', true)
on conflict (value) do update set display_name = excluded.display_name, is_random = excluded.is_random;

-- ---------- Sayı kontrolü: bir satır eksikse işlem geri alınır ----------
do $$
declare
  expected constant jsonb := '{"categories":6,"styles":4,"seasons":4,"thicknesses":3,"colors":13,
    "tones":3,"clothing_statuses":3,"label_fields":7,"label_change_stages":2,"outfit_templates":2,
    "template_requirements":7,"times_of_day":4,"weather_conditions":7,"methods":3,
    "result_statuses":14,"learned_pick_modes":3}';
  t text; n int;
begin
  for t in select jsonb_object_keys(expected) loop
    execute format('select count(*) from %I', t) into n;
    if n <> (expected ->> t)::int then
      raise exception '%: % satır bekleniyordu, % var', t, expected ->> t, n;
    end if;
  end loop;
end $$;

commit;
