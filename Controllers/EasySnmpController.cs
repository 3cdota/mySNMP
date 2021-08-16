using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SnmpSharpNet;

namespace MIB.Controllers
{
    public class EasySnmpController : Controller
    {
        // GET: EasySnmp
        public ActionResult Index(string host)
        {
            return View(GetifTable(host));
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
            EasySnmp es = new EasySnmp(ss,SnmpVersion.Ver2);
            return es.Walk(mt);

        }
    }
}