﻿using MSS.DO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSS.UI
{
    public partial class Sale : Form
    {
        int cleared = 0, remainType=0;
        DataEncryptor keys = new DataEncryptor();
        public Sale()
        {
            InitializeComponent();
            MaxDate();
        }
        private void MaxDate()
        {
            try
            {
                string encrypt_key = new DB.Configuration().GET().Code;
                DateTime maxDate = Convert.ToDateTime(keys.DecryptString(encrypt_key));
                dtpSaleDate.MaxDate = maxDate;
            }catch(Exception e)
            {
                dtpSaleDate.MaxDate = DateTime.Now;
            }

        }
        public Panel SalePanel()
        {
            SHOW_ALL();
            GetCashers();
            GetCustomers();
            GetCategories();
            return panelSale;
        }


        private void btnNewCustomer_Click(object sender, EventArgs e)
        {
            new AddCustomer().ShowDialog();
            GetCustomers();
        }

        private void btnNewCategory_Click(object sender, EventArgs e)
        {
            new AddCategory().ShowDialog();
            GetCategories();
        }
        private void GetCashers()
        {
            cmbCasher.DataSource = null;
            cmbCasher.Text = "select casher";
            var dict = new Dictionary<int, string>();
            foreach (var user in new DB.User().ALL())
            {
                dict.Add(user.Id, user.Name);
            }
            cmbCasher.DataSource = new BindingSource(dict, null);
            cmbCasher.DisplayMember = "Value";
            cmbCasher.ValueMember = "Key";
        }
        private void GetCustomers()
        {
            cmbCustomer.DataSource = null;
            cmbCustomer.Text = "select customer";
            var dict = new Dictionary<int, string>();
            foreach (var customer in new DB.Customer().ALL())
            {
                dict.Add(customer.Id, customer.Name);
            }
            cmbCustomer.DataSource = new BindingSource(dict, null);
            cmbCustomer.DisplayMember = "Value";
            cmbCustomer.ValueMember = "Key";
        }
        private void GetCategories()
        {
            cmbCategory.DataSource = null;
            cmbCategory.Text = "select category";
            var dict = new Dictionary<int, string>();
            foreach (var category in new DB.Category().ALL())
            {
                dict.Add(category.Id, category.Name);
            }
            cmbCategory.DataSource = new BindingSource(dict, null);
            cmbCategory.DisplayMember = "Value";
            cmbCategory.ValueMember = "Key";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
        if (Validation())
            {
                STORE();
            }
        }
        private Boolean Validation()
        {
            var isAnyEmpty = ScanForControls<ComboBox>(this)
                            .Where(x => x.SelectedIndex < 0)
                            .Any();

            if (isAnyEmpty)
            {
                MessageBox.Show("please fill all * fields");
                return false;
            }
            else if (Convert.ToDouble(txtPayment.Text) <= 0)
            {
                MessageBox.Show("please fill Payment");
                return false;
            }
            else
            return true;
        }
        public IEnumerable<T> ScanForControls<T>(Control parent) where T : Control
        {
            if (parent is T)
                yield return (T)parent;

            foreach (Control child in parent.Controls)
            {
                foreach (var child2 in ScanForControls<T>(child))
                    yield return (T)child2;
            }
        }
        private void txtAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }
        private void SHOW_ALL()
        {
            double total = 0;
            List<DO.Sale> sales = new DB.Sale().ALL();
            dgvSale.Rows.Clear();
            foreach(var sale in sales)
            {
                int row = dgvSale.Rows.Add();
                dgvSale.Rows[row].Cells["no"].Value = row + 1;
                dgvSale.Rows[row].Cells["id"].Value = sale.Id;
                dgvSale.Rows[row].Cells["sale_date"].Value = sale.SaleDate.ToString("yyyy-MM-dd");
                dgvSale.Rows[row].Cells["customer"].Value =new DB.Customer().SHOW(sale.CustomerId).Name;
                dgvSale.Rows[row].Cells["item"].Value = new DB.Category().SHOW(sale.CategoryId).Name +"-"+sale.Model;
                dgvSale.Rows[row].Cells["imei"].Value = sale.Imei;
                dgvSale.Rows[row].Cells["sale_type"].Value =(sale.Mass==1?"အလုံး ၊ ":"") + (sale.Item==1?"ဆက်ဆက်ပစ္စည်း":"");
                dgvSale.Rows[row].Cells["total"].Value = sale.Total;
                dgvSale.Rows[row].Cells["status"].Value = sale.Cleared != 0 ? "ရှင်းပြီး" : "မရှင်းရသေး";
                dgvSale.Rows[row].Cells["actions"].Value = "More";
                total += sale.Total;
                dgvSale.Text = (row + 1).ToString();
            }
            txtSaleTotal.Text = total.ToString();
        }
        private void STORE()
        {
            DO.Sale sale = new DO.Sale();
            //@user_id,@customer_id,@category_id,@model,@imei,@mass,@item,@sale_date,@total,@payment,@remain,@remain_type,@cleared,@description
            sale.UserId = (int)cmbCasher.SelectedValue;
            sale.CustomerId = (int)cmbCustomer.SelectedValue;
            sale.CategoryId = (int)cmbCategory.SelectedValue;
            sale.Model = txtPhoneModel.Text;
            sale.Mass = cbMass.Checked == true ? 1 : 0;
            sale.Item = cbItem.Checked == true ? 1 : 0;
            sale.Imei = txtIMEI.Text;
            sale.SaleDate = dtpSaleDate.Value;
            sale.Total = Convert.ToDouble(txtTotal.Text);
            sale.Payment = Convert.ToDouble(txtPayment.Text);
            sale.Remain = Convert.ToDouble(txtReceivablePayable.Text);
            sale.RemainType = remainType;
            sale.Cleared = cleared;
            sale.Description = txtDescription.Text;
            if (new DB.Sale().STORE(sale))
            {
                SetDefault();
            }
            else MessageBox.Show("Something wrong! \n Please try againg (or) Contact support team","Storing Erroor!",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }

        private void rdbCleared_CheckedChanged(object sender, EventArgs e)
        {
            cleared = 1;
        }

        private void rdbNotCleared_CheckedChanged(object sender, EventArgs e)
        {
            cleared = 0;
        }

        private void txtPayment_TextChanged(object sender, EventArgs e)
        {
            double total = Convert.ToDouble(txtTotal.Text);
            double payment = Convert.ToDouble(txtPayment.Text);
            if (total < payment)
            {
                remainType = 1;
                lbRecPay.Text = "ပေးရန် ကျန်ငွေ ကို  :";
                txtReceivablePayable.Text = (payment - total).ToString();
            }
            else
            {
                remainType = 0;
                lbRecPay.Text = "ရရန် ကျန်ငွေ ကို  :";
                txtReceivablePayable.Text = (total - payment).ToString();
            }
            
        }
        private void SetDefault()
        {
            SHOW_ALL();
            GetCashers();
            GetCustomers();
            GetCategories();
            txtIMEI.Text = "";
            txtPhoneModel.Text = "";
            txtPayment.Text = "0";
            txtTotal.Text = "0";
            txtReceivablePayable.Text = "0";
            txtDescription.Text = "";

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            SetDefault();
        }
        private void dgvSale_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            var row = dgvSale.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];

            if (cell is DataGridViewButtonCell)
            {
                SaleEdit saleEdit = new SaleEdit(Convert.ToInt16(row.Cells["id"].Value));
                saleEdit.ShowDialog();
                SHOW_ALL();
            }
        }

        private void btnFilterSearch_Click(object sender, EventArgs e)
        {
            int all = rdbFilterAll.Checked ? 1 : 0;
            int cleared =rdbFilterCleared.Checked ? 1 : 0;
            int notCleared =rdbFilterNotCleared.Checked ? 1: 0;
            if( dtpFilterFrom.Value <= dtpFilterTo.Value )
            {
                double total = 0;
                List<DO.Sale> sales = new DB.Sale().SEARCH(dtpFilterFrom.Value,dtpFilterTo.Value,all,cleared,notCleared );
                dgvSale.Rows.Clear();
                foreach (var sale in sales)
                {
                    int row = dgvSale.Rows.Add();
                    dgvSale.Rows[row].Cells["no"].Value = row + 1;
                    dgvSale.Rows[row].Cells["id"].Value = sale.Id;
                    dgvSale.Rows[row].Cells["sale_date"].Value = sale.SaleDate.ToString("yyyy-MM-dd");
                    dgvSale.Rows[row].Cells["customer"].Value = new DB.Customer().SHOW(sale.CustomerId).Name;
                    dgvSale.Rows[row].Cells["item"].Value = new DB.Category().SHOW(sale.CategoryId).Name + "-" + sale.Model;
                    dgvSale.Rows[row].Cells["imei"].Value = sale.Imei;
                    dgvSale.Rows[row].Cells["sale_type"].Value = (sale.Mass == 1 ? "အလုံး  " : "") + (sale.Item == 1 ? "၊ ဆက်ဆက်ပစ္စည်း" : "");
                    dgvSale.Rows[row].Cells["total"].Value = sale.Total;
                    dgvSale.Rows[row].Cells["status"].Value = sale.Cleared != 0 ? "ရှင်းပြီး" : "မရှင်းရသေး";
                    dgvSale.Rows[row].Cells["actions"].Value = "More";
                    total += sale.Total;
                    dgvSale.Text = (row + 1).ToString();
                }
                txtSaleTotal.Text = total.ToString();
            }
            else
            {
                MessageBox.Show("Please select correct Date Interval");
            }
        }

        private void Sale_Load(object sender, EventArgs e)
        {

        }

        private void txtTotal_TextChanged(object sender, EventArgs e)
        {

        }

        private void dgvSale_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }
            var dataGridView = (sender as DataGridView);
            var row = dgvSale.Rows[e.RowIndex];
            var cell = row.Cells[e.ColumnIndex];
            if (cell is DataGridViewButtonCell)
                dataGridView.Cursor = Cursors.Hand;
            else
                dataGridView.Cursor = Cursors.Default;
        }
    }
}
