import time
from watchdog.observers import Observer
from watchdog.events import PatternMatchingEventHandler

import zipfile
import os, shutil
import psutil
import subprocess
import signal
import threading
from time import sleep
from multiprocessing import Process
from subprocess import check_output
import multiprocessing
import sys
import auto_deploy

if __name__ == "__main__":
    print(f"deploy callback..." + sys.argv[1])
    #auto_deploy.clearContent(sys.argv[1])
    
    with zipfile.ZipFile(sys.argv[2], 'r') as zip_ref:
        zip_ref.extractall(sys.argv[1])
        
    try:
        os.remove(sys.argv[2])
    except:
        sleep(1)

    print(f"again main auto_deploy 1")
    #os.system("python auto_deploy.py");
    print(f"again main auto_deploy 2")
