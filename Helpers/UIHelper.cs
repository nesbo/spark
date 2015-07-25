using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace frontoffice.Helpers
{
    public static class UIHelper
    {
        public static string GetProgressBarCSSStyle(int Value)
        {
            return Value <= 0 ?
                string.Format("height:100%; left:{0}%; width:{1}%; position:relative; background-color: lightsalmon", 50 + Value / 2, -Value / 2) :
                string.Format("height:100%; left:50%; width:{0}%; position:relative; background-color: palegreen", Value / 2);
        }
    }
}