// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: WeatherData.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace unwdmi.Protobuf {

  /// <summary>Holder for reflection information generated from WeatherData.proto</summary>
  public static partial class WeatherDataReflection {

    #region Descriptor
    /// <summary>File descriptor for WeatherData.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static WeatherDataReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChFXZWF0aGVyRGF0YS5wcm90bxILV2VhdGhlckRhdGEigAEKC01lYXN1cmVt",
            "ZW50EhEKCVN0YXRpb25JRBgBIAEoDRIQCghEYXRlVGltZRgCIAEoAxITCgtU",
            "ZW1wZXJhdHVyZRgDIAEoAhIQCghEZXdwb2ludBgEIAEoAhIRCglXaW5kU3Bl",
            "ZWQYBSABKAISEgoKQ2xvdWRDb3ZlchgGIAEoAiJ+Cg5XZWF0aGVyU3RhdGlv",
            "bhIVCg1TdGF0aW9uTnVtYmVyGAEgASgNEgwKBE5hbWUYAiABKAkSDwoHQ291",
            "bnRyeRgDIAEoCRIQCghMYXRpdHVkZRgEIAEoARIRCglMb25naXR1ZGUYBSAB",
            "KAESEQoJRWxldmF0aW9uGAYgASgBImQKB1JlcXVlc3QSLQoHY29tbWFuZBgB",
            "IAEoDjIcLldlYXRoZXJEYXRhLlJlcXVlc3QuQ29tbWFuZCIqCgdDb21tYW5k",
            "Eg8KC1dlYXRoZXJEYXRhEAASDgoKQ2xvdWRDb3ZlchABIg0KC1dlYXRoZXJE",
            "YXRhIgwKCkNsb3VkQ292ZXJCEqoCD3Vud2RtaS5Qcm90b2J1ZmIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::unwdmi.Protobuf.Measurement), global::unwdmi.Protobuf.Measurement.Parser, new[]{ "StationID", "DateTime", "Temperature", "Dewpoint", "WindSpeed", "CloudCover" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::unwdmi.Protobuf.WeatherStation), global::unwdmi.Protobuf.WeatherStation.Parser, new[]{ "StationNumber", "Name", "Country", "Latitude", "Longitude", "Elevation" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::unwdmi.Protobuf.Request), global::unwdmi.Protobuf.Request.Parser, new[]{ "Command" }, null, new[]{ typeof(global::unwdmi.Protobuf.Request.Types.Command) }, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::unwdmi.Protobuf.WeatherData), global::unwdmi.Protobuf.WeatherData.Parser, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::unwdmi.Protobuf.CloudCover), global::unwdmi.Protobuf.CloudCover.Parser, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Measurement : pb::IMessage<Measurement> {
    private static readonly pb::MessageParser<Measurement> _parser = new pb::MessageParser<Measurement>(() => new Measurement());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Measurement> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::unwdmi.Protobuf.WeatherDataReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Measurement() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Measurement(Measurement other) : this() {
      stationID_ = other.stationID_;
      dateTime_ = other.dateTime_;
      temperature_ = other.temperature_;
      dewpoint_ = other.dewpoint_;
      windSpeed_ = other.windSpeed_;
      cloudCover_ = other.cloudCover_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Measurement Clone() {
      return new Measurement(this);
    }

    /// <summary>Field number for the "StationID" field.</summary>
    public const int StationIDFieldNumber = 1;
    private uint stationID_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint StationID {
      get { return stationID_; }
      set {
        stationID_ = value;
      }
    }

    /// <summary>Field number for the "DateTime" field.</summary>
    public const int DateTimeFieldNumber = 2;
    private long dateTime_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long DateTime {
      get { return dateTime_; }
      set {
        dateTime_ = value;
      }
    }

    /// <summary>Field number for the "Temperature" field.</summary>
    public const int TemperatureFieldNumber = 3;
    private float temperature_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Temperature {
      get { return temperature_; }
      set {
        temperature_ = value;
      }
    }

    /// <summary>Field number for the "Dewpoint" field.</summary>
    public const int DewpointFieldNumber = 4;
    private float dewpoint_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Dewpoint {
      get { return dewpoint_; }
      set {
        dewpoint_ = value;
      }
    }

    /// <summary>Field number for the "WindSpeed" field.</summary>
    public const int WindSpeedFieldNumber = 5;
    private float windSpeed_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float WindSpeed {
      get { return windSpeed_; }
      set {
        windSpeed_ = value;
      }
    }

    /// <summary>Field number for the "CloudCover" field.</summary>
    public const int CloudCoverFieldNumber = 6;
    private float cloudCover_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float CloudCover {
      get { return cloudCover_; }
      set {
        cloudCover_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Measurement);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Measurement other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (StationID != other.StationID) return false;
      if (DateTime != other.DateTime) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Temperature, other.Temperature)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Dewpoint, other.Dewpoint)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(WindSpeed, other.WindSpeed)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(CloudCover, other.CloudCover)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (StationID != 0) hash ^= StationID.GetHashCode();
      if (DateTime != 0L) hash ^= DateTime.GetHashCode();
      if (Temperature != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Temperature);
      if (Dewpoint != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Dewpoint);
      if (WindSpeed != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(WindSpeed);
      if (CloudCover != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(CloudCover);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (StationID != 0) {
        output.WriteRawTag(8);
        output.WriteUInt32(StationID);
      }
      if (DateTime != 0L) {
        output.WriteRawTag(16);
        output.WriteInt64(DateTime);
      }
      if (Temperature != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(Temperature);
      }
      if (Dewpoint != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(Dewpoint);
      }
      if (WindSpeed != 0F) {
        output.WriteRawTag(45);
        output.WriteFloat(WindSpeed);
      }
      if (CloudCover != 0F) {
        output.WriteRawTag(53);
        output.WriteFloat(CloudCover);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (StationID != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(StationID);
      }
      if (DateTime != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(DateTime);
      }
      if (Temperature != 0F) {
        size += 1 + 4;
      }
      if (Dewpoint != 0F) {
        size += 1 + 4;
      }
      if (WindSpeed != 0F) {
        size += 1 + 4;
      }
      if (CloudCover != 0F) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Measurement other) {
      if (other == null) {
        return;
      }
      if (other.StationID != 0) {
        StationID = other.StationID;
      }
      if (other.DateTime != 0L) {
        DateTime = other.DateTime;
      }
      if (other.Temperature != 0F) {
        Temperature = other.Temperature;
      }
      if (other.Dewpoint != 0F) {
        Dewpoint = other.Dewpoint;
      }
      if (other.WindSpeed != 0F) {
        WindSpeed = other.WindSpeed;
      }
      if (other.CloudCover != 0F) {
        CloudCover = other.CloudCover;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            StationID = input.ReadUInt32();
            break;
          }
          case 16: {
            DateTime = input.ReadInt64();
            break;
          }
          case 29: {
            Temperature = input.ReadFloat();
            break;
          }
          case 37: {
            Dewpoint = input.ReadFloat();
            break;
          }
          case 45: {
            WindSpeed = input.ReadFloat();
            break;
          }
          case 53: {
            CloudCover = input.ReadFloat();
            break;
          }
        }
      }
    }

  }

  public sealed partial class WeatherStation : pb::IMessage<WeatherStation> {
    private static readonly pb::MessageParser<WeatherStation> _parser = new pb::MessageParser<WeatherStation>(() => new WeatherStation());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<WeatherStation> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::unwdmi.Protobuf.WeatherDataReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public WeatherStation() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public WeatherStation(WeatherStation other) : this() {
      stationNumber_ = other.stationNumber_;
      name_ = other.name_;
      country_ = other.country_;
      latitude_ = other.latitude_;
      longitude_ = other.longitude_;
      elevation_ = other.elevation_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public WeatherStation Clone() {
      return new WeatherStation(this);
    }

    /// <summary>Field number for the "StationNumber" field.</summary>
    public const int StationNumberFieldNumber = 1;
    private uint stationNumber_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint StationNumber {
      get { return stationNumber_; }
      set {
        stationNumber_ = value;
      }
    }

    /// <summary>Field number for the "Name" field.</summary>
    public const int NameFieldNumber = 2;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Country" field.</summary>
    public const int CountryFieldNumber = 3;
    private string country_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Country {
      get { return country_; }
      set {
        country_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Latitude" field.</summary>
    public const int LatitudeFieldNumber = 4;
    private double latitude_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public double Latitude {
      get { return latitude_; }
      set {
        latitude_ = value;
      }
    }

    /// <summary>Field number for the "Longitude" field.</summary>
    public const int LongitudeFieldNumber = 5;
    private double longitude_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public double Longitude {
      get { return longitude_; }
      set {
        longitude_ = value;
      }
    }

    /// <summary>Field number for the "Elevation" field.</summary>
    public const int ElevationFieldNumber = 6;
    private double elevation_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public double Elevation {
      get { return elevation_; }
      set {
        elevation_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as WeatherStation);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(WeatherStation other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (StationNumber != other.StationNumber) return false;
      if (Name != other.Name) return false;
      if (Country != other.Country) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Latitude, other.Latitude)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Longitude, other.Longitude)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(Elevation, other.Elevation)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (StationNumber != 0) hash ^= StationNumber.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (Country.Length != 0) hash ^= Country.GetHashCode();
      if (Latitude != 0D) hash ^= pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Latitude);
      if (Longitude != 0D) hash ^= pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Longitude);
      if (Elevation != 0D) hash ^= pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(Elevation);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (StationNumber != 0) {
        output.WriteRawTag(8);
        output.WriteUInt32(StationNumber);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      if (Country.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Country);
      }
      if (Latitude != 0D) {
        output.WriteRawTag(33);
        output.WriteDouble(Latitude);
      }
      if (Longitude != 0D) {
        output.WriteRawTag(41);
        output.WriteDouble(Longitude);
      }
      if (Elevation != 0D) {
        output.WriteRawTag(49);
        output.WriteDouble(Elevation);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (StationNumber != 0) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(StationNumber);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (Country.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Country);
      }
      if (Latitude != 0D) {
        size += 1 + 8;
      }
      if (Longitude != 0D) {
        size += 1 + 8;
      }
      if (Elevation != 0D) {
        size += 1 + 8;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(WeatherStation other) {
      if (other == null) {
        return;
      }
      if (other.StationNumber != 0) {
        StationNumber = other.StationNumber;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.Country.Length != 0) {
        Country = other.Country;
      }
      if (other.Latitude != 0D) {
        Latitude = other.Latitude;
      }
      if (other.Longitude != 0D) {
        Longitude = other.Longitude;
      }
      if (other.Elevation != 0D) {
        Elevation = other.Elevation;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            StationNumber = input.ReadUInt32();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 26: {
            Country = input.ReadString();
            break;
          }
          case 33: {
            Latitude = input.ReadDouble();
            break;
          }
          case 41: {
            Longitude = input.ReadDouble();
            break;
          }
          case 49: {
            Elevation = input.ReadDouble();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Request : pb::IMessage<Request> {
    private static readonly pb::MessageParser<Request> _parser = new pb::MessageParser<Request>(() => new Request());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Request> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::unwdmi.Protobuf.WeatherDataReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Request() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Request(Request other) : this() {
      command_ = other.command_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Request Clone() {
      return new Request(this);
    }

    /// <summary>Field number for the "command" field.</summary>
    public const int CommandFieldNumber = 1;
    private global::unwdmi.Protobuf.Request.Types.Command command_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::unwdmi.Protobuf.Request.Types.Command Command {
      get { return command_; }
      set {
        command_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Request);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Request other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Command != other.Command) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Command != 0) hash ^= Command.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Command != 0) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Command);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Command != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Command);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Request other) {
      if (other == null) {
        return;
      }
      if (other.Command != 0) {
        Command = other.Command;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            command_ = (global::unwdmi.Protobuf.Request.Types.Command) input.ReadEnum();
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the Request message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      public enum Command {
        [pbr::OriginalName("WeatherData")] WeatherData = 0,
        [pbr::OriginalName("CloudCover")] CloudCover = 1,
      }

    }
    #endregion

  }

  public sealed partial class WeatherData : pb::IMessage<WeatherData> {
    private static readonly pb::MessageParser<WeatherData> _parser = new pb::MessageParser<WeatherData>(() => new WeatherData());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<WeatherData> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::unwdmi.Protobuf.WeatherDataReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public WeatherData() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public WeatherData(WeatherData other) : this() {
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public WeatherData Clone() {
      return new WeatherData(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as WeatherData);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(WeatherData other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(WeatherData other) {
      if (other == null) {
        return;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
        }
      }
    }

  }

  public sealed partial class CloudCover : pb::IMessage<CloudCover> {
    private static readonly pb::MessageParser<CloudCover> _parser = new pb::MessageParser<CloudCover>(() => new CloudCover());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<CloudCover> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::unwdmi.Protobuf.WeatherDataReflection.Descriptor.MessageTypes[4]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CloudCover() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CloudCover(CloudCover other) : this() {
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public CloudCover Clone() {
      return new CloudCover(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as CloudCover);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(CloudCover other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(CloudCover other) {
      if (other == null) {
        return;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
