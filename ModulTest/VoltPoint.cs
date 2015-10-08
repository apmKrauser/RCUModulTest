using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulTest
{
    public class VoltPoint
    {
        public double Voltage { get; set; }
        public double Time { get; set; }
        public VoltPoint(double volt, double time)
        {
            this.Voltage = volt;
            this.Time = time;
        }

        public override string ToString()
        {
            return String.Format("{0:#0.0} {1:##0.0}", Time, Voltage);
        }
    }
}
