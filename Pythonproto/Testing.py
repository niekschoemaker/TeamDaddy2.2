from google.protobuf.internal.encoder import _VarintBytes
from google.protobuf.internal.decoder import _DecodeVarint32
from datetime import datetime
import WeatherData_pb2 as weather
import time
import threading
import mmap
t4 = time.time()
#Maakt hier globale variabelen aan waar alles ingelezen wordt.
global buffer
measurements = []
Stations = []

#Deze kan zo blijven staan.
Start = "WeatherStations.dat"
#Deze moet nog dynamic gemaakt worden.
Data = "Daddy-2019-2-3-01-23.pb"
#Data = 
#Class om threads mee te maken
class MyThread (threading.Thread):
        def __init__(self, threadID, name, counter):
                threading.Thread.__init__(self)
                self.ThreadID = threadID
                self.Name = name
                self.Counter = counter

        def run(self, command, inputs):
                
                x = command(inputs)
                print(self.Name,"Done")
                return (x)
#Threads aanmaken
OpenThread = MyThread(1,"OpenThread",2)
OpenThread2 = MyThread(2,"OpenThread2",2)
LeesThread = MyThread(3,"LeesThread",2)
LeesThread2 = MyThread(4,"LeesThread",2)
OpdrachtThread = MyThread(5,"OpdrachtenThread",1)
OpdrachtThread1 = MyThread(6,"OpdrachtenThread1",1)
OpdrachtThread2 = MyThread(7,"OpdrachtenThread2",1)
OpdrachtThread3 = MyThread(8,"OpdrachtThread3",1)
OpdrachtThread4 = MyThread(9,"OpdrachtThread4",1)
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

#Threads starten
t1 = time.time()
OpenThread.run(openProto,Data)
LeesThread.run(parseProto,buffer)
t3 = time.time()
OpenThread2.run(openProto,Start)
LeesThread2.run(parseDat,buffer)
print(t3-t1)


#Functie om getStation aan te roepen in een thread.
def StationIDthread(name):
    return OpdrachtThread3.run(getStationID,name)


#Functie om het stationid van een station op te halen aan de hand van de naam
def getStationID(name):
    Name = str(name).upper()
    i = 0
    while i < len(Stations):
        s = Stations[i].Name
        if s == Name:
            
            return Stations[i].StationNumber
            
        i += 1

def StationNamethread(ids):
    return OpdrachtThread4.run(getStationName, ids)
    
def getStationName(ids):
    ID = int(ids)
    i = 0
    for s in Stations:
        if s.StationNumber == ID:
            x = s.Name
            return(x)
    
    
    #while i < len(Stations):
    #    s = Stations[i].StationNumber
    #    if s == ID:
    #        x = Stations[i].Name
    #       return(x)


#Functie om getHumidity aan te roepen in een thread.
def Humiditythread(name):
    return OpdrachtThread2.run(getHumidity,name)


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
    
    return(Answer)


#Functie om de timestamp te converten naar de juiste tijd.
def convertDateTime(Datetime):
    ts = Datetime
    x = datetime.fromtimestamp(ts).strftime("%Y-%m-%d %H:%M:%S")
    return x

#Functie om getWindspeed aan te roepen in een thread.
def Windspeedthread(name):
    return OpdrachtThread1.run(getWindspeed,name)

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
    
    return(Answer)

#Functie om getTopten te starten met een thread, hierin wordt het bestand ook gelijk geopend door een andere thread
def Toptenthread(land):
    return OpdrachtThread.run(getTopten, "Random")
    
#Functie om de topten humid places op te halen.
def getTopten(Random):
    with open("TopTen.pb", "rb") as f:
        Daddy_TopTen = weather.TopTen()
        Daddy_TopTen.ParseFromString(f.read())

    return(Daddy_TopTen)
    #Return top 10 humid places in Czech
    


 

