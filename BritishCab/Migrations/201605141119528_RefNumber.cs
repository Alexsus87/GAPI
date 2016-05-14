namespace BritishCab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bookings", "RefNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bookings", "RefNumber");
        }
    }
}
