with open('Assets/scene_geometry_inspection.txt', 'r', encoding='utf-8', errors='ignore') as f:
    lines = f.readlines()

print("Objects on East shore (X in [650, 715], Z in [-760, -680]):")
for line in lines:
    if line.startswith('RENDERER:') and 'fish' not in line:
        parts = line.split(' | ')
        name = parts[0].replace('RENDERER: ', '').strip("'")
        path = parts[1].replace('Path: ', '').strip("'")
        center_str = parts[2].replace('Center: ', '').strip("()")
        size_str = parts[3].replace('Size: ', '').strip("()")
        try:
            cx, cy, cz = [float(v.strip()) for v in center_str.split(",")]
            sx, sy, sz = [float(v.strip()) for v in size_str.split(",")]
            if 650 <= cx <= 715 and -760 <= cz <= -680:
                print(f"'{name}' at ({cx:.1f}, {cy:.1f}, {cz:.1f}) size=({sx:.1f}, {sy:.1f}, {sz:.1f}) path='{path}'")
        except:
            pass
