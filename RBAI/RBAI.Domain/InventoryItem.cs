using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBAI.Domain
{
    public class InventoryItem
    {
        [DisplayName("PART NUMBER")]
        public string PartNumber { get; set; }
        [DisplayName("DESCRIPTION")]
        public string Description { get; set; }
        [DisplayName("INVOICE NUMBER")]
        public string InvoiceNo { get; set; }
         [DisplayName("PALLET NUMBER")]
        public string PalletNo { get; set; }

        [DisplayName("CUSTOMER")]
        public string Customer { get; set; }
        [DisplayName("MIN")]
        public int Min { get; set; }
        [DisplayName("MAX")]
        public int Max { get; set; }
        [DisplayName("ACTUAL")]
        public int CurrentStock { get; set; }
    }

    public class InventoryDaiyFact
    {
        [DisplayName("PART NUMBER")]
        public string PartNumber { get; set; }

        [DisplayName("QUANTITY")]
        public int Quantity { get; set; }

        [Browsable(false)]
        public string InvoiceNo { get; set; }
        [Browsable(false)]
        public string PalletNo { get; set; }

         [DisplayName("TRANSACTION DATE")]
        public DateTime TransactionDate { get; set; }

        [DisplayName("TRANSACTION TYPE")]
        public string IsAddOrTrans { get; set; }

        

    }

    public enum InventoryTransactionType
    {
        Add =1,
        Restore =2
    }

    public class RestoreTransactionStatus
    {
        public string Message { get; set; }
        public bool Valid { get; set; }
    }
}
