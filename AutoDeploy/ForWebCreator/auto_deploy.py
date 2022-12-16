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

# {{Global Setting Value
targetPath = "C:\\Users\\Administrator\\Desktop\\Upload"
extractFPath = 'C:\\inetpub\\wwwroot\\'
extractBPath = 'C:\\Users\\Administrator\\Desktop\\publish\\'
restart = False
selfpid = None
process_name = "cmd"
alreadyExist = False
# }}Global Setting Value

def clearContent(folder):
    for filename in os.listdir(folder):
        file_path = os.path.join(folder, filename)        
        try:
            if os.path.isfile(file_path) or os.path.islink(file_path):
                os.unlink(file_path)
            elif os.path.isdir(file_path):
                shutil.rmtree(file_path)
        except Exception as e:
            print('Failed to delete %s. Reason: %s' % (file_path, e))

def on_created(event):
    global alreadyExist
    
    print(f"hey, {event.src_path} has been created! ["+str(alreadyExist)+"]")
    time.sleep(1)
    #win-x64.zip, build.zip
    extractPath = extractFPath
    hotfile = 0
    print(event.src_path)
    if event.src_path.find("win-x64.zip") > 0 :
        extractPath = extractBPath
        hotfile = 1
    elif event.src_path.find("build.zip") > 0 :
        extractPath = extractFPath
        hotfile = 2
    print(extractPath)

    ##{{
    print("checking if readable file...")
    while True:   # repeat until the try statement succeeds
        try:
            the_zip_file = zipfile.ZipFile(event.src_path)
            ret = the_zip_file.testzip()

            if ret is not None:
                sleep(1)
                continue
            else:
                break
        except:
            sleep(1)
            pass
    print("create file okay!!!["+str(hotfile)+"]  ["+str(alreadyExist)+"]")
    sleep(2)
    ##}}

    if hotfile == 1 and alreadyExist == True:
        for proc in psutil.process_iter():
            if process_name in proc.name():
               pid = proc.pid
               if pid != selfpid:
                   print("this is dotnet process")
                   print("cmd.exe "+str(pid) + " - - " + str(process.pid))
                   os.kill(pid, signal.CTRL_C_EVENT)
                   sleep(10)
    elif hotfile == 2 or alreadyExist == False:
        if(hotfile == 2):
            clearContent(extractPath)
        with zipfile.ZipFile(event.src_path, 'r') as zip_ref:
            zip_ref.extractall(extractPath)

    try:
        os.remove(event.src_path)
        print("zip file removed!")
    except:
        sleep(1)
        print("zip file removed failed!")
    
    
    if hotfile == 1 and alreadyExist == False:
        print("Start ready after 10 s.")
        #os.kill(selfpid, signal.CTRL_C_EVENT)
        #process = Process(target=task)
        #process.start()
    print("Let restart manually")
        

def on_deleted(event):
    #print(f"what the f**k! Someone deleted {event.src_path}!")
    pass

def on_modified(event):
    #print(f"hey buddy,{event.event_type}-{event.is_directory}  {event.src_path} has been modified")
    on_created(event)
    pass

def on_moved(event):
    #print(f"ok ok ok, someone moved {event.src_path} to {event.dest_path}")
    pass

# create a new process
#process = Process(target=task)
# start the process
#process.start()
#sleep(5)
# wait for the process to finish
#process.terminate()
#print("process killed")
# try and start the process again
#process.start()
def task():
    global alreadyExist
    # report a message
    print("Hello from the new process")
    cmd = 'cd '+extractBPath + ' & "C:\\Program Files\\dotnet\\dotnet" ' + extractBPath+'UnitTest.dll --urls=http://0.0.0.0:555';
    print(cmd);
    try:
        alreadyExist = True
        print("Start - " + str(alreadyExist))
        os.system(cmd);
        print("End")
        alreadyExist = False
    except KeyboardInterrupt as error:
        print(repr(error), "!")
        os.system("python deploy-callback.py "+extractBPath+" "+targetPath+"\\win-x64.zip");
    print("Start - end" + str(alreadyExist))
        
def get_pid(name):
    return map(int,check_output(["pidof",name]).split())

if __name__ == "__main__":
    alreadyExist = os.path.exists(extractBPath + "UnitTest.dll")
    print(f"Hellow!, I'm fine...["+str(alreadyExist)+"]")
    
    for proc in psutil.process_iter():
        if process_name in proc.name():
           selfpid = proc.pid

    process = Process(target=task)
    process.start()
    
    patterns = "*"
    ignore_patterns = ""
    ignore_directories = False
    case_sensitive = True
    my_event_handler = PatternMatchingEventHandler(patterns, ignore_patterns, ignore_directories, case_sensitive)
    my_event_handler.on_created = on_created
    my_event_handler.on_deleted = on_deleted
    my_event_handler.on_modified = on_modified
    #my_event_handler.on_moved = on_moved

    path = targetPath
    go_recursively = True
    my_observer = Observer()
    my_observer.schedule(my_event_handler, path, recursive=go_recursively)

    my_observer.start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        my_observer.stop()
        my_observer.join()
