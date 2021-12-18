using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using SnmpSharpNet;


namespace MIB
{
    /// <summary>
    /// 对SimpleSnmap进行封装
    /// </summary>
    public class EasySnmp
    {
        /// <summary>
        /// 表节点
        /// </summary>
        public class MibTable
        {
            /// <summary>
            /// 表名
            /// </summary>
            public string TableName { get; set; }
            /// <summary>
            /// 表Oid
            /// </summary>
            public string TableOid { get; set; }

            public List<TableIndex> Indexs { get; set; }

            /// <summary>
            /// 表的索引项，从哪里开始，到哪里结束
            /// </summary>
            public class TableIndex
            {
                //该索引的名称
                public string IndexName { get; set; }
                //跳过几个unint32
                public int SkipNum { get; set; }
                //占几个 uint32
                public int TakeNum { get; set; }

                public TableIndex(string indexName, int skipnum, int takeNum)
                {
                    this.IndexName = indexName;
                    this.SkipNum = skipnum;
                    this.TakeNum = takeNum;
                }

            }

            /// <summary>
            ///需要读取的列列表
            /// </summary>
            public List<TableColoum> Coloums { get; set; }


            /// <summary>
            /// 列结构
            /// </summary>
            public class TableColoum
            {
                /// <summary>
                ///表列结构
                /// </summary>
                public string ColoumName { get; set; }
                public string ColoumOid { get; set; }
                public Type ColoumType { get; set; }

                public TableColoum(string name, string oid, Type type)
                {
                    this.ColoumName = name;
                    this.ColoumOid = oid;
                    this.ColoumType = type;
                }

                public TableColoum(string name, string oid)
                {
                    this.ColoumName = name;
                    this.ColoumOid = oid;
                    this.ColoumType = typeof(string);
                }
            }

            public MibTable()
            {
                this.Coloums = new List<TableColoum>();
                this.Indexs = new List<TableIndex>();
            }


        }

        /// <summary>
        /// MIB 节点
        /// </summary>
        public class MibNode
        {
            public string Name;
            public string Oid;
            public Type Type;
            public object value;

            public MibNode(string name, string oid, Type type)
            {
                this.Name = name;
                this.Oid = oid;
                this.Type = type;
            }
        }


        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private SimpleSnmp snmp;
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
            if (type == typeof(System.DateTime))
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

        public EasySnmp(SimpleSnmp snmp, SnmpVersion ver)
        {
            this.snmp = snmp;
            this.version = ver;
        }

        /// <summary>
        /// GET 某些节点的值
        /// </summary>
        /// <param name="nodes">节点列表</param>
        /// <returns>节点列表包含值</returns>
        public List<MibNode> Get(List<MibNode> nodes)
        {
            if (!snmp.Valid)
            {
                logger.Error("SNMP agent host name/ip address is invalid.");

                return null;
            }
            string[] oids = nodes.Select(n => n.Oid).ToArray();
            Dictionary<Oid, AsnType> result = snmp.Get(version, oids);
            if (result == null)
            {
                logger.Warn("No results received.");
                return null;
            }
            foreach (var kvp in result)
            {
                var node = nodes.FirstOrDefault(n => n.Oid == kvp.Key.ToString() || n.Oid == "." + kvp.Key.ToString());
                if (node != null)
                {
                    node.value = Parse(node.Type, kvp.Value.ToString());
                }
            }
            return nodes;

        }


        /// <summary>
        /// 遍历表节点
        /// </summary>
        /// <param name="table">表节点</param>
        /// <returns>DataTable结构的结果</returns>
        public DataTable Walk(MibTable table)
        {
            //创建dataTable
            DataTable dt = new DataTable(table.TableName);
            //instance ID
            dt.Columns.Add("InstanceID", typeof(String));
            //添加索引项
            foreach (MibTable.TableIndex index in table.Indexs)
            {
                dt.Columns.Add(index.IndexName);
            }
            foreach (MibTable.TableColoum tc in table.Coloums)
            {
                dt.Columns.Add(tc.ColoumName, tc.ColoumType);
            }

            if (!snmp.Valid)
            {
                logger.Error("SNMP agent host name/ip address is invalid.");

                return dt;
            }
            Dictionary<Oid, AsnType> result = snmp.Walk(version, table.TableOid);
            if (result == null)
            {
                logger.Warn("No results received.");
                return dt;
            }
            foreach (var kvp in result)
            {
                foreach (MibTable.TableColoum tc in table.Coloums)
                {
                    AsnType val = new OctetString("This is my test string");
                    Oid coid = new Oid(tc.ColoumOid);
                    // 如果该kvp 是某一列的值
                    if (coid.IsRootOf(kvp.Key))
                    {
                        //索引项 
                        UInt32[] arrayIndexs = Oid.GetChildIdentifiers(coid, kvp.Key);
                        string instanceId = InstanceToString(arrayIndexs);
                        //查询已经是否存在当前行,如果无，则添加，如果有则更新
                        DataRow dr = dt.AsEnumerable().FirstOrDefault(r => r["InstanceID"].ToString() == instanceId);
                        if (dr == null)
                        {
                            dr = dt.NewRow();
                            dr["InstanceID"] = instanceId;
                            //添加索引项
                            foreach (MibTable.TableIndex index in table.Indexs)
                            {
                                UInt32[] arrayIndex = arrayIndexs.Skip(index.SkipNum)
                                                    .Take(index.TakeNum).ToArray();
                                string indexValue = InstanceToString(arrayIndex);
                                dr[index.IndexName] = indexValue;
                            }
                            dr[tc.ColoumName] = Parse(tc.ColoumType, kvp.Value.ToString());
                            dt.Rows.Add(dr);
                        }
                        else
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