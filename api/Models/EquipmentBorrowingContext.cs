using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace api.Models;

public partial class EquipmentBorrowingContext : DbContext
{
    public EquipmentBorrowingContext()
    {
    }

    public EquipmentBorrowingContext(DbContextOptions<EquipmentBorrowingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<ItemCategory> ItemCategories { get; set; }

    public virtual DbSet<ItemClassification> ItemClassifications { get; set; }

    public virtual DbSet<ItemInstance> ItemInstances { get; set; }

    public virtual DbSet<RequisitionedItem> RequisitionedItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=equipmentBorrowing;user=root;password=TadPhi@2276", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.2.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PRIMARY");

            entity.ToTable("employees");

            entity.Property(e => e.EmployeeId).HasColumnName("employeeId");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("updateDate");
        });

        modelBuilder.Entity<ItemCategory>(entity =>
        {
            entity.HasKey(e => e.ItemCategoryId).HasName("PRIMARY");

            entity.ToTable("itemCategories");

            entity.Property(e => e.ItemCategoryId).HasColumnName("itemCategoryId");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("updateDate");
        });

        modelBuilder.Entity<ItemClassification>(entity =>
        {
            entity.HasKey(e => e.ItemClassificationId).HasName("PRIMARY");

            entity.ToTable("itemClassifications");

            entity.HasIndex(e => e.ItemCategoryId, "FK_itemClassifications_itemCategoryId");

            entity.Property(e => e.ItemClassificationId).HasColumnName("itemClassificationId");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            entity.Property(e => e.ItemCategoryId).HasColumnName("itemCategoryId");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("updateDate");

            entity.HasOne(d => d.ItemCategory).WithMany(p => p.ItemClassifications)
                .HasForeignKey(d => d.ItemCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_itemClassifications_itemCategoryId");
        });

        modelBuilder.Entity<ItemInstance>(entity =>
        {
            entity.HasKey(e => e.ItemInstanceId).HasName("PRIMARY");

            entity.ToTable("itemInstances");

            entity.HasIndex(e => e.ItemClassificationId, "FK_itemInstances_itemClassificationId");

            entity.HasIndex(e => e.RequisitionId, "FK_itemInstances_requisitionId");

            entity.Property(e => e.ItemInstanceId).HasColumnName("itemInstanceId");
            entity.Property(e => e.AssetId)
                .HasMaxLength(50)
                .HasColumnName("assetId");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            entity.Property(e => e.ItemClassificationId).HasColumnName("itemClassificationId");
            entity.Property(e => e.RequisitionId).HasColumnName("requisitionId");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("updateDate");

            entity.HasOne(d => d.ItemClassification).WithMany(p => p.ItemInstances)
                .HasForeignKey(d => d.ItemClassificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_itemInstances_itemClassificationId");

            entity.HasOne(d => d.Requisition).WithMany(p => p.ItemInstances)
                .HasForeignKey(d => d.RequisitionId)
                .HasConstraintName("FK_itemInstances_requisitionId");
        });

        modelBuilder.Entity<RequisitionedItem>(entity =>
        {
            entity.HasKey(e => e.RequisitionId).HasName("PRIMARY");

            entity.ToTable("requisitionedItems");

            entity.HasIndex(e => e.EmployeeId, "FK_ requisitionedItems_employeeId");

            entity.HasIndex(e => e.ItemInstanceId, "FK_ requisitionedItems_itemInstanceId");

            entity.Property(e => e.RequisitionId).HasColumnName("requisitionId");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            entity.Property(e => e.EmployeeId).HasColumnName("employeeId");
            entity.Property(e => e.ItemInstanceId).HasColumnName("itemInstanceId");
            entity.Property(e => e.RequisitonDate)
                .HasColumnType("datetime")
                .HasColumnName("requisitonDate");
            entity.Property(e => e.ReturnDate)
                .HasColumnType("datetime")
                .HasColumnName("returnDate");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("updateDate");

            entity.HasOne(d => d.Employee).WithMany(p => p.RequisitionedItems)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ requisitionedItems_employeeId");

            entity.HasOne(d => d.ItemInstance).WithMany(p => p.RequisitionedItems)
                .HasForeignKey(d => d.ItemInstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ requisitionedItems_itemInstanceId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
