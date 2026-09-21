with open('Assets/scene_geometry_inspection.txt', 'r', encoding='utf-8', errors='ignore') as f:
    lines = f.readlines()

print("All streetlights on East side (X in [670, 750], Z in [-900, -600]):")
east_lights = []
for line in lines:
    if 'GRAPH/ROADSSIDE2/Light' in line:
        parts = line.split(' | ')
        center_str = parts[2].replace('Center: ', '').strip("()")
        cx, cy, cz = [float(v.strip()) for v in center_str.split(",")]
        if 670 <= cx <= 750 and -900 <= cz <= -600:
            east_lights.append((cx, cy, cz))

east_lights.sort(key=lambda p: p[2]) # sort by Z
for p in east_lights:
    print(f"Z={p[2]:6.1f} | X={p[0]:6.1f}, Y={p[1]:.2f}")
