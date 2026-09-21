import re

targets = ['VNPT', 'Player', 'Camera', 'ThapRua', 'NgocSon', 'TheHuc', 'Water', 'HoanKiemWater', 'full_roads', 'HoanKiem_GreenRunningTrack', 'HoanKiem_RoadMarkings', 'ToaSoan_BaoHanoiMoi', 'ToaNha_HamCaMap', 'KhachSan_Apricot_Hotel']

# Scan the YAML scene file quickly
current_id = None
current_type = None
objects = {} # id -> dict
transforms = {} # id -> dict

print("Scanning scene...")
with open('Assets/Scenes/HoanKiem.unity', 'r', encoding='utf-8', errors='ignore') as f:
    for line in f:
        if line.startswith('--- !u!'):
            parts = line.strip().split()
            current_type = parts[1] # e.g. !u!1 or !u!4
            current_id = parts[2][1:] # e.g. 5153352799148587639
            if current_type == '!u!1':
                objects[current_id] = {'name': '', 'components': []}
            elif current_type == '!u!4':
                transforms[current_id] = {'go': None, 'pos': [0,0,0], 'rot': [0,0,0,1], 'scale': [1,1,1], 'father': '0'}
        elif current_type == '!u!1' and 'm_Name:' in line:
            objects[current_id]['name'] = line.split('m_Name:')[1].strip()
        elif current_type == '!u!4':
            if 'm_GameObject:' in line:
                m = re.search(r'fileID: (\d+)', line)
                if m: transforms[current_id]['go'] = m.group(1)
            elif 'm_LocalPosition:' in line:
                m = re.search(r'x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+)', line)
                if m: transforms[current_id]['pos'] = [float(m.group(1)), float(m.group(2)), float(m.group(3))]
            elif 'm_LocalRotation:' in line:
                m = re.search(r'x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+), w: ([-\d.e]+)', line)
                if m: transforms[current_id]['rot'] = [float(m.group(1)), float(m.group(2)), float(m.group(3)), float(m.group(4))]
            elif 'm_LocalScale:' in line:
                m = re.search(r'x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+)', line)
                if m: transforms[current_id]['scale'] = [float(m.group(1)), float(m.group(2)), float(m.group(3))]
            elif 'm_Father:' in line:
                m = re.search(r'fileID: ([-\d]+)', line)
                if m: transforms[current_id]['father'] = m.group(1)

print(f"Total GameObjects: {len(objects)}, Total Transforms: {len(transforms)}")

# Map go to transform
go_to_trans = {}
for tid, t in transforms.items():
    if t['go']:
        go_to_trans[t['go']] = tid

import numpy as np
def quat_to_rot_matrix(q):
    x, y, z, w = q
    n = np.sqrt(x*x + y*y + z*z + w*w)
    if n > 0: x, y, z, w = x/n, y/n, z/n, w/n
    return np.array([
        [1 - 2*(y*y + z*z),     2*(x*y - z*w),     2*(x*z + y*w)],
        [    2*(x*y + z*w), 1 - 2*(x*x + z*z),     2*(y*z - x*w)],
        [    2*(x*z - y*w),     2*(y*z + x*w), 1 - 2*(x*x + y*y)]
    ])

def make_trs_matrix(pos, rot, scale):
    M = np.eye(4)
    R = quat_to_rot_matrix(rot)
    S = np.diag(scale)
    M[:3, :3] = R @ S
    M[:3, 3] = pos
    return M

def get_world_matrix(tid):
    chain = []
    curr = tid
    while curr and curr != '0':
        if curr not in transforms:
            break
        chain.append(transforms[curr])
        curr = transforms[curr]['father']
    
    world_m = np.eye(4)
    for t in reversed(chain):
        M = make_trs_matrix(t['pos'], t['rot'], t['scale'])
        world_m = world_m @ M
    return world_m, chain

for go_id, go in objects.items():
    name = go['name']
    for target in targets:
        if target.lower() in name.lower():
            tid = go_to_trans.get(go_id)
            if tid:
                wm, chain = get_world_matrix(tid)
                w_pos = wm[:3, 3]
                hierarchy = " -> ".join([objects.get(t['go'], {}).get('name', t['go']) for t in reversed(chain)])
                print(f"[FOUND] '{name}': WorldPos=({w_pos[0]:.2f}, {w_pos[1]:.2f}, {w_pos[2]:.2f}) | Hierarchy: {hierarchy}")
            else:
                print(f"[FOUND] '{name}' (No transform)")
            break
