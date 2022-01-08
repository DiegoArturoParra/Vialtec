using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vialtec.Models
{
    public class LineNumberReport
    {
        public int LineNumber { get; set; }
        public double AverageMeasurement { get; set; }
        public int TotalResults { get; set; }
    }
}
