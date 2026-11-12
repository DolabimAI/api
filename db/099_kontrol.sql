-- =====================================================================
-- DolabımAI · 099_kontrol.sql
-- Yalnız okur, hiçbir şey yazmaz. Seed'lerden sonra çalıştırılır.
-- =====================================================================

-- 1) Ortak kod listesi testi: üç listeyi düz metin olarak dışa al.
--    Onur ve Ömer kendi kodlarındaki listeyle karşılaştırır; fark sıfır olmalı.
select 'subtypes' as liste, string_agg(value, E'\n' order by id) as kodlar from subtypes
union all
select 'colors',  string_agg(value, E'\n' order by id) from colors
union all
select 'tones',   string_agg(value, E'\n' order by id) from tones;

-- 2) Her etkinlikte üst, alt ve ayakkabı için en az bir alt tür var mı? (boş satır dönmeli)
select st.value as etkinlik, cat.value as kategori
from styles st
cross join (select * from categories where value in ('ust', 'alt', 'ayakkabi')) cat
where not exists (
  select 1 from subtypes s
  where s.category_id = cat.id and st.id = any (s.default_style_ids)
);

-- 3) Dış giyim aralıkları -20..24 °C arasında boşluk bırakıyor mu? (boş satır dönmeli)
select t as sicaklik
from generate_series(-20, 24) t
where not exists (
  select 1 from subtypes s join categories c on c.id = s.category_id
  where c.value = 'dis' and t between s.min_temp and s.max_temp
);

-- 4) Kasım-Aralık havası (0..15 °C): her etkinlikte üst ve alt aday çıkıyor mu? (boş satır dönmeli)
select st.value as etkinlik, cat.value as kategori, t as sicaklik
from styles st
cross join (select * from categories where value in ('ust', 'alt')) cat
cross join generate_series(0, 15) t
where not exists (
  select 1 from subtypes s
  where s.category_id = cat.id and st.id = any (s.default_style_ids)
    and t between s.min_temp and s.max_temp
);

-- 5) Her ilin en az bir ilçesi var mı? (boş satır dönmeli)
select c.display_name from cities c
where not exists (select 1 from districts d where d.city_id = c.id);
