# AR Population
Visualization of [GAMMA: The Wanderings of Odysseus in 3D Scenes](https://yz-cnsdqz.github.io/eigenmotion/GAMMA/) in AR on the hololens 2.

![](https://github.com/boelukas/ar-population/blob/main/demo.gif)

## Installation
The latest build together with demo videos can be found here: https://polybox.ethz.ch/index.php/s/KU0BVeec2ToCxlq.

To install, copy the build to the hololens with a USB stick or over the device portal.
On the holelens open the bundle with the App Installer.

To run the app, the [GAMMA server](https://github.com/boelukas/GAMMA-server) has to be set up. Clone the repository and install the python requirements using e.g conda and env.yml. Follow the installation instructions in the repo. Then run gamma-server.py and set up the necessary port forwardings.

## Usage
Start the app and open the hand menu by turning the palm of your hand upwards. Press on the server settings button to open the server menu. There the connection to the server can be setup and tested. To enter a different host address, point at the field and perform an air tap. If the test connection button is green, everything works. Alternatively one can edit the config.json file in the hololens device portal under LocalAppData/ar-population/LocalState/config.json

Close the menu and watch around. The white lines visualize the spatial mesh. Air tap on the floor to create a way point, which will be visible as a green sphere. Air tap on a different part of the floor to create a second waypoint. Now a path was created. This path can be extended by any amount of way points. Now open the menu and click create animation. The path will turn blue, which indicates that the application is waiting for a response form the gamma server. This can take up to 30 seconds. Afterwards an animation is generated and played. A different path can be created in the same way. 

With the destroy animation button the last generated animation is destroyed. The reset path button resets the current path if clicked at a wrong position. With the two switches, the spatial mesh visualization and the paths can be enabled and disabled.

The holograms are stored persistently (LocalAppData/ar-population/LocalState/animations) and locked to the place where they were created. To ensure this also works when bringing the hololens to a different place, the application should be closed with the close button of the menu.
## Project Build Instructions
### Requirements
- Windows OS (Tested with Windows 11)
- Unity 2021.3.4f1
- Visual Studio 17.2.4
    - .Net desktop development
    - Desktop development with C++
    - Universal Windows Platform (UWP) development
    - Game development with Unity
    - C++ (v143) Universal Windows Platform tools for UWP
- Blender 2.93.9 (Optional)

### Setup
- Clone this repository
- In unity hub > Projects > Open: Select the cloned repository
- Make sure the correct unity version is selected under Editor Version
- Open the Project
- Click on MixedReality > Project > Apply recommended project settings for Hololens 2
- Open the ar-population scene in Assets/Scenes

### Build
- Go to File > Build Settings
- Adjust the following settings
- Platform: Universal Windows Platform

|Setting Name|Value|
|------------|-----|
|Architecture|ARM 64 bit|
|Build Type|D3D Project|
|Target SDK|Latest installed|
|Minimum Platform Version|Default value|
|Visual Studio|Latest installed|
|Build and Run on|Remote Device(via Device Portal)|
|Build configuration|Release|
|Remaining points| Default values|

- Click Build and open the solution with visual studio
- Open Solution/ar-population/Package.appxmanifest
- Set the following capabilities:
  - Internet (Client & Server)
  - Internet (Client)
  - Microphone
  - Objects 3D
  - Private Networks (Client & Server)
  - Proximity
  - Remote System
  - Removable Storage
  - Spatial Perception
  - Webcam
- In solution configurations select Release - ARM64 - Remote Machine
- Go to Project > Properties > Configuration Properties > Debugging
    - Select Configuration Release
    - Set Machine Name to the IP address of the hololens: e.g 192.168.178.27
    - Apply
- To deploy the solution: Build > Deploy Solution
- To start remote debugging: Click "Play" Remote Machine
- To see debug.Log messages, use the debug configuration

### Packaging
- Right click on ar-population in the visual studio solution
- Publish > Create App Packages
    - Sideloading, no automatic updates, next
    - Yes, Use current certificate (Or create a custom one), next
    - Generate app bundle: Always. Only select ARM64 Release Architecture. Check Include public symbole files, create
    - The created appxbundle/msixbundle can be copied to the hololens
