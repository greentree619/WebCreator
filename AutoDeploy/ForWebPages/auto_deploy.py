import time
from watchdog.observers import Observer
from watchdog.events import PatternMatchingEventHandler
import os
import zipfile

# {{Global Setting Value
# targetPath = "D:\\Workstation\\AutoUploadPy\\path"
# screenshotPath = "D:\\Workstation\\AutoUploadPy\\screenshot\\"
#----------------------------------------------------------------
targetPath = "/home/ubuntu/"
extractPath = '/var/www/html/'
# }}Global Setting Value

def on_created(event):
    #print(f"hey, {event.src_path} has been created!")
    time.sleep(5)
	##{{
    #print("checking if readable file...")
    while True:   # repeat until the try statement succeeds
        try:
            the_zip_file = zipfile.ZipFile(event.src_path)
            ret = the_zip_file.testzip()

            if ret is not None:
                time.sleep(1)
                continue
            else:
                break
        except:
            time.sleep(1)
            pass
    #print("create file okay!!!")
    ##}}

    with zipfile.ZipFile(event.src_path, 'r') as zip_ref:
        zip_ref.extractall(extractPath)
    os.remove(event.src_path)


def on_deleted(event):
    #print(f"what the f**k! Someone deleted {event.src_path}!")
    pass

def on_modified(event):
    #print(f"hey buddy,{event.event_type}-{event.is_directory}  {event.src_path} has been modified")
    pass

def on_moved(event):
    #print(f"ok ok ok, someone moved {event.src_path} to {event.dest_path}")
    pass

if __name__ == "__main__":
    #print(f"Hellow!, I'm fine...")
    patterns = "*"
    ignore_patterns = ""
    ignore_directories = False
    case_sensitive = True
    my_event_handler = PatternMatchingEventHandler(patterns, ignore_patterns, ignore_directories, case_sensitive)
    my_event_handler.on_created = on_created
    my_event_handler.on_deleted = on_deleted
    #my_event_handler.on_modified = on_modified
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
