using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MIB.Models
{
    public class Graph
    {
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Edge> Edges { get; set; } = new List<Edge>();
        public class Node
        {
            /// <summary>
            /// 和 echarts name对应
            /// </summary>
            public string Name
            {
                get
                {
                    return ChassisId;
                }
            }
            public string ChassisId { get; set; }
            public string SysName { get; set; }
        }
        public class Edge
        {
            public string Source { get; set; }
            public string Target { get; set; }
            /// <summary>
            /// 边上显示的内容
            /// </summary>
            public string Text
            { get
                {
                    return string.Join("<br/><br/>", Texts.ToArray())
                        .Replace("GigabitEthernet", "GE");

                }
            }

            public List<string> Texts = new List<string>();

        }

    }
}