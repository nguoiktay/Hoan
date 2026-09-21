import struct
import numpy as np

# Let's inspect the world transform matrix of full_roads
from scan_scene_objects import transforms, go_to_trans, objects, get_world_matrix

full_roads_id = None
for go_id, go in objects.items():
    if go['name'] == 'full_roads':
        full_roads_id = go_id
        break

tid = go_to_trans[full_roads_id]
wm, chain = get_world_matrix(tid)
print("World matrix for full_roads:")
print(wm)

# Now load the vertices of full_roads from HoanKiemLakeAssets.fbx or parse_inspection
# In previous turn, we extracted vertices of full_roads to scratch or we can extract them directly.
# Let's inspect scratch/parse_inspection.py or read the FBX vertices:
import os

with open('Assets/AssetRipper/Lake/HoanKiemLakeAssets.fbx', 'rb') as f:
    fbx_data = f.read()

import re
# Find full_roads in FBX
idx = fbx_data.find(b'Model::full_roads')
print("Model::full_roads index:", idx)
# Find Vertices
v_idx = fbx_data.find(b'Vertices:', idx)
# Extract vertices
# In FBX ASCII or binary:
# Check if binary FBX: starts with b'Kaydara FBX Binary'
is_binary = fbx_data.startswith(b'Kaydara FBX Binary')
print("Is binary FBX:", is_binary)
