# GAMMA-server Google Cloud Setup

- Create a new project
- Go to Compute Engine and to VM instances
- Create Instance
    - Region: Something close-by: e.g  europe-west3, zone: e.g a
    - Machine Family: General Purpose
    - Series: E2
    - Machine type: e2-medium (2 vCPU, 4GB memory)
    - Boot Disk:
        - Operating System: Ubuntu
        - Version: Ubuntu  22.04 LTS
        - Balanced persistent disk
        - Size 20 GB (At least 16 GB are needed)
    - Firewall: Allow HTTP and HTTPS traffic
    - Advanced options: Networking: Add a custom tag. e.g ar-population-instance
- Go to Set up firewall rules: Create Firewall Rule
    - Direction Traffic: Ingress
    - Action on match: Allow
    - Targets: Specified target tags
    - Target tags: Same custom tag. e.g ar-population-instance
    - Source IPv4 ranges: 0.0.0.0/0
    - Check the TCP box
    - Write 8080 in the Ports field
- Go to VM instances
- Log in to the vm instance with e.g the browser ssh window or with the gcloud command from the local terminal
- Install miniconda:
    - ```wget https://repo.anaconda.com/miniconda/Miniconda3-py38_4.12.0-Linux-x86_64.sh```
    - ```bash Miniconda3-py38_4.12.0-Linux-x86_64.sh```
- Clone the [Gamma-server repo](https://github.com/boelukas/GAMMA-server)
- Create conda environment: ```cd GAMMA-server && conda env create -f env.yml```
- ```conda activate gamma```
- Copy the checkpoints and extern dependencies to the repo: ```wget https://polybox.ethz.ch/index.php/s/FtM3i343RqdDldv/download -O checkpoints.zip```
- Unzip them such that results and extern are in the root directory of the repo (GAMMA-server/extern).
- run ```python gamma_server.py```
- To test if everything works, use ```curl -X POST -H "Content-Type: application/json" -d '[1, 2, 3, 2, 3, 4]' http://34.159.11.198:8080``` where the ip is the external ip from the VM displayed under VM instances. The server should start printing and respond with a json.
- Troubleshoot: If when executing the server an error with conversions occurs try changing the following lines to look like this in the file /home/--username--/miniconda3/envs/gamma/lib/python3.8/site-packages/torchgeometry/core/conversions.py:
```
mask_c0 = mask_d2 * mask_d0_d1
mask_c1 = mask_d2 * ~(mask_d0_d1)
mask_c2 = ~(mask_d2) * mask_d0_nd1
mask_c3 = ~(mask_d2) * ~(mask_d0_nd1)
```
- Troubleshoot: If there is no reaction from the server, the network settings of the VM might be the problem. Try in a new VM terminal to CURL the local ip address that is printed by the server. If the server reacts then, check the firewall settings of the VM.


# GAMMA-server local setup
If you have a e.g. desktop and use it as the server, you only need to do the following steps above:

- Install miniconda:
    - ```wget https://repo.anaconda.com/miniconda/Miniconda3-py38_4.12.0-Linux-x86_64.sh```
    - ```bash Miniconda3-py38_4.12.0-Linux-x86_64.sh```
- Clone the [Gamma-server repo](https://github.com/boelukas/GAMMA-server)
- Create conda environment: ```cd GAMMA-server && conda env create -f env.yml```
- ```conda activate gamma```
- Copy the checkpoints and extern dependencies to the repo: ```wget https://polybox.ethz.ch/index.php/s/FtM3i343RqdDldv/download -O checkpoints.zip```
- Unzip them such that results and extern are in the root directory of the repo (GAMMA-server/extern).
- run ```python gamma_server.py```



