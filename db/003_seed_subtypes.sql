-- =====================================================================
-- DolabımAI · 003_seed_subtypes.sql
-- 35 alt tür ve varsayılanları. 002'den sonra çalışır.
-- Kaynak: Ortak Kod Listesi belgesi, "Alt tür varsayılanları" tablosu
-- (7 Kasım değerleri). Onur'un D5 düzeltmeleri buraya işlenip dosya tekrar
-- çalıştırılır; id'ler değişmez.
--
-- Liste sütunları kodla yazılır, numaraya burada çevrilir. Yanlış yazılmış
-- bir kod (ör. 'gunluk' yerine 'gunlk') varsa işlem hata verip geri alınır.
-- =====================================================================

begin;

create temp table _subtypes (
  ord int, value text, display_name text, category text,
  styles text[], seasons text[], thickness text,
  min_temp int, max_temp int,
  avoid_when_wet boolean, preferred_when_wet boolean, preferred_when_hot boolean
) on commit drop;

insert into _subtypes values
-- ord value              display_name       kategori     tarz                         mevsim                                   kalınlık  min  max  ıslak kaçın / ıslak tercih / sıcak tercih
  ( 1, 'tisort',          'Tişört',          'ust',       '{gunluk,spor}',             '{ilkbahar,yaz}',                        'ince',     5,  40, false, false, false),
  ( 2, 'gomlek',          'Gömlek',          'ust',       '{is,davet}',                '{ilkbahar,sonbahar}',                   'ince',   -10,  28, false, false, false),
  ( 3, 'bluz',            'Bluz',            'ust',       '{gunluk,is,davet}',         '{ilkbahar,yaz,sonbahar}',               'ince',     0,  32, false, false, false),
  ( 4, 'kazak',           'Kazak',           'ust',       '{gunluk,is}',               '{sonbahar,kis}',                        'kalin',  -20,  15, false, false, false),
  ( 5, 'sweatshirt',      'Sweatshirt',      'ust',       '{gunluk,spor}',             '{ilkbahar,sonbahar,kis}',               'orta',   -10,  20, false, false, false),
  ( 6, 'kapusonlu',       'Kapüşonlu',       'ust',       '{gunluk,spor}',             '{ilkbahar,sonbahar,kis}',               'orta',   -10,  20, false, false, false),
  ( 7, 'atlet',           'Atlet',           'ust',       '{gunluk,spor}',             '{yaz}',                                 'ince',    22,  40, false, false, false),
  ( 8, 'crop_ust',        'Crop üst',        'ust',       '{gunluk}',                  '{ilkbahar,yaz}',                        'ince',    22,  40, false, false, false),
  ( 9, 'kot',             'Kot pantolon',    'alt',       '{gunluk}',                  '{ilkbahar,sonbahar,kis}',               'orta',   -20,  30, false, false, false),
  (10, 'kumas_pantolon',  'Kumaş pantolon',  'alt',       '{is,davet}',                '{ilkbahar,sonbahar,kis}',               'orta',   -20,  30, false, false, false),
  (11, 'esofman_alti',    'Eşofman altı',    'alt',       '{spor}',                    '{ilkbahar,sonbahar,kis}',               'orta',   -20,  25, false, false, false),
  (12, 'sort',            'Şort',            'alt',       '{gunluk,spor}',             '{yaz}',                                 'ince',    22,  40, false, false, false),
  (13, 'etek',            'Etek',            'alt',       '{gunluk,is,davet}',         '{ilkbahar,yaz,sonbahar}',               'ince',     5,  35, false, false, false),
  (14, 'tayt',            'Tayt',            'alt',       '{gunluk,spor}',             '{ilkbahar,sonbahar,kis}',               'ince',   -10,  28, false, false, false),
  (15, 'elbise',          'Elbise',          'tek_parca', '{gunluk,is,davet}',         '{ilkbahar,yaz}',                        'ince',     5,  38, false, false, false),
  (16, 'tulum',           'Tulum',           'tek_parca', '{gunluk,davet}',            '{ilkbahar,yaz}',                        'ince',     5,  32, false, false, false),
  (17, 'mont',            'Mont',            'dis',       '{gunluk,is,spor}',          '{kis}',                                 'kalin',  -20,  10, false, true,  false),
  (18, 'kaban',           'Kaban',           'dis',       '{gunluk,is,davet}',         '{sonbahar,kis}',                        'kalin',  -20,  12, false, false, false),
  (19, 'ceket',           'Ceket',           'dis',       '{gunluk,is}',               '{ilkbahar,sonbahar}',                   'orta',     8,  20, false, false, false),
  (20, 'blazer',          'Blazer',          'dis',       '{is,davet}',                '{ilkbahar,sonbahar}',                   'orta',    10,  24, false, false, false),
  (21, 'hirka',           'Hırka',           'dis',       '{gunluk,is}',               '{ilkbahar,sonbahar}',                   'orta',    10,  22, false, false, false),
  (22, 'yelek',           'Yelek',           'dis',       '{gunluk}',                  '{ilkbahar,sonbahar}',                   'orta',     8,  18, false, false, false),
  (23, 'yagmurluk',       'Yağmurluk',       'dis',       '{gunluk,is,spor}',          '{ilkbahar,sonbahar}',                   'ince',     5,  22, false, true,  false),
  (24, 'spor_ayakkabi',   'Spor ayakkabı',   'ayakkabi',  '{gunluk,spor}',             '{ilkbahar,yaz,sonbahar,kis}',           'ince',   -10,  40, false, false, true ),
  (25, 'klasik_ayakkabi', 'Klasik ayakkabı', 'ayakkabi',  '{is,davet}',                '{ilkbahar,yaz,sonbahar,kis}',           'orta',   -10,  32, false, false, false),
  (26, 'bot',             'Bot',             'ayakkabi',  '{gunluk,is}',               '{sonbahar,kis}',                        'kalin',  -25,  15, false, true,  false),
  (27, 'sandalet',        'Sandalet',        'ayakkabi',  '{gunluk}',                  '{yaz}',                                 'ince',    22,  45, true,  false, true ),
  (28, 'topuklu',         'Topuklu',         'ayakkabi',  '{is,davet}',                '{ilkbahar,yaz,sonbahar,kis}',           'ince',     5,  35, false, false, false),
  (29, 'babet',           'Babet',           'ayakkabi',  '{gunluk,is,davet}',         '{ilkbahar,yaz,sonbahar}',               'ince',    15,  35, true,  false, true ),
  (30, 'terlik',          'Terlik',          'ayakkabi',  '{gunluk}',                  '{yaz}',                                 'ince',    24,  45, true,  false, true ),
-- Aksesuar: tarz izin tablosundan (yalnız ekranda görünsün diye); sıcaklık -30..50 yüklenir, kullanılmaz
  (31, 'atki',            'Atkı',            'aksesuar',  '{gunluk,is,davet}',         '{sonbahar,kis}',                        'orta',   -30,  50, false, false, false),
  (32, 'bere',            'Bere',            'aksesuar',  '{gunluk,spor}',             '{sonbahar,kis}',                        'orta',   -30,  50, false, false, false),
  (33, 'eldiven',         'Eldiven',         'aksesuar',  '{gunluk,is,davet}',         '{kis}',                                 'orta',   -30,  50, false, false, false),
  (34, 'sapka',           'Şapka',           'aksesuar',  '{gunluk,spor}',             '{ilkbahar,yaz}',                        'ince',   -30,  50, false, false, false),
  (35, 'gunes_gozlugu',   'Güneş gözlüğü',   'aksesuar',  '{gunluk,is,spor}',          '{ilkbahar,yaz}',                        'ince',   -30,  50, false, false, false);

-- Bilinmeyen kod kontrolü
do $$
declare bad text;
begin
  select string_agg(format('%s: %s', s.value, x.code), ', ') into bad
  from _subtypes s
  cross join lateral (
    select 'kategori ' || s.category where not exists (select 1 from categories  where value = s.category)
    union all
    select 'kalınlık ' || s.thickness where not exists (select 1 from thicknesses where value = s.thickness)
    union all
    select 'tarz ' || st from unnest(s.styles)  st where not exists (select 1 from styles  where value = st)
    union all
    select 'mevsim ' || se from unnest(s.seasons) se where not exists (select 1 from seasons where value = se)
  ) as x(code);
  if bad is not null then
    raise exception 'subtypes: bilinmeyen kod(lar): %', bad;
  end if;
end $$;

insert into subtypes (value, display_name, category_id, default_style_ids, default_season_ids,
                      default_thickness_id, min_temp, max_temp,
                      avoid_when_wet, preferred_when_wet, preferred_when_hot)
select s.value, s.display_name, c.id,
       (select array_agg(id order by id) from styles  where value = any (s.styles)),
       (select array_agg(id order by id) from seasons where value = any (s.seasons)),
       t.id, s.min_temp, s.max_temp,
       s.avoid_when_wet, s.preferred_when_wet, s.preferred_when_hot
from _subtypes s
join categories  c on c.value = s.category
join thicknesses t on t.value = s.thickness
order by s.ord
on conflict (value) do update set
  display_name = excluded.display_name, category_id = excluded.category_id,
  default_style_ids = excluded.default_style_ids, default_season_ids = excluded.default_season_ids,
  default_thickness_id = excluded.default_thickness_id,
  min_temp = excluded.min_temp, max_temp = excluded.max_temp,
  avoid_when_wet = excluded.avoid_when_wet, preferred_when_wet = excluded.preferred_when_wet,
  preferred_when_hot = excluded.preferred_when_hot;

do $$
begin
  if (select count(*) from subtypes) <> 35 then
    raise exception 'subtypes: 35 satır bekleniyordu, % var', (select count(*) from subtypes);
  end if;
end $$;

commit;
