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
            public string Name { get; set; }
        }
        public class Edge
        {
            public string Source { get; set; }
            public string Target { get; set; }
            /// <summary>
            /// 边上显示的内容
            /// </summary>
            public string Text { get; set; }

        }

    }
}