from google.protobuf.internal.encoder import _VarintBytes
from google.protobuf.internal.decoder import _DecodeVarint32
from datetime import datetime
import WeatherData_pb2 as weather
import time
import _thread as thread
global buffer
#buffer = ''

def openProto(file):
    with open(file,'rb') as f:
        buf = f.read()
        f.close()
        global buffer
        buffer = buf
        
    
def parseProto(buf):
    n = 0
    while n < len(buf):
        msg_len, new_pos = _DecodeVarint32(buf, n)
        n = new_pos
        msg_buf = buf[n:n+msg_len]
        n += msg_len
        read_daddy_measurement = weather.Measurement()
        read_daddy_measurement.ParseFromString(msg_buf)

def getStation():
    #Return stationID
    pass

def getHumidity():
    #Return humidity
    pass

def getWindspeed():
    #Return windspeed
    pass

def getTopten():
    #Return top 10 humid places in Czech
    pass

#Hieronder werkt, hierboven staan functies om het eventueel multithreaded te krijgen. Deze moeten nog wel afgemaakt worden
#t1 = time.time()
#with open ('Daddy-2019-1-28-14-23.pb', 'rb') as f:
#    buf = f.read()
#    n = 0
#    
#    while n < len(buf):
#        msg_len, new_pos = _DecodeVarint32(buf, n)
#        n = new_pos
#        msg_buf = buf[n:n+msg_len]
#        n += msg_len
#        read_daddy_measurement = weather.Measurement()
#        read_daddy_measurement.ParseFromString(msg_buf)
#        #if (read_daddy_measurement == 949680):
#        print(read_daddy_measurement.StationID)



#ts= 1548690540
#t2 = time.time()
#print("Tijd benodigd: {0}".format(t2-t1))
     
#print(datetime.utcfromtimestamp(ts).strftime("%Y-%m-%d %H:%M:%S"))