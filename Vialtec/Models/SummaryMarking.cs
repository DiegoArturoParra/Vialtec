using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vialtec.Models
{
    public class SummaryMarking
    {
        // Velocidad promedio
        public double AverageSpeed { get; set; }
        // Metros aplicados
        public double SumLeftPaintMeters { get; set; }
        public double SumCenterPaintMeters { get; set; }
        public double SumRightPaintMeters { get; set; }
        public double SumMostRightPaintMeters { get; set; }
        public string InitialDate { get; set; }
        public string FinalDate { get; set; }
        public int TrackNumber { get; set; }
        // Total de metros aplicados
        public double SumTotalPaintMeters
        {
            get
            {
                return SumLeftPaintMeters + SumCenterPaintMeters + SumRightPaintMeters + SumMostRightPaintMeters;
            }
        }
        // Total de metros
        public double TotalMeters { get; set; }
        // Total de segundos
        public double TotalSeconds { get; set; }
        // Total de minutos
        public double TotalMinutes
        {
            get
            {
                return TotalSeconds / 60;
            }
        }
    }
}
