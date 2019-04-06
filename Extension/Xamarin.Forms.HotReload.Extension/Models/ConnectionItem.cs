using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Xamarin.Forms.HotReload.Extension.Models
{
    [DataContract]
    public class ConnectionItem
    {
        private static readonly Regex StrongIpRegex =
            new Regex(
                "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");

        public ConnectionItem(string protocol, string ipAddress, string port)
        {
            Protocol = protocol;
            IpAddress = ipAddress;
            Port = port;
        }

        [DataMember]
        public string Protocol { get; set; }

        [DataMember]
        public string IpAddress { get; set; }

        [DataMember]
        public string Port { get; set; }

        public string FullAddress => $"{Protocol}://{IpAddress}:{Port}";

        public string[] AvailableProtocols => SharedGlobals.DefaultAvailableProtocolsValue;

        public bool IsValid => StrongIpRegex.IsMatch(IpAddress);

        public override string ToString()
        {
            return FullAddress;
        }
    }
}