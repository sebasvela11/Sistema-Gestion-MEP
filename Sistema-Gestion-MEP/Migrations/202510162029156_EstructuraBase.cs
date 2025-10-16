namespace Sistema_Gestion_MEP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EstructuraBase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActivityLogs",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        Action = c.String(),
                        Entity = c.String(),
                        EntityId = c.String(),
                        Info = c.String(),
                        Ip = c.String(),
                        UserAgent = c.String(),
                        Success = c.Boolean(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Byte(nullable: false),
                        OwnerUserId = c.String(maxLength: 128),
                        SpecialtyId = c.Int(nullable: false),
                        TermId = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 255),
                        StoredPath = c.String(nullable: false, maxLength: 500),
                        FileSizeBytes = c.Long(),
                        UploadedAtUtc = c.DateTime(nullable: false),
                        DeadlineUtc = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.OwnerUserId)
                .ForeignKey("dbo.Specialties", t => t.SpecialtyId, cascadeDelete: true)
                .ForeignKey("dbo.Terms", t => t.TermId, cascadeDelete: true)
                .Index(t => t.OwnerUserId)
                .Index(t => t.SpecialtyId)
                .Index(t => t.TermId);
            
            CreateTable(
                "dbo.Specialties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 120),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Terms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Year = c.Int(nullable: false),
                        Label = c.String(nullable: false, maxLength: 50),
                        OrderInYear = c.Byte(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        CreatedAtUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PaymentSimulations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        SpecialtyId = c.Int(nullable: false),
                        TermId = c.Int(nullable: false),
                        AmountCRC = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(maxLength: 20),
                        Reference = c.String(maxLength: 50),
                        PaidAtUtc = c.DateTime(),
                        CreatedAtUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Specialties", t => t.SpecialtyId, cascadeDelete: true)
                .ForeignKey("dbo.Terms", t => t.TermId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.SpecialtyId)
                .Index(t => t.TermId);
            
            CreateTable(
                "dbo.SpecialtyAccesses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        SpecialtyId = c.Int(nullable: false),
                        TermId = c.Int(nullable: false),
                        AccessGrantedUtc = c.DateTime(nullable: false),
                        DeadlineUtc = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Specialties", t => t.SpecialtyId, cascadeDelete: true)
                .ForeignKey("dbo.Terms", t => t.TermId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.SpecialtyId)
                .Index(t => t.TermId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SpecialtyAccesses", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.SpecialtyAccesses", "TermId", "dbo.Terms");
            DropForeignKey("dbo.SpecialtyAccesses", "SpecialtyId", "dbo.Specialties");
            DropForeignKey("dbo.PaymentSimulations", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentSimulations", "TermId", "dbo.Terms");
            DropForeignKey("dbo.PaymentSimulations", "SpecialtyId", "dbo.Specialties");
            DropForeignKey("dbo.Documents", "TermId", "dbo.Terms");
            DropForeignKey("dbo.Documents", "SpecialtyId", "dbo.Specialties");
            DropForeignKey("dbo.Documents", "OwnerUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ActivityLogs", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.SpecialtyAccesses", new[] { "TermId" });
            DropIndex("dbo.SpecialtyAccesses", new[] { "SpecialtyId" });
            DropIndex("dbo.SpecialtyAccesses", new[] { "UserId" });
            DropIndex("dbo.PaymentSimulations", new[] { "TermId" });
            DropIndex("dbo.PaymentSimulations", new[] { "SpecialtyId" });
            DropIndex("dbo.PaymentSimulations", new[] { "UserId" });
            DropIndex("dbo.Documents", new[] { "TermId" });
            DropIndex("dbo.Documents", new[] { "SpecialtyId" });
            DropIndex("dbo.Documents", new[] { "OwnerUserId" });
            DropIndex("dbo.ActivityLogs", new[] { "UserId" });
            DropTable("dbo.SpecialtyAccesses");
            DropTable("dbo.PaymentSimulations");
            DropTable("dbo.Terms");
            DropTable("dbo.Specialties");
            DropTable("dbo.Documents");
            DropTable("dbo.ActivityLogs");
        }
    }
}
