using System.Net;
using KNXLib.Exceptions;

namespace KNXLib;

internal class KnxConnectionConfiguration
{
    public KnxConnectionConfiguration(string host, int port)
    {
        Host = host;
        Port = port;

        IpAddress = null;
        try
        {
            IpAddress = IPAddress.Parse(host);
        }
        catch
        {
            try
            {
                IpAddress = Dns.GetHostEntry(host).AddressList[0];
            }
            catch (Exception)
            {
                throw new InvalidHostException(host);
            }
        }

        if (IpAddress == null)
            throw new InvalidHostException(host);

        EndPoint = new IPEndPoint(IpAddress, port);
    }

    public string Host { get; }

    public int Port { get; }

    public IPAddress IpAddress { get; }

    public IPEndPoint EndPoint { get; }
}