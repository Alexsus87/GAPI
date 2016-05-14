namespace BritishCab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DateOfBooking : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bookings", "BookingDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bookings", "BookingDate");
        }
    }
}
