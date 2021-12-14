namespace STHM.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.SQLite.EF6.Migrations;
    using System.Linq;
    using Data;
    using LIB;

    internal sealed class Configuration : DbMigrationsConfiguration<STHM.Data.HtsmContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
            SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator());
        }

        protected override void Seed(STHM.Data.HtsmContext context)
        {
            //  This method will be called after migrating to the latest version.
            //add nguoi dung mac dinh
            try
            {
                Nguoidung user = context.Nguoidungs.Where(d => d.FullName == "Administrator").FirstOrDefault();
                if (user == null)
                {
                    context.Nguoidungs.AddOrUpdate(new Nguoidung() { FullName = "Administrator", UserName = "Admin", Password = LIB.Password.EnPassword("Admin") });
                }
            }
            catch { }
            //add nhom quyen mac dinh
            context.Nhomquyens.AddOrUpdate(new Nhomquyen()
            {
                Manhomquyen = "ADMINISTRATOR",
                Tennhomquyen = "Quản trị viên",
                Trangthai = 1
            });
            context.Nhomquyens.AddOrUpdate(new Nhomquyen()
            {
                Manhomquyen = "USER",
                Tennhomquyen = "Người dùng",
                Trangthai = 1
            });
            Config check = context.Configs.Where(d => d.Ma_NM == PublicValue.MaNM).FirstOrDefault();
            if (check == null)
            {
                context.Configs.AddOrUpdate(new Config());
            }
            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
