- Run background mode
sudo nohup python3 /home/ubuntu/auto_deploy.py &

sudo apt install zip
sudo apt install python3-pip
sudo pip install watchdog

- kill process
  1. >ps -ef(Get PID of python daemon)
  2. >kill <PID>