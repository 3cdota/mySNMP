using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MIB;
using MIB.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MIB.Controllers
{
    public class LLDPController : Controller
    {
        // GET: LLDP
        public ActionResult Index(string ip = "192.168.56.110")
        {
            //return View(GetLldpRemTable(ip));
            //return View(GetLldpLocProtTable(ip));
            DataTable dt = MeargeTable(ip);
            //大小写和echarts保持一致
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            ViewBag.graph =JsonConvert.SerializeObject(GetGraph(dt,ip), serializerSettings);
            return View(dt);
            
            

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
                            lldpRemSysName = a["lldpRemSysName"],
                            lldpRemChassisId = a["lldpRemChassisId"]
                        };           
            DataTable orderTable = query.CopyToDataTable();
            return orderTable;

        }
        public Graph GetGraph(DataTable dt,string localName)
        {
            Graph g = new Graph();
            //本机节点
            Graph.Node localNode = new Graph.Node();
            localNode.SysName = localName;
            localNode.ChassisId = localName;
            g.Nodes.Add(localNode);
            //远端节点
            if (dt?.Rows?.Count > 0)
            {

                foreach(DataRow dr in dt.Rows)
                {
                    //添加节点
                    Graph.Node remoteNode = new Graph.Node();
                    remoteNode.SysName = dr["lldpRemSysName"].ToString();
                    remoteNode.ChassisId = dr["lldpRemChassisId"].ToString();
                    if (!g.Nodes.Exists(n=>n.ChassisId==remoteNode.ChassisId&&n.SysName==remoteNode.SysName))
                    {
                        g.Nodes.Add(remoteNode);
                    }
                    //添加边
                    string lldpLocPortId = dr["lldpLocPortId"].ToString();
                    string lldpRemPortId = dr["lldpRemPortId"].ToString();
                    Graph.Edge edge = new Graph.Edge();
                    edge.Source = localNode.ChassisId;
                    edge.Target = remoteNode.ChassisId;

                    Graph.Edge oldEdge = g.Edges.Find(e => e.Source == edge.Source && e.Target == edge.Target);
                    //如果已经存在该边，则在文字说明中增加端口连接关系
                    if(oldEdge!=null)
                    {
                        oldEdge.Texts.Add(lldpLocPortId + "-->" + lldpRemPortId);
                    }
                    //否则新增边
                    else
                    {
                        edge.Texts.Add(lldpLocPortId + "-->" + lldpRemPortId);
                        g.Edges.Add(edge);

                    }
                    
                     
                }

            }
            return g;

        }
    }
}