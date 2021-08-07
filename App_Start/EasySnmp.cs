                                     using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using SnmpSharpNet;

namespace MIB.App_Start
{
    public class EasySnmp 
    {
        public SimpleSnmp Snmp;


        public DataTable Walk(Dictionary<string, string> tableOid, Dictionary<string,string> coloums)
        {

            if (!snmp.Valid)
            {
                Console.WriteLine("SNMP agent host name/ip address is invalid.");
                return;
            }
            Dictionary<oid, AsnType> result = snmp.Get(SnmpVersion.Ver1,
                                                      new string[] { ".1.3.6.1.2.1.1.1.0" });
            if (result == null)
            {
                Console.WriteLine("No results received.");
                return;
            }
            foreach (KeyValuePair kvp in result)
            {
                Console.WriteLine("{0}: {1} {2}", kvp.Key.ToString(),
                                      SnmpConstants.GetTypeName(kvp.Value.Type),
                                      kvp.Value.ToString());
            }

            return null;

        }
    }
}