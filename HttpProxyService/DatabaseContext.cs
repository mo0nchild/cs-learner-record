using HttpProxyService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpProxyService
{
    public partial class DatabaseContext : DbContext
    {
        public DbSet<MethodInfo> MethodInfos { get; set; } = default!;
        public DbSet<AccessLog> AccessLogs { get; set; } = default!;

        public DatabaseContext(DbContextOptions options) : base(options) { }
        protected DatabaseContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MethodInfo>().HasData(new MethodInfo[]
            {
                new MethodInfo() { MethodInfoId = 1, Name = "StudentProfile", RequestPath = "student_info/profile" },
                new MethodInfo() { MethodInfoId = 2, Name = "StudentOrders", RequestPath = "student_info/orders" },
                new MethodInfo() { MethodInfoId = 3, Name = "StudentStatements", RequestPath = "student_info/statements" },

                new MethodInfo() { MethodInfoId = 4, Name = "EmployeeProfile", RequestPath = "employee_info/profile" },
                new MethodInfo() { MethodInfoId = 5, Name = "EmployeeAttestation", RequestPath = "employee_info/attestation" },
                new MethodInfo() { MethodInfoId = 6, Name = "EmployeeSetMark", RequestPath = "employee_info/setmark" },
                new MethodInfo() { MethodInfoId = 7, Name = "EmployeeMarksList", RequestPath = "employee_info/markslist" },
            });
        }
    }
}
