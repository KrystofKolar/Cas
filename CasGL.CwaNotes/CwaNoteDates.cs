using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwaNotes
{
    // Summary:
    //    Range of dates around a center date
    public class Dates
    {
        static DateTime DEFAULT = DateTime.MinValue;

        public DateTime minus10y = DEFAULT;
        public DateTime minus5y = DEFAULT;
        public DateTime minus1y = DEFAULT;
        public DateTime minus6m = DEFAULT;
        public DateTime minus3m = DEFAULT;
        public DateTime minus1m = DEFAULT;
        public DateTime minus1w = DEFAULT;
        public DateTime minus1d = DEFAULT;
        public DateTime minus8h = DEFAULT;

        public DateTime plus8h = DEFAULT;
        public DateTime plus1d = DEFAULT;
        public DateTime plus1w = DEFAULT;
        public DateTime plus1m = DEFAULT;
        public DateTime plus3m = DEFAULT;
        public DateTime plus6m = DEFAULT;
        public DateTime plus1y = DEFAULT;
        public DateTime plus5y = DEFAULT;
        public DateTime plus10y = DEFAULT;

        public void Update(DateTime ctr)
        {
            minus10y = ctr.AddYears(-10);
            minus5y = ctr.AddYears(-5);
            minus1y = ctr.AddYears(-1);
            minus6m = ctr.AddMonths(-6);
            minus3m = ctr.AddMonths(-3);
            minus1m = ctr.AddMonths(-1);
            minus1w = ctr.AddDays(-7);
            minus1d = ctr.AddDays(-1);
            minus8h = ctr.AddHours(-8);
            plus8h = ctr.AddHours(+8);
            plus1d = ctr.AddDays(1);
            plus1w = ctr.AddDays(7);
            plus1m = ctr.AddMonths(1);
            plus3m = ctr.AddMonths(3);
            plus6m = ctr.AddMonths(6);
            plus1y = ctr.AddYears(1);
            plus5y = ctr.AddYears(5);
            plus10y = ctr.AddYears(10);
        }
    }

}
