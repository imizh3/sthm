using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STHM.Data
{
    public class HtsmContext:DbContext
    {
        public HtsmContext():base("HtsmConnectionString")
        {
            Database.SetInitializer<HtsmContext>(new CreateDatabaseIfNotExists<HtsmContext>());
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HtsmContext, Migrations.Configuration>());
        }

        public DbSet<Nhomquyen> Nhomquyens { get; set; }
        public DbSet<Nguoidung> Nguoidungs { get; set; }
        public DbSet<Lines> Lines { get; set; }
        public DbSet<Meter> Meters { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Capmatudong> Capmatudongs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Nhomquyen>().HasKey(d => d.Manhomquyen);
            modelBuilder.Entity<Nguoidung>().HasKey(d => d.UserName);
            modelBuilder.Entity<Lines>().HasKey(d => d.LINE_ID);
            modelBuilder.Entity<Meter>().HasKey(d => d.MET_ID);
            modelBuilder.Entity<Schedule>().HasKey(d => d.F_ID);
            modelBuilder.Entity<Config>().HasKey(d => d.Ma_NM);
            modelBuilder.Entity<Capmatudong>().HasKey(d =>d.Chucnang);
            base.OnModelCreating(modelBuilder);
        }
    }
}
