                                     using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using SnmpSharpNet;


namespace MIB
{
    public class EasySnmp 
    {
        //内部类 MibTable
        public class MibTable
        {
            public string TableName { get; set; }
            public string TableOid { get; set; }

            public List<TableColoum> Coloums { get; set; }

            public class TableColoum
            {
                public string ColoumName { get; set; }
                public string ColoumOid { get; set; }
                public Type ColoumType { get; set; }

                public TableColoum(string name,string oid,Type type)
                {
                    this.ColoumName = name;
                    this.ColoumOid = oid;
                    this.ColoumType = type;
                }
            }

            public MibTable()
            {
                this.Coloums = new List<TableColoum>();
            }


        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private  SimpleSnmp snmp;
        private SnmpVersion version;
        private object Parse(Type type, string value)
        {
            if (type == typeof(int))
            {
                return int.Parse(value);
            }
            if (type == typeof(String))
            {
                return value;
            }
            if(type==typeof(System.DateTime))
            {
                return DateTime.Parse(value);
            }
            return value;
        }

        public static string InstanceToString(uint[] instance)
        {
            StringBuilder str = new StringBuilder();
            foreach (uint v in instance)
            {
                if (str.Length == 0)
                    str.Append(v);
                else
                    str.AppendFormat(".{0}", v);
            }
            return str.ToString();
        }

        public EasySnmp(SimpleSnmp snmp,SnmpVersion ver)
        {
            this.snmp = snmp;
            this.version = ver;
        }
        
        public DataTable Walk(MibTable table)
        {
            //创建dataTable
            DataTable dt = new DataTable(table.TableName);
            //instance ID
            dt.Columns.Add("InstanceID", typeof(String));
            foreach (MibTable.TableColoum tc in table.Coloums)
            {
                dt.Columns.Add(tc.ColoumName, tc.ColoumType);
            }

            if (!snmp.Valid)
            {
                logger.Error("SNMP agent host name/ip address is invalid.");
                
                return dt;
            }
            Dictionary<Oid, AsnType> result = snmp.Walk(version,table.TableOid);
            if (result == null)
            {
                logger.Warn("No results received.");
                return dt;
            }
            
            foreach (var kvp in result)
            {
                foreach (MibTable.TableColoum tc in table.Coloums)
                {
                    Oid coid = new Oid(tc.ColoumOid);
                    // 如果该kvp 是某一列的值
                    if(coid.IsRootOf(kvp.Key))
                    {
                        string instanceId = InstanceToString(Oid.GetChildIdentifiers(coid, kvp.Key));
                        //查询已经是否存在当前行,如果无，则添加，如果有则更新
                        DataRow dr = dt.AsEnumerable().FirstOrDefault(r => r["InstanceID"].ToString() == instanceId);
                       if(dr==null)
                        {
                            dr = dt.NewRow();
                            dr["InstanceID"] = instanceId;
                            dr[tc.ColoumName] = Parse(tc.ColoumType, kvp.Value.ToString());
                            dt.Rows.Add(dr);
                        }else
                        {
                            dr[tc.ColoumName] = Parse(tc.ColoumType, kvp.Value.ToString());

                        }
                        break;
                    }
                }
                
            }
            return dt;
        }
    }
}