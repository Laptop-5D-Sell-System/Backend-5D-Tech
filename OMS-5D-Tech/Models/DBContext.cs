using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace OMS_5D_Tech.Models
{
    public partial class DBContext : DbContext
    {
        public DBContext()
            : base("name=DBContext")
        {
        }

        public virtual DbSet<tbl_Accounts> tbl_Accounts { get; set; }
        public virtual DbSet<tbl_Cart> tbl_Cart { get; set; }
        public virtual DbSet<tbl_Categories> tbl_Categories { get; set; }
        public virtual DbSet<tbl_Feedbacks> tbl_Feedbacks { get; set; }
        public virtual DbSet<tbl_Order_Items> tbl_Order_Items { get; set; }
        public virtual DbSet<tbl_Orders> tbl_Orders { get; set; }
        public virtual DbSet<tbl_Payments> tbl_Payments { get; set; }
        public virtual DbSet<tbl_Products> tbl_Products { get; set; }
        public virtual DbSet<tbl_Reports> tbl_Reports { get; set; }
        public virtual DbSet<tbl_Reviews> tbl_Reviews { get; set; }
        public virtual DbSet<tbl_Users> tbl_Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<tbl_Accounts>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Accounts>()
                .Property(e => e.password_hash)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Accounts>()
                .Property(e => e.refresh_token)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Accounts>()
                .Property(e => e.role)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Accounts>()
                .HasMany(e => e.tbl_Users)
                .WithOptional(e => e.tbl_Accounts)
                .HasForeignKey(e => e.account_id)
                .WillCascadeOnDelete();

            modelBuilder.Entity<tbl_Categories>()
                .Property(e => e.name)
                .IsUnicode(true);

            modelBuilder.Entity<tbl_Categories>()
                .HasMany(e => e.tbl_Products)
                .WithOptional(e => e.tbl_Categories)
                .HasForeignKey(e => e.category_id);

            modelBuilder.Entity<tbl_Order_Items>()
                .Property(e => e.price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<tbl_Orders>()
                .Property(e => e.status)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Orders>()
                .Property(e => e.total)
                .HasPrecision(10, 2);

            modelBuilder.Entity<tbl_Orders>()
                .HasMany(e => e.tbl_Order_Items)
                .WithOptional(e => e.tbl_Orders)
                .HasForeignKey(e => e.order_id);

            modelBuilder.Entity<tbl_Orders>()
                .HasMany(e => e.tbl_Payments)
                .WithOptional(e => e.tbl_Orders)
                .HasForeignKey(e => e.order_id);

            modelBuilder.Entity<tbl_Payments>()
                .Property(e => e.payment_method)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Payments>()
                .Property(e => e.amount);

            modelBuilder.Entity<tbl_Payments>()
                .Property(e => e.status)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Products>()
                .Property(e => e.name)
                .IsUnicode(true);

            modelBuilder.Entity<tbl_Products>()
                .Property(e => e.price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<tbl_Products>()
                .Property(e => e.product_image)
                .IsUnicode(true);

            modelBuilder.Entity<tbl_Products>()
                .HasMany(e => e.tbl_Cart)
                .WithOptional(e => e.tbl_Products)
                .HasForeignKey(e => e.product_id);

            modelBuilder.Entity<tbl_Products>()
                .HasMany(e => e.tbl_Order_Items)
                .WithOptional(e => e.tbl_Products)
                .HasForeignKey(e => e.product_id);

            modelBuilder.Entity<tbl_Products>()
                .HasMany(e => e.tbl_Reviews)
                .WithOptional(e => e.tbl_Products)
                .HasForeignKey(e => e.product_id);

            modelBuilder.Entity<tbl_Reports>()
                .Property(e => e.report_type)
                .IsUnicode(true);

            modelBuilder.Entity<tbl_Users>()
                .Property(e => e.first_name)
                .IsUnicode(true);

            modelBuilder.Entity<tbl_Users>()
                .Property(e => e.last_name)
                .IsUnicode(true);

            modelBuilder.Entity<tbl_Users>()
                .Property(e => e.phone_number)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Users>()
                .Property(e => e.profile_picture)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_Users>()
                .HasMany(e => e.tbl_Cart)
                .WithOptional(e => e.tbl_Users)
                .HasForeignKey(e => e.user_id)
                .WillCascadeOnDelete();

            modelBuilder.Entity<tbl_Users>()
                .HasMany(e => e.tbl_Feedbacks)
                .WithOptional(e => e.tbl_Users)
                .HasForeignKey(e => e.user_id);

            modelBuilder.Entity<tbl_Users>()
                .HasMany(e => e.tbl_Orders)
                .WithOptional(e => e.tbl_Users)
                .HasForeignKey(e => e.user_id)
                .WillCascadeOnDelete();

            modelBuilder.Entity<tbl_Users>()
                .HasMany(e => e.tbl_Reviews)
                .WithOptional(e => e.tbl_Users)
                .HasForeignKey(e => e.user_id);
        }
    }
}
