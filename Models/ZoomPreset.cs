using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMPaintApp.Models
{
    public class ZoomPreset(double value)
    {
        public double Value { get; } = value;
        public string Display => $"{Value * 100:N0}%";
        public override string ToString() => Display;
    }
}
