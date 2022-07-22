netsh interface portproxy delete v4tov4 listenport=8080 listenaddress=192.168.178.55
netsh interface portproxy add v4tov4 listenport=8080 listenaddress=192.168.178.55 connectport=8080 connectaddress=$($(wsl hostname -I).Trim());
netsh interface portproxy show v4tov4
wsl /home/lukas/miniconda3/envs/gamma/bin/python /mnt/c/Users/Lukas/Projects/GAMMA-server/gamma_server.py