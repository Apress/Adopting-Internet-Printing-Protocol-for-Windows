using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Multi_Check
{
    public static class Defines
    {
        // Although AF_UNSPEC is defined for backwards compatibility, using
        // AF_UNSPEC for the "af" parameter when creating a socket is STRONGLY
        // DISCOURAGED.  The interpretation of the "protocol" parameter
        // depends on the actual address family chosen.  As environments grow
        // to include more and more address families that use overlapping
        // protocol values there is more and more chance of choosing an
        // undesired address family when AF_UNSPEC is used.
        //
        public const uint AF_UNSPEC = 0;               // unspecified
        public const uint AF_UNIX = 1;                 // local to host (pipes, portals)
        public const uint AF_INET = 2;                 // internetwork: UDP, TCP, etc.
        public const uint AF_IMPLINK = 3;              // arpanet imp addresses
        public const uint AF_PUP = 4;                  // pup protocols: e.g. BSP
        public const uint AF_CHAOS = 5;                // mit CHAOS protocols
        public const uint AF_NS = 6;                   // XEROX NS protocols
        public const uint AF_IPX = AF_NS;              // IPX protocols: IPX, SPX, etc.
        public const uint AF_ISO = 7;                  // ISO protocols
        public const uint AF_OSI = AF_ISO;             // OSI is ISO
        public const uint AF_ECMA = 8;                 // european computer manufacturers
        public const uint AF_DATAKIT = 9;              // datakit protocols
        public const uint AF_CCITT = 10;               // CCITT protocols, X.25 etc
        public const uint AF_SNA = 11;                 // IBM SNA
        public const uint AF_DECnet = 12;              // DECnet
        public const uint AF_DLI = 13;                 // Direct data link interface
        public const uint AF_LAT = 14;                 // LAT
        public const uint AF_HYLINK = 15;              // NSC Hyperchannel
        public const uint AF_APPLETALK = 16;           // AppleTalk
        public const uint AF_NETBIOS = 17;             // NetBios-style addresses
        public const uint AF_VOICEVIEW = 18;           // VoiceView
        public const uint AF_FIREFOX = 19;             // Protocols from Firefox
        public const uint AF_UNKNOWN1 = 20;            // Somebody is using this!
        public const uint AF_BAN = 21;                 // Banyan
        public const uint AF_ATM = 22;                 // Native ATM Services
        public const uint AF_INET6 = 23;               // Internetwork Version 6
        public const uint AF_CLUSTER = 24;             // Microsoft Wolfpack
        public const uint AF_12844 = 25;               // IEEE 1284.4 WG AF
        public const uint AF_IRDA = 26;                // IrDA
        public const uint AF_NETDES = 28;              // Network Designers OSI & gateway

        /**** How to send request data ****/
        public enum TRANSFER_METHOD    
        {
            IPPTOOL_TRANSFER_AUTO,          /* Chunk for files, length for static */
            IPPTOOL_TRANSFER_CHUNKED,       /* Chunk always */
            IPPTOOL_TRANSFER_LENGTH			/* Length always */
        }

        public const uint IPP_STATUS_OK = 0x0000;		// successful-ok
        public const string APP_NAME = "CH_GPA_CH3";
        public const int NUM_ATTRIBUTES = 9;
        public const int npos = -1;

        //Attributes Supported 
        public const int COPIES = 0;
        public const int PRINT_QUALITY = 1;
        public const int MEDIA = 2;
        public const int SIDES = 3;
        public const int ORIENTATION_REQUESTED = 4;
        public const int OUTPUT_BIN = 5;
        public const int MEDIA_SOURCE = 6;
        public const int URI_SECURITY = 7;

        public enum PRINTER_MANUFACTURERS
        {
            PRINTER_MANUFACTURER_HP,
            PRINTER_MANUFACTURER_LEXMARK,
            PRINTER_MANUFACTURER_KONICA,
            PRINTER_MANUFACTURER_EPSON,
            PRINTER_MANUFACTURER_IBM,
            PRINTER_MANUFACTURER_BROTHER,
            PRINTER_MANUFACTURER_XEROX
        };

        public enum TRANFER
        {
            IPPTOOL_TRANSFER_AUTO,          /* Chunk for files, length for static */
            IPPTOOL_TRANSFER_CHUNKED,       /* Chunk always */
            IPPTOOL_TRANSFER_LENGTH			/* Length always */
        };

        public enum HTTP_ENCRYPTION             // HTTP encryption values
        {
            HTTP_ENCRYPTION_IF_REQUESTED,       // Encrypt if requested (TLS upgrade)
            HTTP_ENCRYPTION_NEVER,              // Never encrypt
            HTTP_ENCRYPTION_REQUIRED,           // Encryption is required (TLS upgrade)
            HTTP_ENCRYPTION_ALWAYS              // Always encrypt (HTTPS)
        };

    }
}
