using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SnmpSharpNet;
using System.Data;

namespace MIB.Controllers
{
    public class HostController : Controller
    {
        // GET: Host
        public ActionResult Index(string ip="192.168.56.110")
        {
            List<Host> data = MeargeTable(ip);
            return View(data);
        }

        public class Host
        {
            public int IfIndex { get; set; }
            public string IfDesrc { get; set; }

            public string MacAddress { get; set; }
            public string IpAdress { get; set; }
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

        private string GetIp(string value)
        {
            return string.Join(".", value.Split('.').Skip(1).Take(4).ToArray());
        }

        public List<Host> MeargeTable(string host)

        {
            DataTable arpTable = GetArpTable(host);
            DataTable ifTable = GetifTable(host);

            var query = from a in ifTable.AsEnumerable()
                        join b in arpTable.AsEnumerable() on a["ifIndex"] equals b["hwArpDynOutIfIndex"] into ab
                        from c in ab.DefaultIfEmpty()
                        select new Host { IfIndex =int.Parse( a["ifIndex"].ToString()), IfDesrc = a["ifDesrc"].ToString(), MacAddress = c == null ? "" : c["hwArpDynMacAdd"].ToString(), IpAdress = c == null ? "" : GetIp(c["InstanceID"].ToString()) };
            var data = query.ToList();
            return data;
        }
    }
}