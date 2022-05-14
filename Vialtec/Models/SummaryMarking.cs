using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vialtec.Models
{
    public class SummaryMarking
    {


        // Metros aplicados
        public double SumLeftPaintMeters { get; set; }
        public double SumCenterPaintMeters { get; set; }
        public double SumRightPaintMeters { get; set; }
        public DateTime InitialDate { get; set; }
        public DateTime FinalDate { get; set; }
        public int TrackNumber { get; set; }
        public double TotalMeters { get; set; }
        // Total de minutos
        public double TotalMinutes { get; set; }
    }

    public class TotalSummaryMarking
    {
        // Total de metros pintados en la izquierda
        public double TotalLeftPaintMeters { get; set; }

        // Total de metros pintados en el centro
        public double TotalCenterPaintMeters { get; set; }

        // Total de metros pintados en la derecha   
        public double TotalRightPaintMeters { get; set; }

        //Fecha Incial del reccorrido
        public DateTime InitialDateRoute { get; set; }

        // Total de metros pintados en la derecha   
        public DateTime FinalDateRoute { get; set; }

        // Total de metros pintados
        public double TotalPaintMetersRoute
        {
            get
            {
                return TotalLeftPaintMeters + TotalCenterPaintMeters + TotalRightPaintMeters;
            }
        }
        // Total de minutos recorridos
        public double TotalMinutesRoute
        {
            get
            {
                return (FinalDateRoute - InitialDateRoute).TotalMinutes;
            }
        }
    }
}
