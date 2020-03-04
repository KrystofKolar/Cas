using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Monogame.App.NET472
{
    [Serializable]
    public class GfxItem : MarshalByRefObject
    {
        int[]  a = new int[]{1,2,3,4, 5};

        public int Calc ()
        {
            return a.Sum();
        }
    }
}
