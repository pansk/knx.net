using KNXLib.Log;
using System.Collections.Concurrent;

namespace KNXLib;

internal abstract class KnxReceiver
{
    private static readonly string ClassName = typeof(KnxReceiver).ToString();

    private Thread _receiverThread;
    private Thread _consumerThread;

    private BlockingCollection<KnxDatagram> _rxDatagrams;

    protected KnxReceiver(KnxConnection connection)
    {
        KnxConnection = connection;
    }

    protected KnxConnection KnxConnection { get; }

    public abstract void ReceiverThreadFlow();

    private void ConsumerThreadFlow()
    {
        try
        {
            while (true)
            {
                KnxDatagram datagram = null;

                try
                {
                    if (_rxDatagrams.Any())
                        datagram = _rxDatagrams.Take();
                }
                catch (InvalidOperationException) { }

                if (datagram != null)
                    KnxConnection.Event(datagram.DestinationAddress, datagram.Data);
            }
        }
        catch (ThreadAbortException)
        {
            Thread.ResetAbort();
        }
    }

    public void Start()
    {
        _rxDatagrams = new BlockingCollection<KnxDatagram>();
        _consumerThread = new Thread(ConsumerThreadFlow) { Name = "KnxEventConsumerThread", IsBackground = true };
        _consumerThread.Start();

        _receiverThread = new Thread(ReceiverThreadFlow) { Name = "KnxReceiverThread", IsBackground = true };
        _receiverThread.Start();
    }

    public void Stop()
    {
        try
        {
            if (_receiverThread.ThreadState.Equals(ThreadState.Running))
            {
                _receiverThread.Abort();
            }
        }
        catch (ThreadAbortException)
        {
            Thread.ResetAbort();
        }
        try
        {
            if (_consumerThread.ThreadState.Equals(ThreadState.Running))
            {
                _consumerThread.Abort();
            }
        }
        catch (ThreadAbortException)
        {
            Thread.ResetAbort();
        }
        _rxDatagrams.Dispose();
    }

    protected void ProcessCemi(KnxDatagram datagram, byte[] cemi)
    {
        try
        {
            // CEMI
            // +--------+--------+--------+--------+----------------+----------------+--------+----------------+
            // |  Msg   |Add.Info| Ctrl 1 | Ctrl 2 | Source Address | Dest. Address  |  Data  |      APDU      |
            // | Code   | Length |        |        |                |                | Length |                |
            // +--------+--------+--------+--------+----------------+----------------+--------+----------------+
            //   1 byte   1 byte   1 byte   1 byte      2 bytes          2 bytes       1 byte      2 bytes
            //
            //  Message Code    = 0x11 - a L_Data.req primitive
            //      COMMON EMI MESSAGE CODES FOR DATA LINK LAYER PRIMITIVES
            //          FROM NETWORK LAYER TO DATA LINK LAYER
            //          +---------------------------+--------------+-------------------------+---------------------+------------------+
            //          | Data Link Layer Primitive | Message Code | Data Link Layer Service | Service Description | Common EMI Frame |
            //          +---------------------------+--------------+-------------------------+---------------------+------------------+
            //          |        L_Raw.req          |    0x10      |                         |                     |                  |
            //          +---------------------------+--------------+-------------------------+---------------------+------------------+
            //          |                           |              |                         | Primitive used for  | Sample Common    |
            //          |        L_Data.req         |    0x11      |      Data Service       | transmitting a data | EMI frame        |
            //          |                           |              |                         | frame               |                  |
            //          +---------------------------+--------------+-------------------------+---------------------+------------------+
            //          |        L_Poll_Data.req    |    0x13      |    Poll Data Service    |                     |                  |
            //          +---------------------------+--------------+-------------------------+---------------------+------------------+
            //          |        L_Raw.req          |    0x10      |                         |                     |                  |
            //          +---------------------------+--------------+-------------------------+---------------------+------------------+
            //          FROM DATA LINK LAYER TO NETWORK LAYER
            //          +---------------------------+--------------+-------------------------+---------------------+
            //          | Data Link Layer Primitive | Message Code | Data Link Layer Service | Service Description |
            //          +---------------------------+--------------+-------------------------+---------------------+
            //          |        L_Poll_Data.con    |    0x25      |    Poll Data Service    |                     |
            //          +---------------------------+--------------+-------------------------+---------------------+
            //          |                           |              |                         | Primitive used for  |
            //          |        L_Data.ind         |    0x29      |      Data Service       | receiving a data    |
            //          |                           |              |                         | frame               |
            //          +---------------------------+--------------+-------------------------+---------------------+
            //          |        L_Busmon.ind       |    0x2B      |   Bus Monitor Service   |                     |
            //          +---------------------------+--------------+-------------------------+---------------------+
            //          |        L_Raw.ind          |    0x2D      |                         |                     |
            //          +---------------------------+--------------+-------------------------+---------------------+
            //          |                           |              |                         | Primitive used for  |
            //          |                           |              |                         | local confirmation  |
            //          |        L_Data.con         |    0x2E      |      Data Service       | that a frame was    |
            //          |                           |              |                         | sent (does not mean |
            //          |                           |              |                         | successful receive) |
            //          +---------------------------+--------------+-------------------------+---------------------+
            //          |        L_Raw.con          |    0x2F      |                         |                     |
            //          +---------------------------+--------------+-------------------------+---------------------+

            //  Add.Info Length = 0x00 - no additional info
            //  Control Field 1 = see the bit structure above
            //  Control Field 2 = see the bit structure above
            //  Source Address  = 0x0000 - filled in by router/gateway with its source address which is
            //                    part of the KNX subnet
            //  Dest. Address   = KNX group or individual address (2 byte)
            //  Data Length     = Number of bytes of data in the APDU excluding the TPCI/APCI bits
            //  APDU            = Application Protocol Data Unit - the actual payload including transport
            //                    protocol control information (TPCI), application protocol control
            //                    information (APCI) and data passed as an argument from higher layers of
            //                    the KNX communication stack
            //
            datagram.MessageCode = cemi[0];
            datagram.AdditionalInfoLength = cemi[1];

            if (datagram.AdditionalInfoLength > 0)
            {
                datagram.AdditionalInfo = new byte[datagram.AdditionalInfoLength];
                for (var i = 0; i < datagram.AdditionalInfoLength; i++)
                {
                    datagram.AdditionalInfo[i] = cemi[2 + i];
                }
            }

            datagram.ControlField1 = cemi[2 + datagram.AdditionalInfoLength];
            datagram.ControlField2 = cemi[3 + datagram.AdditionalInfoLength];
            datagram.SourceAddress = KnxHelper.GetIndividualAddress(new[] { cemi[4 + datagram.AdditionalInfoLength], cemi[5 + datagram.AdditionalInfoLength] });

            datagram.DestinationAddress =
                KnxHelper.GetKnxDestinationAddressType(datagram.ControlField2).Equals(KnxHelper.KnxDestinationAddressType.INDIVIDUAL)
                    ? KnxHelper.GetIndividualAddress(new[] { cemi[6 + datagram.AdditionalInfoLength], cemi[7 + datagram.AdditionalInfoLength] })
                    : KnxHelper.GetGroupAddress(new[] { cemi[6 + datagram.AdditionalInfoLength], cemi[7 + datagram.AdditionalInfoLength] }, KnxConnection.ThreeLevelGroupAddressing);

            datagram.DataLength = cemi[8 + datagram.AdditionalInfoLength];
            datagram.Apdu = new byte[datagram.DataLength + 1];

            for (var i = 0; i < datagram.Apdu.Length; i++)
                datagram.Apdu[i] = cemi[9 + i + datagram.AdditionalInfoLength];

            datagram.Data = KnxHelper.GetData(datagram.DataLength, datagram.Apdu);

            if (KnxConnection.Debug)
            {
                Logger.Debug(ClassName, "-----------------------------------------------------------------------------------------------------");
                Logger.Debug(ClassName, BitConverter.ToString(cemi));
                Logger.Debug(ClassName, "Event Header Length: " + datagram.HeaderLength);
                Logger.Debug(ClassName, "Event Protocol Version: " + datagram.ProtocolVersion.ToString("x"));
                Logger.Debug(ClassName, "Event Service Type: 0x" + BitConverter.ToString(datagram.ServiceType).Replace("-", string.Empty));
                Logger.Debug(ClassName, "Event Total Length: " + datagram.TotalLength);

                Logger.Debug(ClassName, "Event Message Code: " + datagram.MessageCode.ToString("x"));
                Logger.Debug(ClassName, "Event Additional Info Length: " + datagram.AdditionalInfoLength);

                if (datagram.AdditionalInfoLength > 0)
                    Logger.Debug(ClassName, "Event Additional Info: 0x" + BitConverter.ToString(datagram.AdditionalInfo).Replace("-", string.Empty));

                Logger.Debug(ClassName, "Event Control Field 1: " + Convert.ToString(datagram.ControlField1, 2));
                Logger.Debug(ClassName, "Event Control Field 2: " + Convert.ToString(datagram.ControlField2, 2));
                Logger.Debug(ClassName, "Event Source Address: " + datagram.SourceAddress);
                Logger.Debug(ClassName, "Event Destination Address: " + datagram.DestinationAddress);
                Logger.Debug(ClassName, "Event Data Length: " + datagram.DataLength);
                Logger.Debug(ClassName, "Event APDU: 0x" + BitConverter.ToString(datagram.Apdu).Replace("-", string.Empty));
                Logger.Debug(ClassName, "Event Data: 0x" + string.Join("", datagram.Data.Select(c => ((int) c).ToString("X2"))));
                Logger.Debug(ClassName, "-----------------------------------------------------------------------------------------------------");
            }

            if (datagram.MessageCode != 0x29)
                return;

            var type = datagram.Apdu[1] >> 4;

            switch (type)
            {
                case 8:
                    _rxDatagrams.Add(datagram);
                    break;
                case 4:
                    KnxConnection.Status(datagram.DestinationAddress, datagram.Data);
                    break;
            }
        }
        catch (Exception e)
        {
            Logger.Error(ClassName, e);
        }
    }
}