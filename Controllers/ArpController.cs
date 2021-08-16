using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SnmpSharpNet;
using System.Data;

namespace MIB.Controllers
{
    public class ArpController : Controller
    {
        // GET: Arp
        public ActionResult Index(string host="192.168.56.110")
        {
            var  mydatat = MeargeTable(host);
           
            return View(GetArpTable(host));
           
        }

        public DataTable GetArpTable(string host)
        {
            EasySnmp.MibTable mt = new EasySnmp.MibTable();
            mt.TableName = "hwArpDynTable";
            mt.TableOid = ".1.3.6.1.4.1.2011.5.25.123.1.17";
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("hwArpDynMacAdd", "1.3.6.1.4.1.2011.5.25.123.1.17.1.11", typeof(string)));
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("hwArpDynVlanId", "1.3.6.1.4.1.2011.5.25.123.1.17.1.12", typeof(int)));
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("hwArpDynOutIfIndex", "1.3.6.1.4.1.2011.5.25.123.1.17.1.14", typeof(int)));
            SimpleSnmp ss = new SimpleSnmp(host, "public2018");
            EasySnmp es = new EasySnmp(ss, SnmpVersion.Ver2);
            return es.Walk(mt);

        }

        public DataTable GetifTable(string host)
        {
            EasySnmp.MibTable mt = new EasySnmp.MibTable();
            mt.TableName = "ifTable";
            mt.TableOid = ".1.3.6.1.2.1.2.2";
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("ifIndex", ".1.3.6.1.2.1.2.2.1.1", typeof(int)));
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("ifDesrc", ".1.3.6.1.2.1.2.2.1.2", typeof(string)));
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("ifType", ".1.3.6.1.2.1.2.2.1.3", typeof(int)));
            SimpleSnmp ss = new SimpleSnmp(host, "public2018");
            EasySnmp es = new EasySnmp(ss, SnmpVersion.Ver2);
            return es.Walk(mt);

        }

        public object MeargeTable(string host)

        {
            DataTable arpTable = GetArpTable(host);
            DataTable ifTable = GetifTable(host);

            //var query = from a in ifTable.AsEnumerable()
            //            join b in arpTable.AsEnumerable() on a["ifIndex"] equals b["hwArpDynOutIfIndex"] into ab
            //            from c in ab.DefaultIfEmpty()
            //            select new { ifIndex = a["ifIndex"], ifDesrc = a["ifDesrc"], macAddress = c==null?"":c["hwArpDynMacAdd"] };

            var query = from a in ifTable.AsEnumerable()
                        join b in arpTable.AsEnumerable() on a["ifIndex"] equals b["hwArpDynOutIfIndex"] into ab
                       
                        select new {a=a,ab=ab};
            var data = query.ToList();
            return data;




        }
    }
}