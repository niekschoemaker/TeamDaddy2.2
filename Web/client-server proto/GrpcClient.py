import grpc

# import the generated classes
import website_pb2
import website_pb2_grpc

# open a gRPC channel
channel = grpc.insecure_channel('127.0.0.1:50000')

# create a stub (client)
stub = website_pb2_grpc.UNWDMIStub(channel)

# request message geven
vraagwind = website_pb2.windspeedrequest(land ='jan mayen')
vraagtop10 = website_pb2.top10request(vraag =1)

# vraag het daadwerkelijk en krijg de reactie
reactie_windsnelheid = stub.Windspeed(vraagwind)
reactie_top10 = stub.toptien(vraagtop10)



def WindSpeedCzech():
    print("ik ga nu wind van de server vragen")
    wind = reactie_windsnelheid
    print("gevraagd")
    print(wind)
    return wind

def top10():
    top10 = reactie_top10
    print("opgevraagd")
    print(top10)
    return top10

WindSpeedCzech()