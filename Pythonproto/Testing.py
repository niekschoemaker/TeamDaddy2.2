from google.protobuf.internal.encoder import _VarintBytes
from google.protobuf.internal.decoder import _DecodeVarint32
from datetime import datetime
import WeatherData_pb2 as weather
import time
import threading
import mmap

#Maakt hier globale variabelen aan waar alles ingelezen wordt.
global buffer
measurements = []
Stations = []

#Deze kan zo blijven staan.
Start = "WeatherStations.dat"
#Deze moet nog dynamic gemaakt worden.
test = "Daddy-2019-1-31-14-21.pb"

#Class om threads mee te maken
class MyThread (threading.Thread):
        def __init__(self, threadID, name, counter):
                threading.Thread.__init__(self)
                self.ThreadID = threadID
                self.Name = name
                self.Counter = counter

        def run(self, command, inputs):
                
                command(inputs)
                print(self.Name,"Done")

#Functie voor het openen van .pb/.dat bestanden.
def openProto(file):
    with open(file, 'rb',buffering=2000000) as f:
        m = mmap.mmap(f.fileno(), 0, access=mmap.ACCESS_READ)
        buf = m.read()
        global buffer
        buffer = buf
        
#Functie voor het decrypten van .pb bestanden.    
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

#Functie voor het decrypten van .dat bestanden.
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

#Functie om getStation aan te roepen in een thread.
def Stationthread(name):
    OpdrachtThread3.run(getStation,name)


#Functie om het stationid van een station op te halen aan de hand van de naam
def getStation(name):
    Name = str(name).upper()
    i = 0
    while i < len(Stations):
        s = Stations[i].Name
        if s == Name:
            print(Stations[i].StationNumber)
            return Stations[i].StationNumber
            
        i += 1

#Functie om getHumidity aan te roepen in een thread.
def Humiditythread(name):
    OpdrachtThread2.run(getHumidity,name)


#Functie om Humidity op te halen van een station aan de hand van de naam.
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


#Functie om de timestamp te converten naar de juiste tijd.
def convertDateTime(Datetime):
    ts = Datetime
    x = datetime.fromtimestamp(ts).strftime("%Y-%m-%d %H:%M:%S")
    return x

#Functie om getWindspeed aan te roepen in een thread.
def Windspeedthread(name):
    OpdrachtThread1.run(getWindspeed,name)

#Functie om Windspeed op te halen van een station aan de hand van de naam.
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
    
    for x, y in Answer.items():
        print(x,y)

#Functie om getTopten te starten met een thread, hierin wordt het bestand ook gelijk geopend door een andere thread
def Toptenthread(land):
    OpenThread.run(openProto, "TopTen.pb")
    OpdrachtThread.run(getTopten, buffer)
    
#Functie om de topten humid places op te halen.
def getTopten(buf):
    n = 0
    while n < len(buf):
        msg_len, new_pos = _DecodeVarint32(buf, n)
        n = new_pos
        msg_buf = buf[n:n+msg_len]
        n += msg_len
        Daddy_topten = weather.Topten()
        Daddy_topten.ParseFromString(msg_buf)
        TopTen.append(Daddy_topten)

    return TopTen
    #Return top 10 humid places in Czech
    

t1 = time.time()

#Threads aanmaken
OpenThread = MyThread(1,"OpenThread",2)
OpenThread2 = MyThread(7,"OpenThread2",2)
LeesThread = MyThread(2,"LeesThread",2)
LeesThread2 = MyThread(8,"LeesThread",2)
OpdrachtThread = MyThread(3,"OpdrachtenThread1",1)
OpdrachtThread1 = MyThread(4,"OpdrachtenThread2",1)
OpdrachtThread2 = MyThread(5,"OpdrachtenThread3",1)
OpdrachtThread3 = MyThread(6,"OpdrachtThread4",1)

#Threads starten
OpenThread.run(openProto,test)
LeesThread.run(parseProto,buffer)
t3 = time.time()
OpenThread2.run(openProto,Start)
LeesThread2.run(parseDat,buffer)

#time kijken
t2 = time.time()
print(t2-t1)
print(t3-t1)

 

