with open('Assets/scene_geometry_inspection.txt', 'r', encoding='utf-8', errors='ignore') as f:
    lines = f.readlines()

print("=== NORTH LIGHTS (Z > -660, X in [560, 680]) ===")
for line in lines:
    if 'GRAPH/ROADSSIDE2/Light' in line:
        parts = line.split(' | ')
        center_str = parts[2].replace('Center: ', '').strip("()")
        cx, cy, cz = [float(v.strip()) for v in center_str.split(",")]
        if cz > -660 and 560 <= cx <= 680:
            print(f"North Light at ({cx:.1f}, {cy:.1f}, {cz:.1f})")

print("\n=== SOUTH LIGHTS (Z < -860, X in [560, 680]) ===")
for line in lines:
    if 'GRAPH/ROADSSIDE2/Light' in line:
        parts = line.split(' | ')
        center_str = parts[2].replace('Center: ', '').strip("()")
        cx, cy, cz = [float(v.strip()) for v in center_str.split(",")]
        if cz < -860 and 560 <= cx <= 680:
            print(f"South Light at ({cx:.1f}, {cy:.1f}, {cz:.1f})")
