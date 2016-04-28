namespace BritishCab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BookingStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookingEntities", "BookingStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BookingEntities", "BookingStatus");
        }
    }
}
