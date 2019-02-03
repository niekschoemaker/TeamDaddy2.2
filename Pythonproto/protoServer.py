

import socket
import os
import _thread
import threading
def saveProto(data):
    pass

def processProto(data):
    #PROCESS

    pass

def receiveProto(HOST,PORT):
    with socket.socket(socket.AF_INET,socket.SOCK_STREAM) as sock:
        while True:
            print("Listening")
            sock.bind((HOST, PORT))
            sock.listen()
            conn = sock.accept()
            with conn:
                data = conn.recv(2048)
                if data > 0:
                    _thread.start_new_thread(processProto,(data))
                    _thread.exit_thread()
                else:
                    break

class MyThread (threading.Thread):
        def __init__(self, threadID, name, counter):
                threading.Thread.__init__(self)
                self.ThreadID = threadID
                self.Name = name
                self.Counter = counter

        def run(self, command, inputs):
                command(inputs[0],inputs[1])
                print(self.Name,"Done")

OpenThread = MyThread(1,"OpenThread",2)
OpenThread.run(receiveProto,("Localhost", 25565))
#receiveProto("Localhost", 25565)
