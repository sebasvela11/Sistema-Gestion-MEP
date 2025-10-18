namespace Sistema_Gestion_MEP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateModelsTransacciones : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Documents", "SpecialtyId", "dbo.Specialties");
            DropForeignKey("dbo.Documents", "TermId", "dbo.Terms");
            DropIndex("dbo.Documents", new[] { "SpecialtyId" });
            DropIndex("dbo.Documents", new[] { "TermId" });
            AddColumn("dbo.Documents", "SpecialtyAccessId", c => c.Int(nullable: false));
            AddColumn("dbo.Documents", "PriceCRC", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PaymentSimulations", "SpecialtyAccessId", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentSimulations", "DocumentId", c => c.Int());
            AddColumn("dbo.PaymentSimulations", "PaymentType", c => c.String());
            CreateIndex("dbo.Documents", "SpecialtyAccessId");
            CreateIndex("dbo.PaymentSimulations", "SpecialtyAccessId");
            CreateIndex("dbo.PaymentSimulations", "DocumentId");
            AddForeignKey("dbo.Documents", "SpecialtyAccessId", "dbo.SpecialtyAccesses", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PaymentSimulations", "DocumentId", "dbo.Documents", "Id");
            AddForeignKey("dbo.PaymentSimulations", "SpecialtyAccessId", "dbo.SpecialtyAccesses", "Id", cascadeDelete: true);
            DropColumn("dbo.Documents", "SpecialtyId");
            DropColumn("dbo.Documents", "TermId");
            DropColumn("dbo.PaymentSimulations", "SpecialtyId");
            DropColumn("dbo.PaymentSimulations", "TermId");
            DropColumn("dbo.SpecialtyAccesses", "PriceCRC");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SpecialtyAccesses", "PriceCRC", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.PaymentSimulations", "TermId", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentSimulations", "SpecialtyId", c => c.Int(nullable: false));
            AddColumn("dbo.Documents", "TermId", c => c.Int(nullable: false));
            AddColumn("dbo.Documents", "SpecialtyId", c => c.Int(nullable: false));
            DropForeignKey("dbo.PaymentSimulations", "SpecialtyAccessId", "dbo.SpecialtyAccesses");
            DropForeignKey("dbo.PaymentSimulations", "DocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "SpecialtyAccessId", "dbo.SpecialtyAccesses");
            DropIndex("dbo.PaymentSimulations", new[] { "DocumentId" });
            DropIndex("dbo.PaymentSimulations", new[] { "SpecialtyAccessId" });
            DropIndex("dbo.Documents", new[] { "SpecialtyAccessId" });
            DropColumn("dbo.PaymentSimulations", "PaymentType");
            DropColumn("dbo.PaymentSimulations", "DocumentId");
            DropColumn("dbo.PaymentSimulations", "SpecialtyAccessId");
            DropColumn("dbo.Documents", "PriceCRC");
            DropColumn("dbo.Documents", "SpecialtyAccessId");
            CreateIndex("dbo.Documents", "TermId");
            CreateIndex("dbo.Documents", "SpecialtyId");
            AddForeignKey("dbo.Documents", "TermId", "dbo.Terms", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Documents", "SpecialtyId", "dbo.Specialties", "Id", cascadeDelete: true);
        }
    }
}
