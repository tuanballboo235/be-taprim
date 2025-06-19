using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TAPrim.Models;

public partial class TaprimContext : DbContext
{
    public TaprimContext()
    {
    }

    public TaprimContext(DbContextOptions<TaprimContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<MailTemplate> MailTemplates { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAccount> ProductAccounts { get; set; }

    public virtual DbSet<TempMailEmailStore> TempMailEmailStores { get; set; }

    public virtual DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("Category_pk");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryId).HasColumnName("categoryId");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("categoryName");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("createAt");
            entity.Property(e => e.ParentId).HasColumnName("parentId");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Categories_Parent");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("Coupon_pk");

            entity.ToTable("Coupon");

            entity.Property(e => e.CouponId)
                .ValueGeneratedNever()
                .HasColumnName("couponId");
            entity.Property(e => e.CouponCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("couponCode");
            entity.Property(e => e.CreateAt).HasColumnName("createAt");
            entity.Property(e => e.DiscountPercent).HasColumnName("discountPercent");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.RemainTurn).HasColumnName("remainTurn");
            entity.Property(e => e.ValidFrom)
                .HasColumnType("datetime")
                .HasColumnName("validFrom");
            entity.Property(e => e.ValidUntil)
                .HasColumnType("datetime")
                .HasColumnName("validUntil");
        });

        modelBuilder.Entity<MailTemplate>(entity =>
        {
            entity.HasKey(e => e.MailTemplateId).HasName("MailTemplate_pk");

            entity.ToTable("MailTemplate");

            entity.Property(e => e.MailTemplateId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.TemplateTitle).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("Order_pk");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.ClientNote).HasColumnName("clientNote");
            entity.Property(e => e.ContactInfo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("contactInfo");
            entity.Property(e => e.CouponId).HasColumnName("couponId");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("createAt");
            entity.Property(e => e.ExpiredAt)
                .HasColumnType("datetime")
                .HasColumnName("expiredAt");
            entity.Property(e => e.PaymentId).HasColumnName("paymentId");
            entity.Property(e => e.ProductAccountId).HasColumnName("productAccountId");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.RemainGetCode).HasColumnName("remainGetCode");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("totalAmount");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("updateAt");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("FK_Order_Coupon");

            entity.HasOne(d => d.Payment).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Payment");

            entity.HasOne(d => d.ProductAccount).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ProductAccountId)
                .HasConstraintName("Order__productAccountId_fk");

            entity.HasOne(d => d.Product).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order__productId_fk");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("Payment_pk");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).HasColumnName("paymentId");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("amount");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("createAt");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PaidDateAt)
                .HasColumnType("datetime")
                .HasColumnName("paidDateAt");
            entity.Property(e => e.PaymentMethod).HasColumnName("paymentMethod");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("transactionCode");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Payment__userId_fk");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("table_name_pk");

            entity.ToTable("Product");

            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.AttentionNote).HasColumnName("attentionNote");
            entity.Property(e => e.CategoryId).HasColumnName("categoryId");
            entity.Property(e => e.Description)
                .HasMaxLength(1)
                .HasColumnName("description");
            entity.Property(e => e.DiscountPercentDisplay).HasColumnName("discountPercentDisplay");
            entity.Property(e => e.DurationDay).HasColumnName("durationDay");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("productCode");
            entity.Property(e => e.ProductImage)
                .IsUnicode(false)
                .HasColumnName("productImage");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .HasColumnName("productName");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Product_Category__fk");
        });

        modelBuilder.Entity<ProductAccount>(entity =>
        {
            entity.HasKey(e => e.ProductAccountId).HasName("ProductAccount_pk");

            entity.ToTable("ProductAccount");

            entity.Property(e => e.ProductAccountId).HasColumnName("productAccountId");
            entity.Property(e => e.AccountData).HasColumnName("accountData");
            entity.Property(e => e.DateChangePass)
                .HasColumnType("datetime")
                .HasColumnName("dateChangePass");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PasswordProductAccount).HasColumnName("passwordProductAccount");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.SellCount).HasColumnName("sellCount");
            entity.Property(e => e.SellFrom)
                .HasColumnType("datetime")
                .HasColumnName("sellFrom");
            entity.Property(e => e.SellTo)
                .HasColumnType("datetime")
                .HasColumnName("sellTo");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UsernameProductAccount).HasColumnName("usernameProductAccount");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAccounts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ProductAccount_Product__fk");
        });

        modelBuilder.Entity<TempMailEmailStore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TempMailEmailStore_pk");

            entity.ToTable("TempMailEmailStore");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.EmailId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("email_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("User_pk");

            entity.ToTable("User");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("createAt");
            entity.Property(e => e.IsEnable).HasColumnName("isEnable");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(30)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
