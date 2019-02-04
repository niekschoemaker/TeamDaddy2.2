from google.protobuf.internal.encoder import _VarintBytes
from google.protobuf.internal.decoder import _DecodeVarint32
from datetime import datetime
import WeatherData_pb2 as weather
import time
import threading
import mmap


global buffer
measurements = []
Stations = []
Start = "WeatherStations.dat"
class MyThread (threading.Thread):
        def __init__(self, threadID, name, counter):
                threading.Thread.__init__(self)
                self.ThreadID = threadID
                self.Name = name
                self.Counter = counter

        def run(self, command, inputs):
                #Dit wordt gedaan zodat het bestand zeker geopend is voor andere threads bezig gaan
                if self.ThreadID == 1:
                        threadlock = threading.Lock()
                        threadlock.acquire()
                        command(inputs)
                        threadlock.release()
                else:
                        command(inputs)
                
                print(self.Name,"Done")


def openProto(file):
    with open(file, 'rb',buffering=2000000) as f:
        m = mmap.mmap(f.fileno(), 0, access=mmap.ACCESS_READ)
        buf = m.read()
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
        measurements.append(read_daddy_measurement)

def parseDat(buf):
    n = 0
    while n < len(buf):
        msg_len, new_pos = _DecodeVarint32(buf, n)
        n = new_pos
        msg_buf = buf[n:n+msg_len]
        n += msg_len
        Daddy_WeatherStations = weather.WeatherStation()
        Daddy_WeatherStations.ParseFromString(msg_buf)
        Stations.append(Daddy_WeatherStations)


def getStation(name):
    Name = name.upper()
    i = 0
    while i < len(Stations):
        s = Stations[i].Name
        if s == Name:
            print(Stations[i].StationNumber)
            return Stations[i].StationNumber
            
        i += 1

def getHumidity(name):
    i = 0
    Answer = {}
    stationid = getStation(name)
    while i < len(measurements):
        s = measurements[i].StationID
        if s == stationid:
            time = convertDateTime(measurements[i].DateTime)
            Answer[time] = round(measurements[i].Humidity,2)
            
        i += 1
    
    for x, y in Answer.items():
        print(x,y)

    
def convertDateTime(Datetime):
    ts = Datetime
    x = datetime.fromtimestamp(ts).strftime("%Y-%m-%d %H:%M:%S")
    return x

def getWindspeed(name):
    i = 0
    Answer = {}
    stationid = getStation(name)
    while i < len(measurements):
        s = measurements[i].StationID
        if s == stationid:
            time = convertDateTime(measurements[i].DateTime)
            Answer[time] = round(measurements[i].WindSpeed,2)
            
        i += 1

    return Answer





def getTopten(Land):
    array = []
    array2 = []
    i = 0
    hallo = Land.upper()
    while i < len(Stations):
        Country = Stations[i].Country
        if Country == hallo:
            array.append(Stations[i].Humidity)
        i += 1
    print(array)
        

    
    #Return top 10 humid places in Czech
    pass
#Variabelen
t1 = time.time()
test = "Daddy-2019-1-31-14-21.pb"


#Threads aanmaken
OpenThread = MyThread(1,"OpenThread",2)
LeesThread = MyThread(2,"LeesThread",2)
OpdrachtThread = MyThread(3,"OpdrachtenThread",1)
OpdrachtThread1 = MyThread(4,"OpdrachtenThread2",1)
OpdrachtThread2 = MyThread(5,"OpdrachtenThread3",1)

#Threads starten
OpenThread.run(openProto,test)
LeesThread.run(parseProto,buffer)
t3 = time.time()
OpenThread.run(openProto,Start)
LeesThread.run(parseDat,buffer)

#OpdrachtThread.run(getStation,"jan mayen")
#OpdrachtThread1.run(getHumidity,"jan mayen")
#OpdrachtThread2.run(getWindspeed,"jan mayen")
#OpdrachtThread.run(getTopten,"Czech")
#time kijken
t2 = time.time()
print(t2-t1)
print(t3-t1)

#print(Stations)
#print(len(measurements))
#print(measurements[0].StationID)
#print(measurements[0].WindSpeed)
#print(measurements[0].DateTime)
#print(Stations[0].Country)
#ts= measurements[0].DateTime
    
#x = datetime.utcfromtimestamp(ts).strftime("%Y-%m-%d %H:%M:%S")
#measurements[0].DateTime = int(x)
#print (measurements[0].DateTime)