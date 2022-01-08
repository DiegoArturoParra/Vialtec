using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Utilitarios
{
    [Table("marking", Schema = "reports")]
    public class Marking
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        
        [Column("gps_valid")]
        public bool GpsValid { get; set; }

        [Column("millis")]
        public long Millis { get; set; }

        [Column("encoder_speed")]
        public double EncoderSpeed { get; set; }

        [Column("gps_speed")]
        public double GpsSpeed { get; set; }

        [Column("latitude")]
        public double? Latitude { get; set; }

        [Column("longitude")]
        public double? Longitude { get; set; }

        [Column("left_time")]
        public double LeftTime { get; set; }

        [Column("center_time")]
        public double CenterTime { get; set; }

        [Column("right_time")]
        public double RightTime { get; set; }

        [Column("most_right_time")]
        public double MostRightTime { get; set; }

        [Column("left_paint_meters")]
        public double LeftPaintMeters { get; set; }

        [Column("center_paint_meters")]
        public double CenterPaintMeters { get; set; }

        [Column("right_paint_meters")]
        public double RightPaintMeters { get; set; }

        [Column("most_right_paint_meters")]
        public double MostRightPaintMeters { get; set; }

        [Column("left_total_meters")]
        public double LeftTotalMeters { get; set; }

        [Column("center_total_meters")]
        public double CenterTotalMeters { get; set; }

        [Column("right_total_meters")]
        public double RightTotalMeters { get; set; }

        [Column("most_right_total_meters")]
        public double MostRightTotalMeters { get; set; }

        [Column("asset_equipment_id")]
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }

        [Column("server_dt")]
        public DateTime ServerDt { get; set; }

        [Column("device_dt")]
        public DateTime DeviceDt { get; set; }

        [Column("gps_odometer")]
        public double GpsOdometer { get; set; }
    }
}
