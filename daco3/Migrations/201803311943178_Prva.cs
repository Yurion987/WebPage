namespace daco3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Prva : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Log",
                c => new
                    {
                        LogId = c.Int(nullable: false, identity: true),
                        ZaznamId = c.Long(nullable: false),
                        UzivatelId = c.Int(nullable: false),
                        StaraHodnota = c.DateTime(nullable: false),
                        ZmenaTypu = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.LogId)
                .ForeignKey("dbo.Uzivatel", t => t.UzivatelId, cascadeDelete: true)
                .ForeignKey("dbo.Zaznam", t => t.ZaznamId, cascadeDelete: true)
                .Index(t => t.ZaznamId)
                .Index(t => t.UzivatelId);
            
            CreateTable(
                "dbo.Uzivatel",
                c => new
                    {
                        UzivatelId = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 25),
                        Heslo = c.String(nullable: false, maxLength: 20),
                        RolaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UzivatelId)
                .ForeignKey("dbo.Rola", t => t.RolaId, cascadeDelete: true)
                .Index(t => t.Username, unique: true)
                .Index(t => t.RolaId);
            
            CreateTable(
                "dbo.Rola",
                c => new
                    {
                        RolaId = c.Int(nullable: false, identity: true),
                        Nazov = c.String(nullable: false, maxLength: 15),
                    })
                .PrimaryKey(t => t.RolaId);
            
            CreateTable(
                "dbo.Zaznam",
                c => new
                    {
                        ZaznamId = c.Long(nullable: false, identity: true),
                        ZaznamIdWeb = c.String(),
                        Cas = c.DateTime(nullable: false),
                        UzivatelId = c.Int(nullable: false),
                        Typ = c.String(nullable: false, maxLength: 1),
                    })
                .PrimaryKey(t => t.ZaznamId)
                .ForeignKey("dbo.Uzivatel", t => t.UzivatelId, cascadeDelete: false)
                .Index(t => t.UzivatelId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Log", "ZaznamId", "dbo.Zaznam");
            DropForeignKey("dbo.Zaznam", "UzivatelId", "dbo.Uzivatel");
            DropForeignKey("dbo.Log", "UzivatelId", "dbo.Uzivatel");
            DropForeignKey("dbo.Uzivatel", "RolaId", "dbo.Rola");
            DropIndex("dbo.Zaznam", new[] { "UzivatelId" });
            DropIndex("dbo.Uzivatel", new[] { "RolaId" });
            DropIndex("dbo.Uzivatel", new[] { "Username" });
            DropIndex("dbo.Log", new[] { "UzivatelId" });
            DropIndex("dbo.Log", new[] { "ZaznamId" });
            DropTable("dbo.Zaznam");
            DropTable("dbo.Rola");
            DropTable("dbo.Uzivatel");
            DropTable("dbo.Log");
        }
    }
}
