"""Builds 004_seed_locations.sql from open data.
Sources:
  - turkiye-api (MIT): 81 il (adı, plaka, merkez koordinatı) + 973 ilçe adı/il bağlantısı
  - geoBoundaries TUR ADM1/ADM2 (OpenStreetMap, ODbL): il/ilçe sınırları
  - GeoNames cities1000 (CC BY 4.0, reverse_geocoder paketinden): yerleşim noktaları
İlçe koordinatı: adı ilçeyle aynı olan ve ilçe sınırının İÇİNDE kalan GeoNames noktası (ilçe merkezi);
bulunamazsa ilçe sınırının iç noktası (point_on_surface).
"""
import json, csv, sys, unicodedata, re
from shapely.geometry import shape, Point
from shapely.prepared import prep

API, ADM1, ADM2, GN, OUT = sys.argv[1:6]

TR = str.maketrans({'ç':'c','ğ':'g','ı':'i','ö':'o','ş':'s','ü':'u','â':'a','î':'i','û':'u',
                    'Ç':'c','Ğ':'g','I':'i','İ':'i','Ö':'o','Ş':'s','Ü':'u','Â':'a','Î':'i','Û':'u'})
def norm(s):
    s = s.translate(TR).lower()
    s = unicodedata.normalize('NFKD', s).encode('ascii','ignore').decode()
    return re.sub(r'[^a-z0-9]+', '', s)
def code(s):
    s = s.translate(TR).lower()
    s = unicodedata.normalize('NFKD', s).encode('ascii','ignore').decode()
    return re.sub(r'[^a-z0-9]+', '_', s).strip('_')

prov = json.load(open(f'{API}/provinces.json'))
dist = json.load(open(f'{API}/districts.json'))
assert len(prov) == 81, len(prov)

adm1 = json.load(open(ADM1))['features']
adm2 = json.load(open(ADM2))['features']

# il sınırlarını il numarasına eşle (ad üzerinden)
pid_by_norm = {norm(p['name']): p['id'] for p in prov}
p_shapes = {}
for f in adm1:
    k = norm(f['properties']['shapeName'])
    assert k in pid_by_norm, f['properties']['shapeName']
    p_shapes[pid_by_norm[k]] = prep(shape(f['geometry']))
assert len(p_shapes) == 81

# ilçe sınırlarını ile ata
d_shapes = {}  # (pid, norm name) -> geometry
unassigned = []
for f in adm2:
    g = shape(f['geometry'])
    rp = g.representative_point()
    pid = next((i for i, ps in p_shapes.items() if ps.contains(rp)), None)
    if pid is None:
        unassigned.append(f['properties']['shapeName']); continue
    d_shapes[(pid, norm(f['properties']['shapeName']))] = g

# GeoNames noktaları
gn = {}
with open(GN, newline='', encoding='utf-8') as fh:
    for r in csv.DictReader(fh):
        if r['cc'] == 'TR':
            gn.setdefault(norm(r['name']), []).append(Point(float(r['lon']), float(r['lat'])))

# OSM'de farklı yazılan ilçeler: norm(api adı) -> norm(osm adı)
ALIASES = {'gokceada': 'imbros', 'adalar': 'princeislands', 'gediz': 'gedizmerkez', 'karakecili': 'karakeceli'}
pname = {p['id']: norm(p['name']) for p in prov}
def osm_key(pid, n):
    if n == 'merkez':  # OSM: '<il> Merkez', '<il> Merkez İlçe', '<il> District' ya da yalnız '<il>'
        for cand in (pname[pid] + 'merkez', pname[pid] + 'merkezi', pname[pid] + 'merkezilce', pname[pid] + 'district', pname[pid]):
            if (pid, cand) in d_shapes: return (pid, cand)
    return (pid, ALIASES.get(n, n))
rows, missing, src = [], [], {'merkez': 0, 'sinir': 0}
for d in dist:
    pid, n = d['provinceId'], norm(d['name'])
    g = d_shapes.get(osm_key(pid, n))
    if g is None:
        missing.append(f"{pid} {d['name']}"); continue
    gname = pname[pid] if n == 'merkez' else n  # Merkez ilçenin merkezi, ilin kendisidir
    pt = next((p for p in gn.get(gname, []) if g.contains(p)), None)
    if pt is not None: src['merkez'] += 1
    else:
        pt = g.representative_point(); src['sinir'] += 1
    rows.append((pid, code(d['name']), d['name'], round(pt.y, 5), round(pt.x, 5)))

print('ilce:', len(dist), 'eslesen:', len(rows), 'kaynak:', src, file=sys.stderr)
print('ile atanamayan sinir:', unassigned, file=sys.stderr)
print('eslesmeyen:', missing, file=sys.stderr)
if missing: sys.exit(1)

# il içinde tekil kod kontrolü
seen = set()
for pid, v, *_ in rows:
    assert (pid, v) not in seen, (pid, v); seen.add((pid, v))

q = lambda s: "'" + s.replace("'", "''") + "'"
with open(OUT, 'w', encoding='utf-8') as o:
    o.write("-- 004_seed_locations.sql\n-- 81 il ve %d ilçe. build_locations.py ile üretildi; elle düzenlemeyin.\n" % len(rows))
    o.write("-- Kaynaklar: turkiye-api (MIT) il/ilçe listesi ve il merkezleri; ilçe merkezi GeoNames (CC BY 4.0),\n")
    o.write("-- bulunamayan %d ilçe için OpenStreetMap sınırının iç noktası (geoBoundaries, ODbL).\n" % src['sinir'])
    o.write("-- Tekrar çalıştırılabilir.\n\nbegin;\n\n")
    o.write("insert into cities (value, display_name, plate_code, latitude, longitude) values\n")
    o.write(",\n".join(f"  ({q(code(p['name']))}, {q(p['name'])}, {p['id']}, {round(p['coordinates']['latitude'],5)}, {round(p['coordinates']['longitude'],5)})"
                       for p in sorted(prov, key=lambda p: p['id'])))
    o.write("\non conflict (value) do update set display_name = excluded.display_name, plate_code = excluded.plate_code,\n"
            "  latitude = excluded.latitude, longitude = excluded.longitude;\n\n")
    o.write("insert into districts (city_id, value, display_name, latitude, longitude)\n"
            "select c.id, v.value, v.display_name, v.latitude, v.longitude\nfrom (values\n")
    rows.sort(key=lambda r: (r[0], r[2]))
    o.write(",\n".join(f"  ({pid}, {q(v)}, {q(dn)}, {la}, {lo})" for pid, v, dn, la, lo in rows))
    o.write("\n) as v(plate_code, value, display_name, latitude, longitude)\n"
            "join cities c on c.plate_code = v.plate_code\n"
            "on conflict (city_id, value) do update set display_name = excluded.display_name,\n"
            "  latitude = excluded.latitude, longitude = excluded.longitude;\n\n")
    o.write(f"""do $$
begin
  if (select count(*) from cities) <> 81 then raise exception 'cities: 81 satır bekleniyordu'; end if;
  if (select count(*) from districts) <> {len(rows)} then raise exception 'districts: {len(rows)} satır bekleniyordu'; end if;
end $$;

commit;
""")
