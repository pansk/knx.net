using KNXLib.Log;
using System.Net;
using System.Net.Sockets;

namespace KNXLib;

internal class KnxReceiverRouting : KnxReceiver
{
    private static readonly string ClassName = typeof(KnxReceiverRouting).ToString();

    private readonly IList<UdpClient> _udpClients;

    internal KnxReceiverRouting(KnxConnection connection, IList<UdpClient> udpClients)
        : base(connection)
    {
        _udpClients = udpClients;
    }

    public override void ReceiverThreadFlow()
    {
        try
        {
            foreach (var client in _udpClients)
                client.BeginReceive(OnReceive, new object[] { client });

            // just wait to be aborted
            while (true)
                Thread.Sleep(60000);
        }
        catch (ThreadAbortException)
        {
            Thread.ResetAbort();
        }
        catch (Exception e)
        {
            Logger.Error(ClassName, e);
        }
    }

    private void OnReceive(IAsyncResult result)
    {
        IPEndPoint? endPoint = null;
        var args = result.AsyncState as object[];
        var session = args?[0] as UdpClient;

        try
        {
            var datagram = session?.EndReceive(result, ref endPoint);
            if (datagram != null)
                ProcessDatagram(datagram);

            // We make the next call to the begin receive
            session?.BeginReceive(OnReceive, args);
        }
        catch (ObjectDisposedException)
        {
            // ignore and exit, session was disposed
        }
        catch (Exception e)
        {
            Logger.Error(ClassName, e);
        }
    }

    private void ProcessDatagram(byte[] datagram)
    {
        try
        {
            ProcessDatagramHeaders(datagram);
        }
        catch (Exception e)
        {
            Logger.Error(ClassName, e);
        }
    }

    private void ProcessDatagramHeaders(byte[] datagram)
    {
        // HEADER
        var knxDatagram = new KnxDatagram
        {
            HeaderLength = datagram[0],
            ProtocolVersion = datagram[1],
            ServiceType = new[] { datagram[2], datagram[3] },
            TotalLength = datagram[4] + datagram[5]
        };

        var cemi = new byte[datagram.Length - 6];
        Array.Copy(datagram, 6, cemi, 0, datagram.Length - 6);

        ProcessCemi(knxDatagram, cemi);
    }
}