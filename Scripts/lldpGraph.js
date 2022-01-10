var chartDom = document.getElementById('main');
var myChart = echarts.init(chartDom);
var option;
var myData = JSON.parse(document.getElementById('customInput').dataset.value);
// 设置边的宽度和颜色
myData.edges.forEach(x => x.lineStyle = {
    color:"#0f0" ,
    width:x.texts.length,
    
})
option = {
    title: {
        text: 'lldp Graph'
    },
    //设置提示，当鼠标放到边上时的提示。此处设置有问题，放到node上不显示提示
    //因为node 节点无text 属性
    tooltip: {
        show: true,
        formatter: p => p.data.text,
    },
    animationDurationUpdate: 1500,
    animationEasingUpdate: 'quinticInOut',
    series: [
        {
            type: 'graph',
            layout: 'force',
            symbolSize: 50,
            roam: true,
            //节点名称及显示位置
            label: {
                show:true,
                // 显示 sysName
                formatter: p=>p.data.sysName,
                position:'bottom',
            },
            // 显示箭头
            edgeSymbol: ['circle', 'arrow'],
            edgeSymbolSize: [4, 10],
            // 边上的文字不显示
            edgeLabel: {
                fontSize: 12,
                show:false,
                formatter:p=>p.data.text,
            },
            force: {
                edgeLength: 300,
                repulsion: 300,

            },
            draggable: true,
            data: myData.nodes,
            // links: [],
            links: myData.edges,
            lineStyle: {
                opacity: 0.9,
                width: 2,
                curveness: 0
            }
        }
    ]
};

option && myChart.setOption(option);