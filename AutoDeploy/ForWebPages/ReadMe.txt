- Run background mode
sudo nohup python3 /home/ubuntu/auto_deploy.py &

sudo apt install zip
sudo apt install python3-pip
sudo pip install watchdog

- kill process
  1. >ps -ef(Get PID of python daemon)
  2. >kill <PID>


pscp -i searchsystem.ppk auto_deploy.py ubuntu@3.14.14.86:/home/ubuntu/auto_deploy.py
pscp -i searchsystem.ppk auto_deploy.py ubuntu@3.131.110.136:/home/ubuntu/auto_deploy.py
pscp -i searchsystem.ppk auto_deploy.py ubuntu@3.142.69.221:/home/ubuntu/auto_deploy.py

pscp -i live-article-server.ppk auto_deploy.py ubuntu@52.53.170.58:/home/ubuntu/auto_deploy.py
pscp -i live-article-server.ppk auto_deploy.py ubuntu@54.241.151.2:/home/ubuntu/auto_deploy.py
pscp -i live-article-server.ppk auto_deploy.py ubuntu@54.215.134.13:/home/ubuntu/auto_deploy.py