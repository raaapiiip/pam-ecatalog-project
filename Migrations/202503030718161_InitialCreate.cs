namespace ItemListApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                {
                    Category_id = c.Int(nullable: false, identity: true),
                    Category_name = c.String(),
                })
                .PrimaryKey(t => t.Category_id);

            CreateTable(
                "dbo.Products",
                c => new
                {
                    Product_id = c.Int(nullable: false, identity: true),
                    Product_Category_id = c.Int(),
                    Product_code_number = c.String(),
                    Product_accessories_name = c.String(),
                    Product_qty = c.Int(),
                    Product_owner = c.String(),
                    Product_dept = c.String(),
                    Product_drawing_filepath = c.String(),
                    Product_photo_filepath = c.String(),
                    Product_eccn = c.String(),
                    Product_hs_code = c.String(),
                    Product_vendor_name = c.String(),
                    Product_quotation_filepath = c.String(),
                    Product_last_modification = c.DateTime(),
                    Product_function = c.String(),
                    Product_remark = c.String(),
                })
                .PrimaryKey(t => t.Product_id)
                .ForeignKey("dbo.Categories", t => t.Product_Category_id)
                .Index(t => t.Product_Category_id);

        }

        public override void Down()
        {
            DropForeignKey("dbo.Products", "Product_Category_id", "dbo.Categories");
            DropIndex("dbo.Products", new[] { "Product_Category_id" });
            DropTable("dbo.Products");
            DropTable("dbo.Categories");
        }
    }
}
