# TeamDaddy2.2

## File Definitions

The protocol buffer file (WeatherData.proto) can be found in /Protobuf/WeatherData.proto
This can be compiled with protoc.bat (change the csharp_out to whichever language you want.
For C# you need to compile the resulting WeatherData.cs file and copy the build result (Protobuf.dll) over to the lib folder.

**TopTen.pb**
  Contains one message as defined by Protobuf.TopTen (WeatherData.proto -> message TopTen)
  Has no delimiter.
  
**./Data/Daddy-{yyyy-M-d-HH-m}.pb**
  Contains the averages of every weather station over the minute in the file name.
  Uses data in format as defined by Protobuf.Measurement (WeatherData.proto -> message Measurement)
  Has Delimiter in front of every protocol buffer.
