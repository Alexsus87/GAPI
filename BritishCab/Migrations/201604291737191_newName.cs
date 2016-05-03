namespace BritishCab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newName : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bookings",
                c => new
                    {
                        BookingId = c.Int(nullable: false, identity: true),
                        PickUpLocation = c.String(),
                        PickUpAddress = c.String(),
                        DropLocation = c.String(),
                        DropAddress = c.String(),
                        PickUpDateTime = c.DateTime(nullable: false),
                        DriverActualDepartureTime = c.DateTime(nullable: false),
                        TransferTime = c.Time(nullable: false, precision: 7),
                        TotalTime = c.Time(nullable: false, precision: 7),
                        DrivingDistance = c.Double(nullable: false),
                        TotalDrivingDistance = c.Double(nullable: false),
                        ErrorMessage = c.String(),
                        PhoneNumber = c.String(),
                        Email = c.String(),
                        IsSlotAvailable = c.Boolean(nullable: false),
                        Name = c.String(),
                        Price = c.Double(nullable: false),
                        ConfirmationCode = c.Guid(nullable: false),
                        Comments = c.String(),
                        BookingStatus = c.Int(nullable: false),
                        NumberOfPassengers = c.Int(nullable: false),
                        NumberOfLuggage = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BookingId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Bookings");
        }
    }
}
