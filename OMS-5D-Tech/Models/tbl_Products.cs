namespace OMS_5D_Tech.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Products
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Products()
        {
            tbl_Cart = new HashSet<tbl_Cart>();
            tbl_Order_Items = new HashSet<tbl_Order_Items>();
            tbl_Reviews = new HashSet<tbl_Reviews>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(255)]
        public string name { get; set; }

        public string description { get; set; }

        public decimal price { get; set; }

        [Required]
        [StringLength(255)]
        public string product_image { get; set; }

        public int stock_quantity { get; set; }

        public int? category_id { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Cart> tbl_Cart { get; set; }

        public virtual tbl_Categories tbl_Categories { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Order_Items> tbl_Order_Items { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Reviews> tbl_Reviews { get; set; }
    }
}
