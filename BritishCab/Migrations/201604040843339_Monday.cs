namespace BritishCab.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Monday : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BookingEntities", "PickUpLocation", c => c.String());
            AlterColumn("dbo.BookingEntities", "DropLocation", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.BookingEntities", "DropLocation", c => c.String(nullable: false));
            AlterColumn("dbo.BookingEntities", "PickUpLocation", c => c.String(nullable: false));
        }
    }
}
