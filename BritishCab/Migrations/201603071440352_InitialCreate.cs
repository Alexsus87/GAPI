namespace BritishCab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BookingEntities",
                c => new
                    {
                        BookingEntityId = c.Int(nullable: false, identity: true),
                        PickUpLocation = c.String(nullable: false),
                        DropLocation = c.String(nullable: false),
                        PickUpDateTime = c.DateTime(nullable: false),
                        PickUpTime = c.DateTime(nullable: false),
                        DriverActualDepartureTime = c.DateTime(nullable: false),
                        TransferTime = c.Time(nullable: false, precision: 7),
                        TotalTime = c.Time(nullable: false, precision: 7),
                        DrivingDistance = c.Int(nullable: false),
                        TotalDrivingDistance = c.Int(nullable: false),
                        ErrorMessage = c.String(),
                        PhoneNumber = c.String(),
                        Email = c.String(),
                        IsSlotAvailable = c.Boolean(nullable: false),
                        IsSlotCheckWasMade = c.Boolean(nullable: false),
                        Name = c.String(),
                        Price = c.Double(nullable: false),
                        ConfirmationCode = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.BookingEntityId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BookingEntities");
        }
    }
}
