# Global Settings
TRAJ_EXPORT = False
TRAJ_EXPORT_PATH = 'C:\\Users\\Lukas\\Projects\\ar-population\\Data\\PathTrajectories'
GAMMA_RESULTS_PATH = 'C:\\Users\\Lukas\\Projects\\GAMMA-server\\GammaResults\\*.pkl'

N_TRAJECTORIES = 1

import bpy
import bmesh
import pdb
from mathutils import Vector, Quaternion, Matrix
from math import radians
import numpy as np
import os, glob
import pickle
import random

def export_trajectory():
    bm = bmesh.new()
    ob = bpy.context.active_object
    bm = bmesh.from_edit_mesh(ob.data)
    bm.verts.ensure_lookup_table()

    '''save manually selected points to list, keeping the selection order'''
    points = []
    #Show the selection order of vertices
    for e in bm.select_history:
        if isinstance(e, bmesh.types.BMVert) and e.select:
            obMat = ob.matrix_world
            ## transform to global coordinate from object coordinate
            points.append(obMat @ e.co)
            

    '''save the list to file'''
#    traj_outfolder = bpy.data.filepath.replace('.blend','_traj')
    traj_outfolder = TRAJ_EXPORT_PATH
    if not os.path.exists(traj_outfolder):
        os.makedirs(traj_outfolder)

    existing_trajs = sorted(glob.glob(os.path.join(traj_outfolder,'*.pkl')))
    print(existing_trajs)
    traj_idx = len(existing_trajs)
    outfilename = os.path.join(traj_outfolder, 'traj_{:05d}.pkl'.format(traj_idx))

    with open(outfilename, 'wb') as f:
        pickle.dump(np.array(points),f)

# ##### BEGIN GPL LICENSE BLOCK #####
#
#  This program is free software; you can redistribute it and/or
#  modify it under the terms of the GNU General Public License
#  as published by the Free Software Foundation; either version 2
#  of the License, or (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License 
#  along with this program; if not, write to the Free Software Foundation,
#  Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
#
# ##### END GPL LICENSE BLOCK #####


import bpy
import bmesh
from bpy_extras.io_utils import ImportHelper,ExportHelper # ImportHelper/ExportHelper is a helper class, defines filename and invoke() function which calls the file selector.
import pdb
from mathutils import Vector, Quaternion, Matrix
from math import radians
import numpy as np
import os
import pickle
import random
# from scipy.spatial.transform import Rotation as R

from bpy.props import ( BoolProperty, EnumProperty, FloatProperty, PointerProperty, StringProperty )
from bpy.types import ( PropertyGroup )

SMPLX_MODELFILE = "smplx_model_20210421.blend"
SMPLX_JOINT_NAMES = [
    'pelvis','left_hip','right_hip','spine1','left_knee','right_knee','spine2',
    'left_ankle','right_ankle','spine3', 'left_foot','right_foot','neck',
    'left_collar','right_collar','head','left_shoulder','right_shoulder','left_elbow',
    'right_elbow','left_wrist','right_wrist',
    'jaw','left_eye_smplhf','right_eye_smplhf','left_index1','left_index2','left_index3',
    'left_middle1','left_middle2','left_middle3','left_pinky1','left_pinky2','left_pinky3',
    'left_ring1','left_ring2','left_ring3','left_thumb1','left_thumb2',
    'left_thumb3','right_index1','right_index2','right_index3','right_middle1',
    'right_middle2','right_middle3','right_pinky1','right_pinky2','right_pinky3',
    'right_ring1','right_ring2','right_ring3','right_thumb1','right_thumb2','right_thumb3'
] #same to the definition in https://github.com/vchoutas/smplx/blob/master/smplx/joint_names.py

NUM_SMPLX_JOINTS = len(SMPLX_JOINT_NAMES)
NUM_SMPLX_BODYJOINTS = 21
NUM_SMPLX_HANDJOINTS = 15
ROT_NEGATIVE_X = Matrix(np.array([  [1.0000000,  0.0000000,  0.0000000],
                             [0.0000000,  0.0000000,  1.0000000],
                             [0.0000000, -1.0000000,  0.0000000]])
                        )
ROT_POSITIVE_Y = Matrix(np.array([  [-1.0000000, 0.0000000,  0.0000000],
                                    [0.0000000,  1.0000000,  0.0000000],
                                    [0.0000000,  0.0000000, -1.0000000]])
                        )
'''
note
    - rotation in pelvis is in the original smplx coordinate
    - rotation of the armature is in the blender coordinate
'''

FPS_SOURCE = 0
FPS_TARGET = 30
FPS_DOWNSAMPLE = 1


def aa2quaternion(aa):
    rod = Vector((aa[0], aa[1], aa[2]))
    angle_rad = rod.length
    axis = rod.normalized()
    return Quaternion(axis, angle_rad)


def rodrigues_from_pose(armature, bone_name):
    # Use quaternion mode for all bone rotations
    if armature.pose.bones[bone_name].rotation_mode != 'QUATERNION':
        armature.pose.bones[bone_name].rotation_mode = 'QUATERNION'

    quat = armature.pose.bones[bone_name].rotation_quaternion
    (axis, angle) = quat.to_axis_angle()
    rodrigues = axis
    rodrigues.normalize()
    rodrigues = rodrigues * angle
    return rodrigues



def set_pose_from_rodrigues(armature, bone_name, rodrigues, rodrigues_reference=None):
    rod = Vector((rodrigues[0], rodrigues[1], rodrigues[2]))
    angle_rad = rod.length
    axis = rod.normalized()

    if armature.pose.bones[bone_name].rotation_mode != 'QUATERNION':
        armature.pose.bones[bone_name].rotation_mode = 'QUATERNION'

    quat = Quaternion(axis, angle_rad)

    if rodrigues_reference is None:
        armature.pose.bones[bone_name].rotation_quaternion = quat
    else:
        rod_reference = Vector((rodrigues_reference[0], rodrigues_reference[1], rodrigues_reference[2]))
        rod_result = rod + rod_reference
        angle_rad_result = rod_result.length
        axis_result = rod_result.normalized()
        quat_result = Quaternion(axis_result, angle_rad_result)
        armature.pose.bones[bone_name].rotation_quaternion = quat_result

        """
        rod_reference = Vector((rodrigues_reference[0], rodrigues_reference[1], rodrigues_reference[2]))
        angle_rad_reference = rod_reference.length
        axis_reference = rod_reference.normalized()
        quat_reference = Quaternion(axis_reference, angle_rad_reference)

        # Rotate first into reference pose and then add the target pose
        armature.pose.bones[bone_name].rotation_quaternion = quat_reference @ quat
        """
    return




# Remove default cube
if 'Cube' in bpy.data.objects:
    bpy.data.objects['Cube'].select_set(True)
    bpy.ops.object.delete()

def animate_smplx_one_primitive(armature, scene, data, frame):
    smplx_params = data['smplx_params'][0]
    pelvis_locs = data['pelvis_loc']
    transf_rotmat = Matrix(data['transf_rotmat'].reshape(3,3))
    transf_transl = Vector(data['transf_transl'].reshape(3))
    n_frames_per_mp = 10
    if data['mp_type'] == '1-frame':
        ss=1
    elif data['mp_type'] == '2-frame':
        ss=2
    elif data['mp_type'] == 'start-frame':
        ss=0
    elif data['mp_type'] == 'target-frame':
        n_frames_per_mp = 1
    elif data['mp_type'] == 'humor':
        n_frames_per_mp = 300
        ss=0


    for t in range(ss,n_frames_per_mp):
        print('|-- processing frame {}...'.format(frame), end='\r')
        scene.frame_set(frame)
        transl = pelvis_locs[t].reshape(3)
        global_orient = np.array(smplx_params[t][3:6]).reshape(3)
        body_pose = np.array(smplx_params[t][6:69]).reshape(63).reshape(NUM_SMPLX_BODYJOINTS, 3)

        # Update body pose
        for index in range(NUM_SMPLX_BODYJOINTS):
            pose_rodrigues = body_pose[index]
            bone_name = SMPLX_JOINT_NAMES[index + 1] # body pose starts with left_hip
            set_pose_from_rodrigues(armature, bone_name, pose_rodrigues)

        # set global configurations
        ## set global orientation and translation at local coodinate
        if global_orient is not None:
            armature.rotation_mode = 'QUATERNION'
            global_orient_w = transf_rotmat @ (aa2quaternion(global_orient).to_matrix())
            armature.rotation_quaternion = global_orient_w.to_quaternion()


        if transl is not None:
            transl_w = transf_rotmat @  Vector(transl) + transf_transl
            armature.location = transl_w

        # Activate corrective poseshapes (update pose blend shapes)
        bpy.ops.object.smplx_set_poseshapes('EXEC_DEFAULT')

        # set the current status to a keyframe for animation
        armature.keyframe_insert('location', frame=frame)
        armature.keyframe_insert('rotation_quaternion', frame=frame)
        bones = armature.pose.bones
        for bone in bones:
            bone.keyframe_insert('rotation_quaternion', frame=frame)
        frame += 1

    return frame



def add_material_target(objname, color):
    mat = bpy.data.materials.new(objname)
    # # Add material slot to parachute mesh object (currently active)
    bpy.ops.object.material_slot_add()
    # Assign the material to the new material slot (currently active)
    obj = bpy.data.objects[objname]
    obj.active_material = mat
    # assign the texture
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes["Principled BSDF"]
    bsdf.inputs[0].default_value=(color[0],color[1],color[2],1.0)
    bsdf.inputs[5].default_value=0
    # bsdf.inputs[17].default_value=(0.8,0.503,0.009,1.0)
    bsdf.inputs[17].default_value=(color[0],color[1],color[2],1.0)




import math



def add_cylinder(p1, p2, radius, color):
    delta = p2 - p1
    dist = np.linalg.norm(delta)
    center = (p2 + p1) / 2
    bpy.ops.mesh.primitive_cylinder_add(
        radius=radius,
        depth=dist,
        location=center,
        rotation=(0, math.acos(delta[2]/dist), math.atan2(delta[1], delta[2])),
        vertices=8
    )
    cc = bpy.context.object
    add_material_target(cc.name, color)


def render_walk_points(wpath, collection_name=None):
    color=np.random.rand(3)
    for pt in wpath:
        bpy.ops.mesh.primitive_torus_add()
        torus = bpy.context.object
        torus.scale=Vector((0.5,0.5,0.1))
        torus.location=Vector((pt[0],pt[1],pt[2]+0.3))
        add_material_target(torus.name, color=color) 

        if collection_name is not None:
            collection = bpy.data.collections[collection_name+'_targets']
            master_collection = bpy.context.scene.collection
            collection.objects.link(torus) #link it with collection
            master_collection.objects.unlink(torus) #unlink it from master collection
    


def animate_smplx(filepath):

    print()
    print()

    '''create a new collection for the body and the target'''
    collection_name = "motions_{:05d}".format(random.randint(0,1000))
    collection_motion = bpy.data.collections.new(collection_name)
    bpy.context.scene.collection.children.link(collection_motion)
    collection_targets = bpy.data.collections.new(collection_name+'_targets')
    collection_motion.children.link(collection_targets)


    '''read search results'''
    with open(filepath, "rb") as f:
        dataall = pickle.load(f, encoding="latin1")
    print('read files and setup global info...')
    motiondata = dataall['motion']
    wpath = dataall['wpath']
    render_walk_points(wpath, collection_name)

    '''set keyframe range'''
    scene = bpy.data.scenes['Scene']
    scene.render.fps = FPS_TARGET
    scene.frame_end = 9*len(motiondata)

    '''add a smplx into blender context'''
    bpy.data.window_managers['WinMan'].smplx_tool.smplx_gender = str(motiondata[0]['gender'])
    bpy.ops.scene.smplx_add_gender()

    '''set global variables'''
    obj = bpy.context.object
    if obj.type == 'MESH':
        armature = obj.parent
    else:
        armature = obj
        obj = armature.children[0]
        bpy.context.view_layer.objects.active = obj # mesh needs to be active object for recalculating joint locations

    print('animate character: {}'.format(obj.name))
    collection_motion.objects.link(armature) #link it with collection
    bpy.context.scene.collection.objects.unlink(armature) #unlink it from master collection
    collection_motion.objects.link(obj) #link it with collection
    bpy.context.scene.collection.objects.unlink(obj) #unlink it from master collection


    '''update the body shape according to beta'''
    betas = np.array(motiondata[0]["betas"]).reshape(-1).tolist()
    bpy.ops.object.mode_set(mode='OBJECT')
    for index, beta in enumerate(betas):
        key_block_name = f"Shape{index:03}"
        if key_block_name in obj.data.shape_keys.key_blocks:
            obj.data.shape_keys.key_blocks[key_block_name].value = beta
        else:
            print(f"ERROR: No key block for: {key_block_name}")
    ## Update joint locations. This is necessary in this add-on when applying body shape.
    bpy.ops.object.smplx_update_joint_locations('EXEC_DEFAULT')
    print('|-- shape updated...')


    '''move the origin to the body pelvis, and rotate around x by -90degree'''
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.mode_set(mode='EDIT')
    deltaT = armature.pose.bones['pelvis'].head.z # the position at pelvis
    bpy.ops.object.mode_set(mode='POSE')
    armature.pose.bones['pelvis'].location.y -= deltaT
    armature.pose.bones['pelvis'].rotation_quaternion = ROT_NEGATIVE_X.to_quaternion()
    bpy.ops.object.mode_set(mode='OBJECT')


    '''update the body pose'''
    transl = None
    global_orient = None
    body_pose = None
    jaw_pose = None
    left_hand_pose = None
    right_hand_pose = None
    expression = None


    '''main loop to update body pose and insert keyframes'''
    frame = 0
    for data in motiondata:
        frame = animate_smplx_one_primitive(armature, scene, data, frame)

    print('|-- poses and keyframes updated...')
#    scene.frame_end = frame+10


import glob

if __name__ == '__main__':
    if TRAJ_EXPORT:
        export_trajectory()
    else:
    #   prefix = '/home/yzhang/workspaces/HumanMotionGen/MOJO-plus/results/tmp123/GAMMAVAEComboPolicy_PPO/MPVAEPolicy_v0/*.pkl'
        prefix = GAMMA_RESULTS_PATH
        filenames = sorted(glob.glob(prefix))

        for idx in range(0, N_TRAJECTORIES):
            filepath = filenames[idx]
            print(filepath)
            if os.path.exists(filepath):
                animate_smplx(filepath)

    