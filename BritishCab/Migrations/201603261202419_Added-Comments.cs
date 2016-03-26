namespace BritishCab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedComments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookingEntities", "Comments", c => c.String());
            AlterColumn("dbo.BookingEntities", "DrivingDistance", c => c.Double(nullable: false));
            AlterColumn("dbo.BookingEntities", "TotalDrivingDistance", c => c.Double(nullable: false));
            DropColumn("dbo.BookingEntities", "PickUpTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BookingEntities", "PickUpTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.BookingEntities", "TotalDrivingDistance", c => c.Int(nullable: false));
            AlterColumn("dbo.BookingEntities", "DrivingDistance", c => c.Int(nullable: false));
            DropColumn("dbo.BookingEntities", "Comments");
        }
    }
}
