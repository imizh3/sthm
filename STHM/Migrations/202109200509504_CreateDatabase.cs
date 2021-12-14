namespace STHM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Capmatudongs",
                c => new
                    {
                        Chucnang = c.String(nullable: false, maxLength: 128),
                        Loai = c.String(maxLength: 2147483647),
                        Sotutang = c.Int(nullable: false),
                        Ten = c.String(maxLength: 2147483647),
                        Sokytu = c.Int(nullable: false),
                        IsAuto = c.Int(nullable: false),
                        Kieututang = c.Int(nullable: false),
                        Matruoc = c.String(maxLength: 2147483647),
                        Masau = c.String(maxLength: 2147483647),
                    })
                .PrimaryKey(t => t.Chucnang);
            
            CreateTable(
                "dbo.Configs",
                c => new
                    {
                        Ma_NM = c.String(nullable: false, maxLength: 128),
                        AutoRun = c.Boolean(nullable: false),
                        CommunicationDelay = c.Int(nullable: false),
                        ConnectionString = c.String(maxLength: 2147483647),
                        ExportDataCompany = c.String(maxLength: 2147483647),
                        ExportDataOldPath = c.String(maxLength: 2147483647),
                        formatfile = c.String(maxLength: 2147483647),
                        IsPCVersion = c.Boolean(nullable: false),
                        LocalData = c.Boolean(nullable: false),
                        ManualExportFile = c.Boolean(nullable: false),
                        ManualExportFilePath = c.String(maxLength: 2147483647),
                        MonitorLineInterval = c.Int(nullable: false),
                        OpenFile = c.Boolean(nullable: false),
                        Password = c.String(maxLength: 2147483647),
                        Password2 = c.String(maxLength: 2147483647),
                        PasswordLoggedIn = c.String(maxLength: 2147483647),
                        Port = c.Int(nullable: false),
                        ReadingFailResetPercent = c.Int(nullable: false),
                        RememberLogin = c.Int(nullable: false),
                        ScheduleDelay = c.Int(nullable: false),
                        ScheduleExportFilePath = c.String(maxLength: 2147483647),
                        ScheduleInterval = c.Int(nullable: false),
                        ShowCommunicationLog = c.Boolean(nullable: false),
                        User = c.String(maxLength: 2147483647),
                        User2 = c.String(maxLength: 2147483647),
                        UserLoggedIn = c.String(maxLength: 2147483647),
                        UserType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Ma_NM);
            
            CreateTable(
                "dbo.Lines",
                c => new
                    {
                        LINE_ID = c.String(nullable: false, maxLength: 128),
                        BUSY = c.Boolean(nullable: false),
                        CONFIG = c.String(maxLength: 2147483647),
                        CONNECT = c.Boolean(nullable: false),
                        CONNECTION_STATUS = c.String(maxLength: 2147483647),
                        LINE_TYPE = c.String(maxLength: 2147483647),
                        READING_STATUS = c.String(maxLength: 2147483647),
                        RECONNECT_NUMBER = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LINE_ID);
            
            CreateTable(
                "dbo.Meters",
                c => new
                    {
                        MET_ID = c.String(nullable: false, maxLength: 128),
                        BAUD_RATE = c.Int(nullable: false),
                        COMM_LINE = c.String(maxLength: 2147483647),
                        DATALINK_OSN = c.String(maxLength: 2147483647),
                        MADDO = c.String(maxLength: 2147483647),
                        MET_KEY = c.String(maxLength: 2147483647),
                        MET_TYPE = c.String(maxLength: 2147483647),
                        OUT_STATION_NUMBER = c.String(maxLength: 2147483647),
                        PROG_PW = c.String(maxLength: 2147483647),
                        REG_NUM = c.Int(nullable: false),
                        T = c.Int(nullable: false),
                        TENDDO = c.String(maxLength: 2147483647),
                        TI = c.Int(nullable: false),
                        TU = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MET_ID);
            
            CreateTable(
                "dbo.Nguoidungs",
                c => new
                    {
                        UserName = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(maxLength: 2147483647),
                        Password = c.String(maxLength: 2147483647),
                    })
                .PrimaryKey(t => t.UserName);
            
            CreateTable(
                "dbo.Nhomquyens",
                c => new
                    {
                        Manhomquyen = c.String(nullable: false, maxLength: 128),
                        Tennhomquyen = c.String(maxLength: 2147483647),
                        Trangthai = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Manhomquyen);
            
            CreateTable(
                "dbo.Schedules",
                c => new
                    {
                        F_ID = c.Int(nullable: false, identity: true),
                        DATALINK_OSN = c.String(maxLength: 2147483647),
                        ENABLED = c.Boolean(nullable: false),
                        F_EXCEPT_FROM = c.DateTime(nullable: false),
                        F_EXCEPT_TO = c.DateTime(nullable: false),
                        F_EXCEPTION = c.Boolean(nullable: false),
                        F_MET_KEY = c.String(maxLength: 2147483647),
                        F_PERIOD_VALUE = c.Int(nullable: false),
                        F_READING_COUNT = c.Int(nullable: false),
                        F_READING_STATUS = c.Int(nullable: false),
                        IS_FROM_STARTDATE = c.Boolean(nullable: false),
                        LINE_ID = c.String(maxLength: 2147483647),
                        LINE_TYPE = c.String(maxLength: 2147483647),
                        MET_KEY = c.String(maxLength: 2147483647),
                        MET_TYPE = c.String(maxLength: 2147483647),
                        OUT_STATION_NUMBER = c.String(maxLength: 2147483647),
                        PROG_PW = c.String(maxLength: 2147483647),
                        SET_NUMBER = c.Int(nullable: false),
                        STARTDATE = c.DateTime(nullable: false),
                        TASK_ID = c.Int(nullable: false),
                        TASK_NAME = c.String(maxLength: 2147483647),
                    })
                .PrimaryKey(t => t.F_ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Schedules");
            DropTable("dbo.Nhomquyens");
            DropTable("dbo.Nguoidungs");
            DropTable("dbo.Meters");
            DropTable("dbo.Lines");
            DropTable("dbo.Configs");
            DropTable("dbo.Capmatudongs");
        }
    }
}
