using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace ComputerPartsShop
{
    public partial class ProductEditForm : Form
    {
        private string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";
        private int? productId = null;

        public ProductEditForm(int? id = null)
        {
            InitializeComponent();
            productId = id;

            if (productId.HasValue)
            {
                lblTitle.Text = "Редактировать товар";
            }
            else
            {
                lblTitle.Text = "Добавить товар";
            }

            LoadDictionaries();
            
            if (productId.HasValue)
            {
                LoadProduct(productId.Value);
            }
        }

        private void LoadDictionaries()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    
                    // Categories
                    NpgsqlDataAdapter daCat = new NpgsqlDataAdapter("SELECT CategoryID, CategoryName FROM Categories", con);
                    DataTable dtCat = new DataTable();
                    daCat.Fill(dtCat);
                    cmbCategory.DataSource = dtCat;
                    cmbCategory.DisplayMember = "CategoryName";
                    cmbCategory.ValueMember = "CategoryID";

                    // Suppliers
                    NpgsqlDataAdapter daSup = new NpgsqlDataAdapter("SELECT SupplierID, CompanyName FROM Suppliers", con);
                    DataTable dtSup = new DataTable();
                    daSup.Fill(dtSup);
                    cmbSupplier.DataSource = dtSup;
                    cmbSupplier.DisplayMember = "CompanyName";
                    cmbSupplier.ValueMember = "SupplierID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message);
            }
        }

        private void LoadProduct(int id)
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT Name, CategoryID, SupplierID, Price, StockQuantity FROM Products WHERE ProductID = @ID";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtName.Text = reader["Name"].ToString();
                                cmbCategory.SelectedValue = reader["CategoryID"];
                                cmbSupplier.SelectedValue = reader["SupplierID"];
                                numPrice.Value = Convert.ToDecimal(reader["Price"]);
                                numStock.Value = Convert.ToInt32(reader["StockQuantity"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товара: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название товара!");
                return;
            }

            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query;

                    if (productId.HasValue)
                    {
                        query = "UPDATE Products SET Name=@Name, CategoryID=@Cat, SupplierID=@Sup, Price=@Price, StockQuantity=@Stock WHERE ProductID=@ID";
                    }
                    else
                    {
                        query = "INSERT INTO Products (Name, CategoryID, SupplierID, Price, StockQuantity) VALUES (@Name, @Cat, @Sup, @Price, @Stock)";
                    }

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Cat", cmbCategory.SelectedValue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Sup", cmbSupplier.SelectedValue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Price", numPrice.Value);
                        cmd.Parameters.AddWithValue("@Stock", Convert.ToInt32(numStock.Value)); // IMPORTANT: Cast to int

                        if (productId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@ID", productId.Value);
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
