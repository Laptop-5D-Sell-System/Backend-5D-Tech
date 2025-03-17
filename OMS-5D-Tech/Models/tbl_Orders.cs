namespace OMS_5D_Tech.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Orders
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Orders()
        {
            tbl_Order_Items = new HashSet<tbl_Order_Items>();
            tbl_Payments = new HashSet<tbl_Payments>();
        }

        public int id { get; set; }

        public int? user_id { get; set; }

        public DateTime? order_date { get; set; }

        [StringLength(50)]
        public string status { get; set; }

        public decimal total { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Order_Items> tbl_Order_Items { get; set; }

        public virtual tbl_Users tbl_Users { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Payments> tbl_Payments { get; set; }
    }
}
