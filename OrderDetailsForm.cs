using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace ComputerPartsShop
{
    public partial class OrderDetailsForm : Form
    {
        private string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";
        private int _orderId;

        public OrderDetailsForm(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            lblTitle.Text = $"Заказ #{_orderId}";
            LoadDetails();
        }

        private void LoadDetails()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            p.Name, 
                            od.Quantity, 
                            od.PriceAtSale, 
                            (od.Quantity * od.PriceAtSale) AS Total
                        FROM OrderDetails od
                        JOIN Products p ON od.ProductID = p.ProductID
                        WHERE od.OrderID = @OrderID";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", _orderId);
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvDetails.DataSource = dt;
                    }
                }

                // Localization
                if (dgvDetails.Columns["Name"] != null) dgvDetails.Columns["Name"].HeaderText = "Товар";
                if (dgvDetails.Columns["name"] != null) dgvDetails.Columns["name"].HeaderText = "Товар";

                if (dgvDetails.Columns["Quantity"] != null) dgvDetails.Columns["Quantity"].HeaderText = "Кол-во";
                if (dgvDetails.Columns["quantity"] != null) dgvDetails.Columns["quantity"].HeaderText = "Кол-во";

                if (dgvDetails.Columns["PriceAtSale"] != null) dgvDetails.Columns["PriceAtSale"].HeaderText = "Цена";
                if (dgvDetails.Columns["priceatsale"] != null) dgvDetails.Columns["priceatsale"].HeaderText = "Цена";

                if (dgvDetails.Columns["Total"] != null) dgvDetails.Columns["Total"].HeaderText = "Сумма";
                if (dgvDetails.Columns["total"] != null) dgvDetails.Columns["total"].HeaderText = "Сумма";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки состава заказа: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
