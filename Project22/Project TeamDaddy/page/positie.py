import Testing

def getall(input):
    x = Testing.getAllthread(input)
    k = 0
    while k < len(x):
        print(x[k].StationNumber)
        print(x[k].Name)
        print(x[k].Country)
        print(x[k].Latitude)
        print(x[k].Longitude)
        print(x[k].Elevation)
        k += 1




getall("all")
