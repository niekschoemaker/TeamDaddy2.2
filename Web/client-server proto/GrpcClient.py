import grpc

# import the generated classes
import website_pb2
import website_pb2_grpc

# open a gRPC channel
channel = grpc.insecure_channel('13.81.171.134:50000')

# create a stub (client)
stub = website_pb2_grpc.UNWDMIStub(channel)

# request message geven
vraagwind = website_pb2.windspeedrequest(vraag = 1)
vraagtop10 = website_pb2.top10request(vraag = 1)

# vraag het daadwerkelijk en krijg de reactie
reactie_windsnelheid = stub.Windspeed(vraagwind)
reactie_top10 = stub.toptien(vraagtop10)


def WindSpeedCzech():
    wind = reactie_windsnelheid
    print(wind)
    return (wind)

def top10():
    top10 = reactie_top10
    print("opgevraagd")
    print(top10)
    return top10

top10()
WindSpeedCzech()