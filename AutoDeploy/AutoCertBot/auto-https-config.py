import os

os.system(f"sudo apt update")
os.system(f"sudo apt install snapd")
os.system(f"sudo snap install core; sudo snap refresh core")
os.system(f"sudo apt-get remove certbot")
os.system(f"sudo snap install --classic certbot")
os.system(f"sudo ln -s /snap/bin/certbot /usr/bin/certbot")
os.system(f"sudo certbot --nginx --noninteractive --agree-tos -m greentree619@outlook.com -d test.com")
os.system(f"sudo certbot renew --dry-run")
