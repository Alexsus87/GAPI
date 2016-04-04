namespace BritishCab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddressAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookingEntities", "PickUpAddress", c => c.String());
            AddColumn("dbo.BookingEntities", "DropAddress", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BookingEntities", "DropAddress");
            DropColumn("dbo.BookingEntities", "PickUpAddress");
        }
    }
}
