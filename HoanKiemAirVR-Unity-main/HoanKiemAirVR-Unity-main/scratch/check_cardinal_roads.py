import json

with open('Assets/road_geometry_calibrated.json', 'r', encoding='utf-8-sig') as f:
    d = json.load(f)

sectors = d['sectors']

# Let's inspect sectors by cardinal directions:
# Center is d['roadCenter']
cx, cy, cz = d['roadCenter']
print(f"Road Center: ({cx:.2f}, {cy:.2f}, {cz:.2f})")

# West: X < cx, angleDeg around 180 (150 to 210)
# East: X > cx, angleDeg around 0 / 360 (-30 to 30)
# South: Z < cz, angleDeg around 270 (240 to 300)
# North: Z > cz, angleDeg around 90 (60 to 120)

print("\n=== EAST (Dinh Tien Hoang): angleDeg around 0 ===")
for s in sectors:
    if s['angleDeg'] <= 25 or s['angleDeg'] >= 335:
        print(f"Angle {s['angleDeg']:5.1f}: Inner=({s['inner'][0]:6.1f}, {s['inner'][2]:6.1f}) -> Outer=({s['outer'][0]:6.1f}, {s['outer'][2]:6.1f}) | Width={s['width']:4.1f}m")

print("\n=== NORTH (Dong Kinh Nghia Thuc): angleDeg around 90 ===")
for s in sectors:
    if 65 <= s['angleDeg'] <= 115:
        print(f"Angle {s['angleDeg']:5.1f}: Inner=({s['inner'][0]:6.1f}, {s['inner'][2]:6.1f}) -> Outer=({s['outer'][0]:6.1f}, {s['outer'][2]:6.1f}) | Width={s['width']:4.1f}m")

print("\n=== WEST (Le Thai To): angleDeg around 180 ===")
for s in sectors:
    if 155 <= s['angleDeg'] <= 205:
        print(f"Angle {s['angleDeg']:5.1f}: Inner=({s['inner'][0]:6.1f}, {s['inner'][2]:6.1f}) -> Outer=({s['outer'][0]:6.1f}, {s['outer'][2]:6.1f}) | Width={s['width']:4.1f}m")

print("\n=== SOUTH (Hang Khay): angleDeg around 270 ===")
for s in sectors:
    if 245 <= s['angleDeg'] <= 295:
        print(f"Angle {s['angleDeg']:5.1f}: Inner=({s['inner'][0]:6.1f}, {s['inner'][2]:6.1f}) -> Outer=({s['outer'][0]:6.1f}, {s['outer'][2]:6.1f}) | Width={s['width']:4.1f}m")
