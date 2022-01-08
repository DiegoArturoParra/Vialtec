using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Utilitarios;

namespace Datos
{
    public class VialtecContext : DbContext
    {
        public VialtecContext(DbContextOptions<VialtecContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Command> Commands { get; set; }
        public DbSet<Marking> Markings { get; set; }
        public DbSet<MuReport> MuReports { get; set; }
        public DbSet<SAdmin> SuperAdmins { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Geometry> Geometries { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<LineColor> LineColors { get; set; }
        public DbSet<Precommand> Precommands { get; set; }
        public DbSet<Subproject> Subprojects { get; set; }
        public DbSet<ModelEvent> ModelEvents { get; set; }
        public DbSet<TelegramBot> TelegramBots { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<CustomerInfo> CustomerInfos { get; set; }
        public DbSet<CustomerUser> CustomerUsers { get; set; }
        public DbSet<EncodingType> EncodingTypes { get; set; }
        public DbSet<Reflectivity> Reflectivities { get; set; }
        public DbSet<EquipmentGroup> EquipmentGroups { get; set; }
        public DbSet<SpeedStatReport> SpeedStatReports { get; set; }
        public DbSet<DistributorInfo> DistributorInfos { get; set; }
        public DbSet<DistributorUser> DistributorUsers { get; set; }
        public DbSet<SecurityProfile> SecurityProfiles { get; set; }
        public DbSet<PrecommandByUser> PrecommandByUsers { get; set; }
        public DbSet<SinglePermission> SinglePermissions { get; set; }
        public DbSet<TransmissionInfo> TransmissionInfos { get; set; }
        public DbSet<CustomVehicleType> CustomVehicleTypes { get; set; }
        public DbSet<ProfilePermission> ProfilePermissions { get; set; }
        public DbSet<CustomerModelEvent> CustomerModelEvents { get; set; }
        public DbSet<CoefficientFriction> CoefficientFrictions { get; set; }
        public DbSet<SecurityProfileDist> SecurityProfileDists { get; set; }
        public DbSet<SpeedReportCustomer> SpeedReportsCustomer { get; set; }
        public DbSet<SinglePermissionDist> SinglePermissionDists { get; set; }
        public DbSet<StationarySpeedRadar> StationarySpeedRadars { get; set; }
        public DbSet<ProfilePermissionDist> ProfilePermissionDists { get; set; }
        public DbSet<CustomerUserPermission> CustomerUserPermissions { get; set; }
        public DbSet<PrecommandCustomerName> PrecommandCustomerNames { get; set; }
        public DbSet<EmailNotificationProfile> EmailNotificationProfiles { get; set; }
        public DbSet<CustomerEventNotification> CustomerEventNotifications { get; set; }
        public DbSet<TelegramNotificationProfile> TelegramNotificationProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Definir Primary Key compuesta para la tabla de stationary_speed_radar
            modelBuilder.Entity<StationarySpeedRadar>().HasKey(e => new { e.EquipmentId, e.DeviceDt, e.ServerDt });
            modelBuilder.Entity<SpeedStatReport>().HasKey(e => new { e.EquipmentId, e.DeviceDt, e.ServerDt });
            base.OnModelCreating(modelBuilder);
        }
    }
}
