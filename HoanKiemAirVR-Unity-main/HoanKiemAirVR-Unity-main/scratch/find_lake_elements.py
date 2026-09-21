import re
import numpy as np

# Let's inspect where the water is, where the lake shore is, and where the road is.
# In scene_geometry_inspection.txt:
with open('Assets/scene_geometry_inspection.txt', 'r', encoding='utf-8', errors='ignore') as f:
    lines = f.readlines()

print("--- WATER OBJECTS ---")
for line in lines:
    if 'Water' in line and 'RENDERER:' in line:
        print(line.strip())

print("\n--- BRIDGE OBJECTS ---")
for line in lines:
    if ('TheHuc' in line or 'Bridge' in line or 'Cau' in line) and 'RENDERER:' in line:
        print(line.strip())

print("\n--- TURTLE TOWER (THAP RUA) ---")
for line in lines:
    if ('Rua' in line or 'Turtle' in line or 'Tower' in line) and 'RENDERER:' in line:
        print(line.strip())
