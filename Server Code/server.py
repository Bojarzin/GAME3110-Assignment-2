import socket
import time
from _thread import *
import threading
from datetime import datetime
import json
from enum import IntEnum

def main():
    port = 12345
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.bind(('', port))
    ##_thread.start_new_thread(gameLoop, (s,))
    ##_thread.start_new_thread(messageLoop, (s,))

    while True:
        time.sleep(1)

if __name__ == '__main__':
    main()