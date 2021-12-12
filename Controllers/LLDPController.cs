using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MIB.Controllers
{
    public class LLDPController : Controller
    {
        // GET: LLDP
        public ActionResult Index(string ip = "192.168.56.110")
        {
            return View(GetLLDPTable(ip));

        }
        public DataTable GetLLDPTable(string host)
        {
            EasySnmp.MibTable mt = new EasySnmp.MibTable();
            mt.TableName = "lldpRemTable";
            mt.TableOid = ".1.0.8802.1.1.2.1.4.1.1";
            mt.Indexs.Add(new EasySnmp.MibTable.TableIndex("lldpRemLocalPortNum", 1, 1));
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("lldpRemPortId", "1.0.8802.1.1.2.1.4.1.1.7", typeof(string)));
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("lldpRemSysName", "1.0.8802.1.1.2.1.4.1.1.9", typeof(string)));
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("lldpRemChassisId", "1.0.8802.1.1.2.1.4.1.1.5", typeof(string)));
            SimpleSnmp ss = new SimpleSnmp(host, "public2018");
            EasySnmp es = new EasySnmp(ss, SnmpVersion.Ver2);
            return es.Walk(mt);

        }
    }
}