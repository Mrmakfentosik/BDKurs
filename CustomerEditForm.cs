using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace ComputerPartsShop
{
    public partial class CustomerEditForm : Form
    {
        private string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";
        private int? customerId = null;

        public CustomerEditForm(int? id = null)
        {
            InitializeComponent();
            customerId = id;

            if (customerId.HasValue)
            {
                lblTitle.Text = "Редактировать клиента";
            }
            else
            {
                lblTitle.Text = "Добавить клиента";
            }

            if (customerId.HasValue)
            {
                LoadCustomer(customerId.Value);
            }
        }

        private void LoadCustomer(int id)
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT FullName, Phone, Email, Address FROM Customers WHERE CustomerID = @ID";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtName.Text = reader["FullName"].ToString();
                                txtPhone.Text = reader["Phone"].ToString();
                                txtEmail.Text = reader["Email"].ToString();
                                txtAddress.Text = reader["Address"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки клиента: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите имя клиента!");
                return;
            }

            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query;

                    if (customerId.HasValue)
                    {
                        query = "UPDATE Customers SET FullName=@Name, Phone=@Phone, Email=@Email, Address=@Address WHERE CustomerID=@ID";
                    }
                    else
                    {
                        query = "INSERT INTO Customers (FullName, Phone, Email, Address) VALUES (@Name, @Phone, @Email, @Address)";
                    }

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());

                        if (customerId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@ID", customerId.Value);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
                
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
