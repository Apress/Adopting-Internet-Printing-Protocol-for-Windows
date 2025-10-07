namespace WSDPrinterProbe
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class UdpState
    {
        public IPEndPoint e;
        public UdpClient u;

        public UdpState(IPEndPoint e, UdpClient u, CancellationTokenSource source)
        {
            this.e = e;
            this.u = u;
        }
    }
}

