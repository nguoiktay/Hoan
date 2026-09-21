import re
import numpy as np

def quat_to_rot_matrix(q):
    x, y, z, w = q
    # Normalize quaternion
    n = np.sqrt(x*x + y*y + z*z + w*w)
    if n > 0:
        x, y, z, w = x/n, y/n, z/n, w/n
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

with open('Assets/Scenes/HoanKiem.unity', 'r', encoding='utf-8', errors='ignore') as f:
    text = f.read()

def get_transform(trans_id):
    pattern = r'--- !u!4 &' + trans_id + r'\s+Transform:\s+.*?m_GameObject: \{fileID: (\d+)\}.*?m_LocalRotation: \{x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+), w: ([-\d.e]+)\}\s+m_LocalPosition: \{x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+)\}\s+m_LocalScale: \{x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+)\}.*?m_Father: \{fileID: ([-\d]+)\}'
    m = re.search(pattern, text, re.DOTALL)
    if m:
        go_id = m.group(1)
        go_m = re.search(r'--- !u!1 &' + go_id + r'\s+GameObject:.*?m_Name: ([^\r\n]+)', text, re.DOTALL)
        go_name = go_m.group(1) if go_m else 'Unknown'
        return {
            'id': trans_id,
            'name': go_name,
            'rot': [float(m.group(2)), float(m.group(3)), float(m.group(4)), float(m.group(5))],
            'pos': [float(m.group(6)), float(m.group(7)), float(m.group(8))],
            'scale': [float(m.group(9)), float(m.group(10)), float(m.group(11))],
            'father': m.group(12)
        }
    return None

curr = '5153352799148587639'
chain = []
while curr and curr != '0':
    t = get_transform(curr)
    if not t:
        break
    chain.append(t)
    curr = t['father']

# chain is [full_roads, Decor, Lake, GRAPH]
# World matrix = M_GRAPH @ M_Lake @ M_Decor @ M_full_roads
world_matrix = np.eye(4)
for t in reversed(chain):
    M = make_trs_matrix(t['pos'], t['rot'], t['scale'])
    world_matrix = world_matrix @ M

print("Compound World Matrix for full_roads:")
print(world_matrix)

# Let's test a few sample points from full_roads FBX:
# In the FBX raw coordinates:
# West road: X ≈ 555, Y ≈ -0.49, Z ≈ -750
# East road: X ≈ 688, Y ≈ -0.49, Z ≈ -750
# South road: X ≈ 620, Y ≈ -0.49, Z ≈ -934
# North road: X ≈ 620, Y ≈ -0.49, Z ≈ -610

def to_world(local_pt):
    p = np.array([local_pt[0], local_pt[1], local_pt[2], 1.0])
    w = world_matrix @ p
    return w[:3]

print("\n--- SAMPLE FBX LOCAL POINTS TRANSFORMED TO WORLD SPACE ---")
pts = [
    ("West Road (FBX 555, -0.49, -750)", [555, -0.49, -750]),
    ("East Road (FBX 688, -0.49, -750)", [688, -0.49, -750]),
    ("South Road (FBX 620, -0.49, -934)", [620, -0.49, -934]),
    ("North Road (FBX 620, -0.49, -610)", [620, -0.49, -610]),
    ("Lake Center (FBX 620, -0.49, -770)", [620, -0.49, -770])
]

for name, pt in pts:
    w = to_world(pt)
    print(f"{name} -> World: X={w[0]:.2f}, Y={w[1]:.2f}, Z={w[2]:.2f}")
