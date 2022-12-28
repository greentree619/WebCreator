import time
from watchdog.observers import Observer
from watchdog.events import PatternMatchingEventHandler
import os
import zipfile
from pathlib import Path

LANG_FLAG = 0x800               # bit 11 indicates UTF-8 for filenames
OS_FLAG = 3                     # 3 represents UNIX
FILEMODE = 0o100664             # filemode byte for -rw-rw-r--

# {{Global Setting Value
# targetPath = "D:\\Workstation\\AutoUploadPy\\path"
# screenshotPath = "D:\\Workstation\\AutoUploadPy\\screenshot\\"
#----------------------------------------------------------------
targetPath = "/home/ubuntu/"
extractPath = '/home/ubuntu/'
# }}Global Setting Value

with zipfile.ZipFile("/home/ubuntu/traepiller.net.zip", 'r') as ztf:
    ztf.comment = zf.comment
    for zinfo in zf.infolist():
        zinfo.CRC = None
        ztinfo = copy(zinfo)
        ztinfo.filename = zinfo.filename.encode('cp437').decode('gbk')
        ztinfo.flag_bits |= LANG_FLAG
        ztinfo.create_system = OS_FLAG
        ztinfo.external_attr = FILEMODE << 16
        ztf.writestr(ztinfo, zf.read(zinfo))