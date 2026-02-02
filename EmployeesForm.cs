using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace ComputerPartsShop
{
    public partial class EmployeesForm : Form
    {
        private string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";

        public EmployeesForm()
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
                    string query = "SELECT EmployeeID, FullName, Position, Login FROM Employees ORDER BY FullName";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvEmployees.DataSource = dt;
                    }
                }
                
                // Localization
                if (dgvEmployees.Columns["EmployeeID"] != null) dgvEmployees.Columns["EmployeeID"].HeaderText = "ID";
                if (dgvEmployees.Columns["employeeid"] != null) dgvEmployees.Columns["employeeid"].HeaderText = "ID";
                
                if (dgvEmployees.Columns["FullName"] != null) dgvEmployees.Columns["FullName"].HeaderText = "ФИО";
                if (dgvEmployees.Columns["fullname"] != null) dgvEmployees.Columns["fullname"].HeaderText = "ФИО";

                if (dgvEmployees.Columns["Position"] != null) dgvEmployees.Columns["Position"].HeaderText = "Должность";
                if (dgvEmployees.Columns["position"] != null) dgvEmployees.Columns["position"].HeaderText = "Должность";

                if (dgvEmployees.Columns["Login"] != null) dgvEmployees.Columns["Login"].HeaderText = "Логин";
                if (dgvEmployees.Columns["login"] != null) dgvEmployees.Columns["login"].HeaderText = "Логин";
            }
            catch (Exception ex)
            {
               MessageBox.Show("Ошибка загрузки сотрудников: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            EmployeeEditForm form = new EmployeeEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count > 0)
            {
                var cell = dgvEmployees.SelectedRows[0].Cells["EmployeeID"] ?? dgvEmployees.SelectedRows[0].Cells["employeeid"];
                if (cell != null)
                {
                     int id = Convert.ToInt32(cell.Value);
                     EmployeeEditForm form = new EmployeeEditForm(id);
                     if (form.ShowDialog() == DialogResult.OK)
                     {
                         LoadData();
                     }
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этого сотрудника?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        var cell = dgvEmployees.SelectedRows[0].Cells["EmployeeID"] ?? dgvEmployees.SelectedRows[0].Cells["employeeid"];
                        if (cell != null)
                        {
                            int id = Convert.ToInt32(cell.Value);
                            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                            {
                                con.Open();
                                string query = "DELETE FROM Employees WHERE EmployeeID = @ID";
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
                        MessageBox.Show("Ошибка удаления (возможно, сотрудник " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для удаления");
            }
        }
    }
}
