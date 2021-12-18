using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MIB;

namespace MIB.Controllers
{
    public class LLDPController : Controller
    {
        // GET: LLDP
        public ActionResult Index(string ip = "192.168.56.110")
        {
            //return View(GetLldpRemTable(ip));
            //return View(GetLldpLocProtTable(ip));
            return View(MeargeTable(ip));

        }
        public DataTable GetLldpRemTable(string host)
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
        public DataTable GetLldpLocProtTable(string host)
        {
            EasySnmp.MibTable mt = new EasySnmp.MibTable();
            mt.TableName = "lldpLocPortTable";
            mt.TableOid = "1.0.8802.1.1.2.1.3.7";
            mt.Indexs.Add(new EasySnmp.MibTable.TableIndex("lldpLocPortNum", 0, 1));
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("lldpLocPortId", "1.0.8802.1.1.2.1.3.7.1.3"));
            mt.Coloums.Add(new EasySnmp.MibTable.TableColoum("lldpLocPortDes", "1.0.8802.1.1.2.1.3.7.1.4"));
            SimpleSnmp ss = new SimpleSnmp(host, "public2018");
            EasySnmp es = new EasySnmp(ss, SnmpVersion.Ver2);
            return es.Walk(mt);

        }
        public DataTable MeargeTable(string host)
        {
            DataTable remTable = GetLldpRemTable(host);
            DataTable locTable = GetLldpLocProtTable(host);

            var query = from a in remTable.AsEnumerable()
                        join b in locTable.AsEnumerable() on a["lldpRemLocalPortNum"] equals b["lldpLocPortNum"]
                        select new
                        {
                            lldpLocPortNum = b["lldpLocPortNum"],
                            lldpLocPortId = b["lldpLocPortId"],
                            lldpRemPortId = a["lldpRemPortId"],
                            lldpRemSysName = a["lldpRemSysName"]
                        };           
            DataTable orderTable = query.CopyToDataTable();
            return orderTable;

        }
    }
}