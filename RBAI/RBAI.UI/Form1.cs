using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RBAI.Domain;
using RBAI.Repository;

namespace RBAI.UI
{
    public partial class frmMainUI : Form
    {
        public AutoCompleteStringCollection partNumberAutoComplete = new AutoCompleteStringCollection();
        public frmMainUI()
        {
            InitializeComponent();
            LoadInitialSettings();
            SetAutoCompleteSources();
        }

        private void SetAutoCompleteSources()
        {
            List<string> listPartNumbers = InventoryRepository.GetPartNumbers();
            txtPartNumber.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtPartNumber.AutoCompleteSource = AutoCompleteSource.CustomSource;
            partNumberAutoComplete.AddRange(listPartNumbers.ToArray());
            txtPartNumber.AutoCompleteCustomSource = partNumberAutoComplete;
            
        }
        public void LoadInitialSettings()
        {
            lblDate.Text = DateTime.Now.ToShortDateString();
            lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void timerDigitalClock_Tick(object sender, EventArgs e)
        {
            lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            
            string partNumber = txtPartNumber.Text;
            int qunatity = txtQuantity.Text!=""?Convert.ToInt32(txtQuantity.Text):0;
            string palletNumber = txtPalletNumber.Text;
            var item = new InventoryItem(){PartNumber = partNumber,CurrentStock = qunatity,PalletNo =palletNumber };

            if (ValidateNullValues(item, InventoryTransactionType.Add))
            {
                if (ValidateTransactionAction(txtPalletNumber.Text, InventoryTransactionType.Add))
                {
                    bool valid = InventoryRepository.AddPartNumber(item);
                    if(valid)
                        ShowMessage("Add quantity "+ item.CurrentStock + " to PartNumber " + item.PartNumber,MessageBoxIcon.Information);
                    else
                        ShowMessage("Error while adding quantity partnumber", MessageBoxIcon.Information);
                }
            }
        }

        private bool ValidateNullValues(InventoryItem item, InventoryTransactionType transAction)
        {
            lblReqPartnumber.Visible = false;
            lblQuantity.Visible = false;
            lblReqPartnumber.Visible = false;
            lblReqPartnumber.Visible = false;
            if (string.IsNullOrEmpty(item.PartNumber))
            {
                ShowMessage("PartNumber should not empty", MessageBoxIcon.Warning);
                lblReqPartnumber.Visible = true;
                return false;
            }
            if (item.CurrentStock<=0)
            {
                ShowMessage("Quantity should be greater than Zero", MessageBoxIcon.Warning);
                lblQuantity.Visible = true;
                return false;
            }
            if (transAction == InventoryTransactionType.Add || transAction == InventoryTransactionType.Restore)
            {
                if (string.IsNullOrEmpty(item.PalletNo))
                {
                    ShowMessage("Palletnumber should not empty", MessageBoxIcon.Warning);
                    lblReqPalletNumber.Visible = true;
                    return false;
                }
            }
            if (transAction == InventoryTransactionType.Restore)
            {
                if (item.InvoiceNo != null && string.IsNullOrEmpty(item.InvoiceNo))
                {
                    ShowMessage("Invoice Number should not be empty", MessageBoxIcon.Warning);
                    lblReqInvoiceNumber.Visible = true;
                    return false;
                }
            }
            lblReqPartnumber.Visible = true;
            lblQuantity.Visible = true;
            lblReqPartnumber.Visible = true;
            lblReqPartnumber.Visible = true;
            return true;

        }

        private void txtPalletNumber_Leave(object sender, EventArgs e)
        {
            
        }


        public static bool ValidateTransactionAction(string palletNo, InventoryTransactionType transAction,string invoiceNo="")
        {
           
            if (transAction == InventoryTransactionType.Add)
            {
                if (string.IsNullOrEmpty(palletNo))
                {
                    ShowMessage("Either PalletNo/InvoiceNo is empty !", MessageBoxIcon.Warning);
                    return false;
                }
                if (!InventoryRepository.ValidateInvoiceTransaction(palletNo, transAction))
                {
                    ShowMessage("Pallet Number already exist ! Please try different one", MessageBoxIcon.Error);
                    return false;
                }
            }
            else if (transAction == InventoryTransactionType.Restore)
            {
                if (string.IsNullOrEmpty(palletNo) || string.IsNullOrEmpty(invoiceNo))
                {
                    ShowMessage("Either PalletNo/InvoiceNo is empty !", MessageBoxIcon.Warning);
                    return false;
                }
                if (!InventoryRepository.ValidateInvoiceTransaction(palletNo, transAction, invoiceNo))
                {
                    ShowMessage("Pallet Number or Invoice Number already exist ! Please try different one", MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;

        }

        

        public static void ShowMessage(string text, MessageBoxIcon icon)
        {
            MessageBox.Show(text, "RBAI Log", MessageBoxButtons.OK, icon);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ClearAllFields();
        }

        private void ClearAllFields()
        {
            txtPartNumber.Text = "";
            txtQuantity.Text = "";
            txtInvoiceNumber.Text = "";
            txtPalletNumber.Text = "";
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
             string partNumber = txtPartNumber.Text;
            int qunatity = txtQuantity.Text!=""?Convert.ToInt32(txtQuantity.Text):0;
            string palletNumber = txtPalletNumber.Text;
            string inoviceNumber = txtPalletNumber.Text;
            var item = new InventoryItem() { PartNumber = partNumber, CurrentStock = qunatity, PalletNo = palletNumber, InvoiceNo = inoviceNumber };

            if (ValidateNullValues(item, InventoryTransactionType.Add))
            {
                var obj = InventoryRepository.ValidateRestoreTransaction(item,
                    InventoryTransactionType.Restore
                    )   ;
                if (obj.Valid == true)
                {
                    bool valid = InventoryRepository.RestorePartNumber(item);
                    if (valid)
                        ShowMessage("Add quantity " + item.CurrentStock + " to PartNumber " + item.PartNumber,
                            MessageBoxIcon.Information);
                    else
                        ShowMessage("Error while adding quantity partnumber", MessageBoxIcon.Information);
                }
                else
                {
                    ShowMessage("Unable to perform Resotre. Reason : - " + obj.Message, MessageBoxIcon.Information);
                }
            }
        }

        private void txtPartNumber_Leave(object sender, EventArgs e)
        {
            lblcurrentStock.Visible = true;
            lblcurrentStock.Text = "( " + Convert.ToString(InventoryRepository.GetCurrentStockByPartNumber(txtPartNumber.Text)) + " )";
        }

        private void adminPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAdminPanel obj = new frmAdminPanel();
            obj.ShowDialog();
        }

        private void closeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
