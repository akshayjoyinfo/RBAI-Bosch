using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RBAI.Domain;
using RBAI.Repository;

namespace RBAI.UI
{
    public partial class frmAdminPanel : Form
    {
        public frmAdminPanel()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
          clearAllValues();
        }

        private void clearAllValues()
        {
            txtPartNumber.Text = "";
            txtCustomer.Text = "";
            txtDescription.Text = "";
            txtMin.Text = "";
            txtMax.Text = "";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string partNumber = txtPartNumber.Text;
            int min = txtMin.Text!=""?Convert.ToInt32(txtMin.Text):0;
            int max = txtMax.Text!=""?Convert.ToInt32(txtMax.Text):0;
            string customer = txtCustomer.Text;
            string description = txtDescription.Text;
            var item = new InventoryItem(){PartNumber = partNumber,Min = min,Max = max , Customer = customer,Description = description};

            if (ValidateNullValues(item))
            {
                if (!InventoryRepository.CheckPartNumberExist(item))
                {
                    bool result = InventoryRepository.InsertPartNumberAdmin(item);
                    if (result == true)
                    {
                        ShowMessage("Successfully inserted Partnumber", MessageBoxIcon.Information);
                        clearAllValues();
                    }
                    else
                    {
                        ShowMessage("Unable to Insert PartNumber to Master table", MessageBoxIcon.Information);
                    }
                }
                else
                {
                    ShowMessage("Partnumber already exist", MessageBoxIcon.Information);
                }
            }
        }

        private bool ValidateNullValues(InventoryItem item)
        {
            lblRegPartNumber.Visible = false;
            lblReqCustomer.Visible = false;
            lblReqDescription.Visible = false;
            lblReqMax.Visible = false;
            lblReqMin.Visible = false;
            if (string.IsNullOrEmpty(item.PartNumber))
            {
                ShowMessage("PartNumber should not empty", MessageBoxIcon.Warning);
                lblRegPartNumber.Visible = true;
                return false;
            }
            if (string.IsNullOrEmpty(item.Customer))
            {
                ShowMessage("Customer should not empty", MessageBoxIcon.Warning);
                lblReqCustomer.Visible = true;
                return false;
            }
           if (string.IsNullOrEmpty(item.Description))
            {
                ShowMessage("Description should not empty", MessageBoxIcon.Warning);
                lblReqDescription.Visible = true;
                    return false;
                
            }
            if (item.Max <=0)
            {
                    ShowMessage("Max should be greater than ZERO", MessageBoxIcon.Warning);
                    lblReqMax.Visible = true;
                    return false;
            }
            if (item.Min <= 0)
            {
                ShowMessage("Min should be greater than ZERO", MessageBoxIcon.Warning);
                lblReqMax.Visible = true;
                return false;
            }
            return true;
        }
        public static void ShowMessage(string text, MessageBoxIcon icon)
        {
            MessageBox.Show(text, "RBAI Admin", MessageBoxButtons.OK, icon);
        }

        private void txtPartNumber_Leave(object sender, EventArgs e)
        {
            string partNumber = txtPartNumber.Text;
            var item = new InventoryItem() { PartNumber = partNumber };
            if (InventoryRepository.CheckPartNumberExist(item))
            {
                ShowMessage("PartNumber already exist! Please try different partnumber", MessageBoxIcon.Warning);
                lblRegPartNumber.Visible = true;
                
            }

        }
    }
}
