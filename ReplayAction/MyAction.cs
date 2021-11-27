using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReplayAction.MyHook;

namespace ReplayAction
{
    public class MyOptions
    {
        public List<object> param;
        public MyOptions(params object[] _obj)
        {
            for (int i = 0; i < _obj.Length; i++)
            {
                param.Add(_obj[i]);
            }
        }
    }
    internal class MyAction
    {
        public string Name;
        public int reqParamNum;
        public Action<List<object>> make;
        public Type type;
        public MyAction(string _name, int _count, Action<List<object>> _action, Type _type)
        {
            Name = _name;
            reqParamNum = _count;
            make = _action;
            type = _type;
        }
    }
}
