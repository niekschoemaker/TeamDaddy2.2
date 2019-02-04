import grpc
from concurrent import futures
import time

# import the generated classes
import website_pb2
import website_pb2_grpc
import Testing


# create a class to define the server functions, derived from
class UNWDMI(website_pb2_grpc.UNWDMIServicer):

  # missing associated documentation comment in .proto file
    def windspeed(self, request, context):
        land = request.land
        print(land)

        #website_pb2.windspeedresponse.reactie = ("99")
        #print(website_pb2.windspeedresponse.reactie)

        for x, y in Testing.getWindspeed(land):
            result = {'tijd': str(x), 'windsnelheid': str(y)}
            print(result)
            yield website_pb2.windspeedresponse(**result)



    def toptien(self, request, context):
        top = ["Amsterdam", "Groningen","Praag", "Brussel","Parijs",
                   "Madrid","Barcelone", "Riga", "Oslo", "Tbilisi"]
        print(top)
        teverwerken = request.vraag
        result2 = {'nummer1': str(top[0]),
                      'nummer2': str(top[1]),
                      'nummer3': str(top[2]),
                      'nummer4': str(top[3]),
                      'nummer5': str(top[4]),
                      'nummer6': str(top[5]),
                      'nummer7': str(top[6]),
                      'nummer8': str(top[7]),
                      'nummer9': str(top[8]),
                      'nummer10': str(top[9])}
        print(result2)
        return website_pb2.top10response(**result2)





# create a gRPC server
server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))

# gebruik de gegenereerde fuctie 'add_windspeedserviceServicer_to_server(windspeedserviceServicer()'
# om de gedefineerde class aan de server toe te voegen
website_pb2_grpc.add_UNWDMIServicer_to_server(UNWDMI(), server)

# listen port 50000
print('Starting server. Listening on port 50000.')
server.add_insecure_port('127.0.0.1:50000')
server.start()

Windspeed("jan maven")
# since server.start() will not block,
# a sleep-loop is added to keep alive
try:
    while True:
        time.sleep(86400)
except KeyboardInterrupt:
    server.stop(0)
