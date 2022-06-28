# AR Population
Visualization of [GAMMA: The Wanderings of Odysseus in 3D Scenes](https://yz-cnsdqz.github.io/eigenmotion/GAMMA/) in AR on the hololens 2.

## Installation
### Requirements
- Blender 2.93.9
- Unity 2021.3.4f1
- Visual Studio 17.2.4
- [Gamma](https://github.com/yz-cnsdqz/GAMMA-release)

## Workflow
### Hololens
- Use hololens to walk around and scan environment
- Download scan from device protal:
  - Get ip by saying: "What's my ip" e.g 192.168.178.27
  - Go to https://192.168.178.27/#Home
  - Go to Views/3DViews/
  - Under Spatial Mapping click update to visualize the current spatial mesh and save to store .obj file.

### Blender
- File -> import -> Wavefront(.obj)
- Select all imported meshed and combine them with Ctr + J
- Change to the Scripting tab and drop the script inside the BlenderScripts directory ([script](BlenderScripts/extract_and_render_motion.py)) into the open panel
- Set `TRAJ_EXPORT` = True
- Set `TRAJ_EXPORT_PATH` to the directory where the path trajectories should be exported to
- Select the scan and switch to edit mode
- Hold Shift and select vertexes on the floor of the scan. This will be the path the human walks.
- Run the script. This should create a file *traj_xxxxx.pkl* in the `TRAJ_EXPORT_PATH` directory

### GAMMA
- In *exp_GAMMAPrimitive/gen_motion_long_in_Cubes.py*
- Change line 445, `dataset_path=` to `TRAJ_EXPORT_PATH`
- Change line 493, `resultdir=` to the path where gamma should store the results
- Run from the GAMMA root directory
``` 
python exp_GAMMAPrimitive/gen_motion_long_in_Cubes.py --cfg MPVAEPolicy_v0
```

### Blender
- Set `TRAJ_EXPORT` = False
- Set `GAMMA_RESULTS_PATH` to the directory where gamma exported the result .pkl files. Add **.pkl* at the end. *This is a subdirectory of `resultdir=` from gamma*.
- Change to Object Mode and select the Scene Collection Object. Run the script.
- Texture:
  - View: Activate Sidebar
  - Change to SMPLX tab
  - Select human in scene, then choose texture
  - Press set
- Export to fbx
  - Select Scan and motion objects 
  - File -> Export .fbx
  - Pathmode: copy, enable embed texcture buttom right next to select
  - Select Armature and Mesh
  - Check Apply Transform
  - Export

### Unity
- Assets/3DScans: Right click: Import new asset. Select blender export
- Drag into scene under MixedRealitySceneContent and in inspector go to select
- Go to Materials click Extract Textures
- Go to Animation, check Loop Time. Click apply.
- Add tag 3Dscan to the 3D scan object
- Add tag Human to all human objects
- Drag the 3 space pins from the GameObjects directory under the 3D scan object. Place them at door steps.
- Unpack all prefabs
- Set the orienter of the space pins to WLT_Adjustment
- Add the animator component to the parent of the scan object. Select the animator controller from Animators Directory.
- Open the controller and add the animation from the import.
- Build and Deploy on Hololens

