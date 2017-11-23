namespace WebApp1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.loginUsers", "Viewable", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.loginUsers", "Viewable");
        }
    }
}
