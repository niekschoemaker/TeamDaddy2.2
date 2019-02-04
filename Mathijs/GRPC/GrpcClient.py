import grpc
import time

# import the generated classes
import website_pb2
import website_pb2_grpc

# open grpc kanaal
channel = grpc.insecure_channel('localhost:50001')

# maak stub
stub = website_pb2_grpc.UNWDMIStub(channel)





#geef windsnelheid
def WindSpeedCzech(station):
    #maak stub
    vraagwind = website_pb2.windspeedrequest(station=station)
    reactie = stub.Windspeed(vraagwind)
    for i in reactie:
        print(i)

#geef humidity
def Humidity(station):
    vraaghumidity = website_pb2.humidity_request(station=station)
    reactie = stub.Humidity(vraaghumidity)
    for i in reactie:
        print(i)


#geef toptien
def top10(land):
    vraagtop10 = website_pb2.top10request(land=land)
    reactie_top10 = stub.toptien(vraagtop10)
    for i in reactie_top10:
        print(i)


Humidity("jan mayen")
WindSpeedCzech("jan mayen")

#top10(Czech) <-- werkt nog niet

