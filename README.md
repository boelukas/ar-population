# AR Population
Visualization of [GAMMA: The Wanderings of Odysseus in 3D Scenes](https://yz-cnsdqz.github.io/eigenmotion/GAMMA/) in AR on the hololens 2.

## Installation
The latest build can be found here: https://polybox.ethz.ch/index.php/s/KU0BVeec2ToCxlq.
To install, copy the build to the hololens with a USB stick or over the device portal.
On the holelens open the msix file.

To run the app, the [GAMMA server](https://github.com/boelukas/GAMMA-server) has to be set up. Clone the repository and install the python requirements using e.g conda and env.yml. Follow the installation instructions in the repo. Then run gamma-server.py and set up the necessary port forwardings.

## Usage
Start the app and open the hand menu by turning the palm of your hand upwards. Press on the server settings button to open the server menu. There the connection to the server can be setup and tested. To enter a different host address, point at the field and perform an air tap. If the test connection button is green, everything works. Alternatively one can edit the config.json file in the hololens device portal under LocalAppData.

Close the menu and watch around. The white lines visualize the spatial mesh. Air tap on the floor to create a way point, which will be visible as a green sphere. Air tap on a different part of the floor to create a second waypoint. Now a path was created. This path can be extended by any amount of way points. Now open the menu and click create animation. The path will turn yellow, which indicates that the application is waiting for a response form the gamma server. This can take up to 30 seconds. Afterwards an animation is generated and played. A different path can be created in the same way. 

With the destroy animation button the last generated animation is destroyed. The reset path button resets the current path if clicked at a wrong position. With the two switches, the spatial mesh visualization and the paths can be enabled and disabled.
## Project Build instructions
### Requirements
- Windows OS (Tested with Windows 11)
- Unity 2021.3.4f1
- Visual Studio 17.2.4
- [Gamma](https://github.com/yz-cnsdqz/GAMMA-release)
- Blender 2.93.9 (Optional)


