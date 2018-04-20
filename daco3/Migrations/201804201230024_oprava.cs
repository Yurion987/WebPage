namespace daco3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class oprava : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Log", "StaraHodnota", c => c.DateTime());
        }

        public override void Down()
        {
            AlterColumn("dbo.Log", "StaraHodnota", c => c.DateTime(nullable: false));
        }
    }
}
