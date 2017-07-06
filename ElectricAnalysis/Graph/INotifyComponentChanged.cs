using ElectricAnalysis.Model;
using ElectricAnalysis.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.Graph
{
    public interface INotifyComponentChanged
    {
        ComponentState State { get; set; }
        ComponentType CptType { get; set; }
        void initStatus(ComponentType type);//初始化元件的状态
        void reverseNetLabelColor(string label);//提供改变相同网络标号的Label颜色的接口
        void reversePowMark(string label);//统一修改等电位点的上电标志
        void markNode();//提供改变指定元件颜色的接口
        void resetNode();//提供改变指定元件颜色的接口
        bool isCFNode();//判断是否是测试点
        bool CanPowUp(string label);//判断该点是否可以加电
        void connect();//使元件状态切换至导通状态
        void block();//使元件状态切换至阻塞状态
        void reverseStatus();//反转元件的导通状态
        bool CanConduct(bool order);//判断元件状态是否导通
        TNode getHeadNode();//获取元件的头结点
        TNode getTailNode();//获取元件的尾节点
        string getName();//获取元件的名称
        INotifyComponentChanged clone();//浅克隆内部字段
    }
}
