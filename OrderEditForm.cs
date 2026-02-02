using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace ComputerPartsShop
{
    public partial class OrderEditForm : Form
    {
        private string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";
        private int? orderId = null;
        private DataTable dtProducts; // Кеш товаров для ComboBox

        public OrderEditForm(int? id = null)
        {
            InitializeComponent();
            orderId = id;
            lblTitle.Text = orderId.HasValue ? "Редактирование заказа" : "Новый заказ";
            
            LoadDictionaries();
            if (orderId.HasValue)
            {
               LoadOrder(orderId.Value);
            }
        }

        private void LoadDictionaries()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Customers
                    NpgsqlDataAdapter daCust = new NpgsqlDataAdapter("SELECT CustomerID, FullName FROM Customers ORDER BY FullName", con);
                    DataTable dtCust = new DataTable();
                    daCust.Fill(dtCust);
                    cmbCustomer.DisplayMember = "FullName";
                    cmbCustomer.ValueMember = "CustomerID";
                    cmbCustomer.DataSource = dtCust;

                    // Employees
                    NpgsqlDataAdapter daEmp = new NpgsqlDataAdapter("SELECT EmployeeID, FullName FROM Employees ORDER BY FullName", con);
                    DataTable dtEmp = new DataTable();
                    daEmp.Fill(dtEmp);
                    cmbEmployee.DisplayMember = "FullName";
                    cmbEmployee.ValueMember = "EmployeeID";
                    cmbEmployee.DataSource = dtEmp;

                    // Products (для грида)
                    NpgsqlDataAdapter daProd = new NpgsqlDataAdapter(
                        @"SELECT p.ProductID, p.Name, p.Price, p.StockQuantity as Quantity 
                          FROM Products p 
                          ORDER BY p.Name", con);
                    dtProducts = new DataTable();
                    daProd.Fill(dtProducts);
                    
                    ((DataGridViewComboBoxColumn)dgvItems.Columns["colProductId"]).DataSource = dtProducts;
                    ((DataGridViewComboBoxColumn)dgvItems.Columns["colProductId"]).DisplayMember = "Name";
                    ((DataGridViewComboBoxColumn)dgvItems.Columns["colProductId"]).ValueMember = "ProductID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message);
            }
        }

        private void LoadOrder(int id)
        {
            // TODO: Реализовать загрузку существующего заказа (для редактирования)
            // Это сложнее, т.к. нужно вернуть stock при отмене/изменении.
            // Пока сосредоточимся на создании.
            MessageBox.Show("Редактирование заказов пока не реализовано полностью (только создание).");
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            dgvItems.Rows.Add();
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvItems.SelectedRows)
                {
                    if (!row.IsNewRow) dgvItems.Rows.Remove(row);
                }
                CalculateTotal();
            }
        }

        private void dgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Если изменился товар, подставить цену и остаток
            if (e.ColumnIndex == dgvItems.Columns["colProductId"].Index)
            {
                var val = dgvItems.Rows[e.RowIndex].Cells["colProductId"].Value;
                if (val != null)
                {
                    int pid = Convert.ToInt32(val);
                    DataRow[] rows = dtProducts.Select("ProductID=" + pid);
                    if (rows.Length > 0)
                    {
                        dgvItems.Rows[e.RowIndex].Cells["colPrice"].Value = rows[0]["Price"];
                        dgvItems.Rows[e.RowIndex].Cells["colStock"].Value = rows[0]["Quantity"]; // Update Stock
                    }
                }
            }

            // Пересчет строки
            if (e.ColumnIndex == dgvItems.Columns["colProductId"].Index || e.ColumnIndex == dgvItems.Columns["colQuantity"].Index)
            {
                UpdateRowTotal(e.RowIndex);
                CalculateTotal();
            }
        }

        private void UpdateRowTotal(int rowIndex)
        {
            var cellQty = dgvItems.Rows[rowIndex].Cells["colQuantity"].Value;
            var cellPrice = dgvItems.Rows[rowIndex].Cells["colPrice"].Value;

            if (cellQty != null && cellPrice != null)
            {
                if (int.TryParse(cellQty.ToString(), out int q) && decimal.TryParse(cellPrice.ToString(), out decimal p))
                {
                     dgvItems.Rows[rowIndex].Cells["colTotal"].Value = (q * p);
                }
            }
        }

        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                var val = row.Cells["colTotal"].Value;
                if (val != null && decimal.TryParse(val.ToString(), out decimal t))
                {
                    total += t;
                }
            }
            lblTotal.Text = "Итого: " + total.ToString("C");
        }

        private void dgvItems_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Игнорируем ошибки UI
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue == null || cmbEmployee.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента и сотрудника!");
                return;
            }

            if (dgvItems.Rows.Count == 0 || (dgvItems.Rows.Count == 1 && dgvItems.Rows[0].IsNewRow))
            {
                MessageBox.Show("Добавьте товары в заказ!");
                return;
            }

            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    using (var trans = con.BeginTransaction())
                    {
                        try 
                        {
                            // 1. Создаем заказ
                            decimal totalAmount = 0;
                             // Сначала посчитаем итог точно
                            foreach (DataGridViewRow row in dgvItems.Rows)
                            {
                                if (row.IsNewRow) continue;
                                var v = row.Cells["colTotal"].Value;
                                if (v != null) totalAmount += Convert.ToDecimal(v);
                            }

                            int newOrderId = 0;
                            string insertOrder = @"
                                INSERT INTO Orders (Date, CustomerID, EmployeeID, TotalAmount, Status)
                                VALUES (@Date, @Cust, @Emp, @Total, 'Новый')
                                RETURNING OrderID;
                            ";
                            using (NpgsqlCommand cmd = new NpgsqlCommand(insertOrder, con, trans))
                            {
                                cmd.Parameters.AddWithValue("@Date", dtpDate.Value);
                                cmd.Parameters.AddWithValue("@Cust", Convert.ToInt32(cmbCustomer.SelectedValue));
                                cmd.Parameters.AddWithValue("@Emp", Convert.ToInt32(cmbEmployee.SelectedValue));
                                cmd.Parameters.AddWithValue("@Total", totalAmount);
                                newOrderId = (int)cmd.ExecuteScalar();
                            }

                            // 2. Добавляем детали и ОБНОВЛЯЕМ ОСТАТКИ (ProductStock)
                            foreach (DataGridViewRow row in dgvItems.Rows)
                            {
                                if (row.IsNewRow) continue;
                                if (row.Cells["colProductId"].Value == null) continue;

                                int pid = Convert.ToInt32(row.Cells["colProductId"].Value);
                                int qty = Convert.ToInt32(row.Cells["colQuantity"].Value);
                                decimal price = Convert.ToDecimal(row.Cells["colPrice"].Value);
                                
                                // Проверка остатка
                                string checkStock = "SELECT StockQuantity FROM Products WHERE ProductID = @PID";
                                int currentStock = 0;
                                using (NpgsqlCommand cs = new NpgsqlCommand(checkStock, con, trans))
                                {
                                    cs.Parameters.AddWithValue("@PID", pid);
                                    object res = cs.ExecuteScalar();
                                    if (res != null) currentStock = Convert.ToInt32(res);
                                }

                                if (currentStock < qty)
                                {
                                    throw new Exception($"Недостаточно товара на складе. Доступно: {currentStock}, Требуется: {qty}");
                                }

                                // Списание
                                string updateStock = "UPDATE Products SET StockQuantity = StockQuantity - @Q WHERE ProductID = @PID";
                                using (NpgsqlCommand us = new NpgsqlCommand(updateStock, con, trans))
                                {
                                    us.Parameters.AddWithValue("@Q", qty);
                                    us.Parameters.AddWithValue("@PID", pid);
                                    us.ExecuteNonQuery();
                                }

                                // Вставка в детали
                                string insertDetail = "INSERT INTO OrderDetails (OrderID, ProductID, Quantity, PriceAtSale) VALUES (@OID, @PID, @Q, @P)";
                                using (NpgsqlCommand idc = new NpgsqlCommand(insertDetail, con, trans))
                                {
                                    idc.Parameters.AddWithValue("@OID", newOrderId);
                                    idc.Parameters.AddWithValue("@PID", pid);
                                    idc.Parameters.AddWithValue("@Q", qty);
                                    idc.Parameters.AddWithValue("@P", price);
                                    idc.ExecuteNonQuery();
                                }
                            }

                            trans.Commit();
                            MessageBox.Show("Заказ успешно создан!");
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex; 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения заказа: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
