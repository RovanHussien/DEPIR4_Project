using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DEPI.DAL.Model;
using DEPI.DAL.Models;
namespace DEPI.DAL.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options):base(options) 
        { 

        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<EmployeeDepartment> EmployeeDepartments { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<JopDescription> JopDescriptions { get; set; }
        public DbSet<Mission> Missions { get; set; }
        public DbSet<VacationRequest> VacationRequests { get; set; }
        public DbSet<SwapRequest> SwapRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.EmployeeSsn)
                .ValueGeneratedNever();

                entity.HasKey(e => e.EmployeeSsn);
                entity.Property(e => e.Salary)
                .HasColumnType("decimal(18, 2)")
                .HasPrecision(18, 2);

                entity.HasOne(e => e.Manager)
                    .WithMany(e => e.Subordinates)
                    .HasForeignKey(e => e.ManagerSsn)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Shift)
                    .WithMany(s => s.Employees)
                    .HasForeignKey(e => e.ShiftId);

                entity.HasOne(e => e.ProductionLine)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(e => e.ProductionLineId);

                entity.HasOne(e => e.ApplicationUser)
                .WithOne(i => i.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .IsRequired(false);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.DepartmentId);

                entity.HasOne(d => d.Manager)
                    .WithOne(e => e.ManagedDepartment)
                    .HasForeignKey<Department>(d => d.ManagerSsn)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeDepartment>(entity =>
            {
                entity.HasKey(ed => new { ed.EmployeeSsn, ed.DepartmentID });

                entity.HasOne(ed => ed.Employees)
                    .WithMany(e => e.EmployeeDepartments)
                    .HasForeignKey(ed => ed.EmployeeSsn)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ed => ed.Departments)
                    .WithMany(d => d.EmployeeDepartments)
                    .HasForeignKey(ed => ed.DepartmentID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Shift>(entity =>
            {
                entity.HasKey(s => s.ShiftId);
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(s => s.ScheduleId);

                entity.HasOne(s => s.Employee)
                    .WithMany(e => e.Schedules)
                    .HasForeignKey(s => s.EmployeeSsn);

                entity.HasOne(s => s.Mission)
                    .WithMany(m => m.Schedules)
                    .HasForeignKey(s => s.MissionId);

                entity.HasOne(s => s.Shift)
                    .WithMany(sh => sh.Schedules)
                    .HasForeignKey(s => s.ShiftId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.JopDescription)
                    .WithMany(j => j.Schedules)
                    .HasForeignKey(s => s.JopDescriptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.ProductionLine)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(s => s.ProductionLineId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.VacationRequest)
                    .WithMany(v => v.Schedules)
                    .HasForeignKey(s => s.VacationRequestId);

                entity.HasOne(s => s.Attendance)
                    .WithOne(a => a.Schedule)
                    .HasForeignKey<Attendance>(a => a.ScheduleId);

                entity.HasOne(s => s.SwapRequest)
                    .WithOne(sr => sr.Schedule)
                    .HasForeignKey<SwapRequest>(sr => sr.ScheduleId);
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(a => a.AttendanceId);
            });

            modelBuilder.Entity<ProductionLine>(entity =>
            {
                entity.HasKey(p => p.ProductionLineId);

                entity.HasOne(p => p.Department)
                    .WithMany(d => d.ProductionLines)
                    .HasForeignKey(p => p.DepartmentId);
            });

            modelBuilder.Entity<JopDescription>(entity =>
            {
                entity.HasKey(j => j.JopDescriptionId);

                entity.HasOne(j => j.ProductionLine)
                    .WithMany(p => p.JopDescriptions)
                    .HasForeignKey(j => j.ProductionId);
            });

            modelBuilder.Entity<Mission>(entity =>
            {
                entity.HasKey(m => m.MissionId);
             
                entity.HasOne(m => m.AuthorizedEmployee)
                    .WithMany(e => e.AuthorizedMissions)
                    .HasForeignKey(m => m.AuthorizedEmployeeSsn)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.GoesOnEmployee)
                    .WithMany(e => e.GoesOnMissions)
                    .HasForeignKey(m => m.GoesOnEmployeeSsn)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VacationRequest>(entity =>
            {
                entity.HasKey(v => v.VacationRequestId);

                entity.HasOne(v => v.Employee)
                    .WithMany(e => e.VacationRequests)
                    .HasForeignKey(v => v.EmployeeSsn);
            });

            modelBuilder.Entity<SwapRequest>(entity =>
            {
                entity.HasKey(sr => sr.RequestId);

                entity.HasOne(sr => sr.RequestEmployee)
                    .WithMany(e => e.SentSwapRequests)
                    .HasForeignKey(sr => sr.RequestingEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.RecipientEmployee)
                    .WithMany(e => e.ReceivedSwapRequests)
                    .HasForeignKey(sr => sr.RecipientEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
