using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace ComputerPartsShop
{
    public partial class CustomersForm : Form
    {
        private string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";

        public CustomersForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT CustomerID, FullName, Phone, Email, Address, RegistrationDate FROM Customers ORDER BY FullName";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvCustomers.DataSource = dt;
                    }
                }
                
                // Localization
                if (dgvCustomers.Columns["CustomerID"] != null) dgvCustomers.Columns["CustomerID"].HeaderText = "ID";
                if (dgvCustomers.Columns["customerid"] != null) dgvCustomers.Columns["customerid"].HeaderText = "ID";
                
                if (dgvCustomers.Columns["FullName"] != null) dgvCustomers.Columns["FullName"].HeaderText = "ФИО";
                if (dgvCustomers.Columns["fullname"] != null) dgvCustomers.Columns["fullname"].HeaderText = "ФИО";

                if (dgvCustomers.Columns["Phone"] != null) dgvCustomers.Columns["Phone"].HeaderText = "Телефон";
                if (dgvCustomers.Columns["phone"] != null) dgvCustomers.Columns["phone"].HeaderText = "Телефон";

                if (dgvCustomers.Columns["Email"] != null) dgvCustomers.Columns["Email"].HeaderText = "Email";
                if (dgvCustomers.Columns["email"] != null) dgvCustomers.Columns["email"].HeaderText = "Email";

                if (dgvCustomers.Columns["Address"] != null) dgvCustomers.Columns["Address"].HeaderText = "Адрес";
                if (dgvCustomers.Columns["address"] != null) dgvCustomers.Columns["address"].HeaderText = "Адрес";

                if (dgvCustomers.Columns["RegistrationDate"] != null) dgvCustomers.Columns["RegistrationDate"].HeaderText = "Дата рег.";
                if (dgvCustomers.Columns["registrationdate"] != null) dgvCustomers.Columns["registrationdate"].HeaderText = "Дата рег.";
            }
            catch (Exception ex)
            {
               MessageBox.Show("Ошибка загрузки клиентов: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            CustomerEditForm form = new CustomerEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                // Поиск правильного столбца ID (учитывая разный регистр в разных БД)
                var cell = dgvCustomers.SelectedRows[0].Cells["CustomerID"] ?? dgvCustomers.SelectedRows[0].Cells["customerid"];
                if (cell != null)
                {
                     int id = Convert.ToInt32(cell.Value);
                     CustomerEditForm form = new CustomerEditForm(id);
                     if (form.ShowDialog() == DialogResult.OK)
                     {
                         LoadData();
                     }
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этого клиента?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        var cell = dgvCustomers.SelectedRows[0].Cells["CustomerID"] ?? dgvCustomers.SelectedRows[0].Cells["customerid"];
                        if (cell != null)
                        {
                            int id = Convert.ToInt32(cell.Value);
                            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                            {
                                con.Open();
                                string query = "DELETE FROM Customers WHERE CustomerID = @ID";
                                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                                {
                                    cmd.Parameters.AddWithValue("@ID", id);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            LoadData();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка удаления (возможно, есть связанные заказы): " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления");
            }
        }
    }
}
