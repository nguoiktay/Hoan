import os
import struct

fbx_path = 'Assets/Eloi_CityKit/Mesh/HoanKiemLake/HoanKiemLakeAssets.fbx'
with open(fbx_path, 'rb') as f:
    data = f.read()

print("FBX size:", len(data))
# Check what Model objects exist in the FBX:
import re
models = re.findall(b'Model::([a-zA-Z0-9_.-]+)', data)
unique_models = []
for m in models:
    name = m.decode('ascii', errors='ignore')
    if name not in unique_models:
        unique_models.append(name)

print("Unique models in FBX:", len(unique_models))
for name in unique_models:
    print(" -", name)
