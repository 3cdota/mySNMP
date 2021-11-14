using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MIB.Controllers
{
    public class SysInfoController : Controller
    {
        // GET: SysnInfo
        public ActionResult Index(string host= "192.168.56.110")
        {

            return View(GetSysInfo(host));
        }

        public List<EasySnmp.MibNode> GetSysInfo(string host)
        {
            List<EasySnmp.MibNode> nodes = new List<EasySnmp.MibNode>();
            EasySnmp.MibNode node = new EasySnmp.MibNode("sysDescr", ".1.3.6.1.2.1.1.1.0",typeof(string));
            nodes.Add(node);
            EasySnmp.MibNode node1 = new EasySnmp.MibNode("sysObject", ".1.3.6.1.2.1.1.2.0", typeof(string));
            nodes.Add(node1);
            EasySnmp.MibNode node2 = new EasySnmp.MibNode("sysUpTime", ".1.3.6.1.2.1.1.3.0", typeof(string));
            nodes.Add(node2);
            EasySnmp.MibNode node3 = new EasySnmp.MibNode("sysName", ".1.3.6.1.2.1.1.5.0", typeof(string));
            nodes.Add(node3);
            EasySnmp.MibNode node4 = new EasySnmp.MibNode("sysServices", ".1.3.6.1.2.1.1.7.0", typeof(string));
            nodes.Add(node4);
            SimpleSnmp ss = new SimpleSnmp(host, "public2018");
            EasySnmp es = new EasySnmp(ss, SnmpVersion.Ver2);
            return es.Get(nodes);

        }
    }
}