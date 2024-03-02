using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HammerHead
{
    public enum AppLayerProtocolTCP
    {
        Echo = 7,
        FTPdata = 20,
        FTP = 21,
        SSH = 22,
        Telnet = 23,
        SMTP = 25,
        WHOIS = 43,
        DNS = 53,
        Gopher = 70,
        Finger = 79,
        HTTP = 80,
        Kerberos = 88,
        POP3 = 110,
        Ident = 113,
        NNTP = 119,
        NetBIOSns = 137,
        NetBIOSdgm = 138,
        NetBIOSssn = 139,
        IMAP = 143,
        BGP = 179,
        LDAP = 389,
        SLP = 427,
        TLS = 443,
        FTPS = 989,
        IMAPS = 993,
        POP3S = 995

    }
    public enum AppLayerProtocolUDP : byte
    {

    }
}
