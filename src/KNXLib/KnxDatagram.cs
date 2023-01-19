namespace KNXLib;

internal class KnxDatagram
{
    // HEADER
    public int HeaderLength;
    public byte ProtocolVersion;
    public byte[] ServiceType;
    public int TotalLength;

    // CONNECTION
    public byte ChannelId;
    public byte Status;

    // CEMI
    public byte MessageCode;
    public int AdditionalInfoLength;
    public byte[] AdditionalInfo;
    public byte ControlField1;
    public byte ControlField2;
    public string SourceAddress;
    public string DestinationAddress;
    public int DataLength;
    public byte[] Apdu;
    public string Data;
}