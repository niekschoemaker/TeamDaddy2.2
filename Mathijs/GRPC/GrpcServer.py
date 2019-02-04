import grpc
from concurrent import futures
import time
import Testing


# import the generated classes
import website_pb2
import website_pb2_grpc



# create een class om server functies te maken
class UNWDMI(website_pb2_grpc.UNWDMIServicer):

 #vraagt windspeed van testing.py op. vervolgens stuurt die het met een stream terug naar client
 def Windspeed(self, request, context):
    teverwerken = request.station
    #print(Testing.getWindspeed(teverwerken))
    wind_dict = Testing.getWindspeed(teverwerken)
    for key in wind_dict:
        #wordt in juiste protobuf format teruggestuurd
        result = {'tijd': key, 'windsnelheid': wind_dict[key]}
        #print(result)
        yield website_pb2.windspeedresponse(**result)

# Krijgt humidity in dictionary uit testing.py en stuurt deze in een stream terug
 def Humidity(self, request, context):
      teverwerken = request.station
      # print(Testing.getWindspeed(teverwerken))
      humidity_dict = Testing.getHumidity(teverwerken)
      for key in humidity_dict:
          result = {'tijd': key, 'humidity': humidity_dict[key]}
          # print(result)
          yield website_pb2.humidity_response(**result)

# ontvangt de top 10 uit testing.py en stuurt deze in 1 bericht terug.
 def toptien(self, request, context):
    print("wordt gecalled")
    print(request.land)
    top = Testing.getTopten("Czech")
    print(top)
    result = {'nummer1': str(top[0]),
                  'nummer2': str(top[1]),
                  'nummer3': str(top[2]),
                  'nummer4': str(top[3]),
                  'nummer5': str(top[4]),
                  'nummer6': str(top[5]),
                  'nummer7': str(top[6]),
                  'nummer8': str(top[7]),
                  'nummer9': str(top[8]),
                  'nummer10': str(top[9])}
    print(result)
    return website_pb2.top10response(**result)





# maak gRPC server
server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))

# gebruik de gegenereerde fuctie 'add_windspeedserviceServicer_to_server(windspeedserviceServicer()'
# om de gedefineerde class aan de server toe te voegen
website_pb2_grpc.add_UNWDMIServicer_to_server(UNWDMI(), server)

# listen port 50000
print('Starting server. Listening on port 50001.')

#localhost
server.add_insecure_port('[::]:50001')
server.start()


try:
    while True:
        time.sleep(86400)
except KeyboardInterrupt:
    server.stop(0)