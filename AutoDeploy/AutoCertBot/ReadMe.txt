printf 'test.com\n' | python3 auto-https-config.py


pscp -i searchsystem.ppk auto-https-config.py ubuntu@3.14.14.86:/home/ubuntu/auto-https-config.py
pscp -i searchsystem.ppk auto-https-config.py ubuntu@3.131.110.136:/home/ubuntu/auto-https-config.py
pscp -i searchsystem.ppk auto-https-config.py ubuntu@3.142.69.221:/home/ubuntu/auto-https-config.py

pscp -i live-article-server.ppk auto-https-config.py ubuntu@52.53.170.58:/home/ubuntu/auto-https-config.py
pscp -i live-article-server.ppk auto-https-config.py ubuntu@54.241.151.2:/home/ubuntu/auto-https-config.py
pscp -i live-article-server.ppk auto-https-config.py ubuntu@54.215.134.13:/home/ubuntu/auto-https-config.py