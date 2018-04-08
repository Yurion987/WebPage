namespace daco3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class heloZmena : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Uzivatel", "Heslo", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Uzivatel", "Heslo", c => c.String(nullable: false, maxLength: 20));
        }
    }
}
