using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace ComputerPartsShop
{
    public partial class EmployeeEditForm : Form
    {
        private string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";
        private int? employeeId = null;

        public EmployeeEditForm(int? id = null)
        {
            InitializeComponent();
            employeeId = id;

            if (employeeId.HasValue)
            {
                lblTitle.Text = "Редактировать сотрудника";
            }
            else
            {
                lblTitle.Text = "Добавить сотрудника";
            }

            if (employeeId.HasValue)
            {
                LoadEmployee(employeeId.Value);
            }
        }

        private void LoadEmployee(int id)
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT FullName, Position, Login FROM Employees WHERE EmployeeID = @ID";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtName.Text = reader["FullName"].ToString();
                                txtPosition.Text = reader["Position"].ToString();
                                txtLogin.Text = reader["Login"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки сотрудника: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите ФИО сотрудника!");
                return;
            }

            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query;

                    if (employeeId.HasValue)
                    {
                        query = "UPDATE Employees SET FullName=@Name, Position=@Position, Login=@Login WHERE EmployeeID=@ID";
                    }
                    else
                    {
                        query = "INSERT INTO Employees (FullName, Position, Login) VALUES (@Name, @Position, @Login)";
                    }

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Position", txtPosition.Text.Trim());
                        cmd.Parameters.AddWithValue("@Login", txtLogin.Text.Trim());

                        if (employeeId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@ID", employeeId.Value);
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
