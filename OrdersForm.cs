using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace ComputerPartsShop
{
    public partial class OrdersForm : Form
    {
        private string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";

        public OrdersForm()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            o.OrderID, 
                            o.Date, 
                            c.FullName AS Customer, 
                            e.FullName AS Employee, 
                            o.TotalAmount, 
                            o.Status 
                        FROM Orders o
                        LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                        LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                        ORDER BY o.Date DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvOrders.DataSource = dt;
                    }
                }
                
                LocalizeColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LocalizeColumns()
        {
             if (dgvOrders.Columns["OrderID"] != null) dgvOrders.Columns["OrderID"].HeaderText = "Номер";
             if (dgvOrders.Columns["orderid"] != null) dgvOrders.Columns["orderid"].HeaderText = "Номер";
             
             if (dgvOrders.Columns["Date"] != null) dgvOrders.Columns["Date"].HeaderText = "Дата";
             if (dgvOrders.Columns["date"] != null) dgvOrders.Columns["date"].HeaderText = "Дата";
             
             if (dgvOrders.Columns["Customer"] != null) dgvOrders.Columns["Customer"].HeaderText = "Клиент";
             if (dgvOrders.Columns["customer"] != null) dgvOrders.Columns["customer"].HeaderText = "Клиент";
             
             if (dgvOrders.Columns["Employee"] != null) dgvOrders.Columns["Employee"].HeaderText = "Сотрудник";
             if (dgvOrders.Columns["employee"] != null) dgvOrders.Columns["employee"].HeaderText = "Сотрудник";
             
             if (dgvOrders.Columns["TotalAmount"] != null) dgvOrders.Columns["TotalAmount"].HeaderText = "Сумма";
             if (dgvOrders.Columns["totalamount"] != null) dgvOrders.Columns["totalamount"].HeaderText = "Сумма";
             
             if (dgvOrders.Columns["Status"] != null) dgvOrders.Columns["Status"].HeaderText = "Статус";
             if (dgvOrders.Columns["status"] != null) dgvOrders.Columns["status"].HeaderText = "Статус";
        }

        private void dgvOrders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                OpenDetails();
            }
        }

        private void OpenDetails()
        {
            if (dgvOrders.SelectedRows.Count > 0)
            {
                var idVal = dgvOrders.SelectedRows[0].Cells["OrderID"].Value ?? dgvOrders.SelectedRows[0].Cells["orderid"].Value;
                if (idVal != null)
                {
                    int orderId = Convert.ToInt32(idVal);
                    OrderDetailsForm details = new OrderDetailsForm(orderId);
                    details.ShowDialog();
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OrderEditForm form = new OrderEditForm(); // New Order (ID null)
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadOrders();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            // Пока открываем только детали, т.к. полноценное редактирование сложнее
            if (dgvOrders.SelectedRows.Count > 0)
            {
                 OpenDetails();
            }
            else
            {
                 MessageBox.Show("Выберите заказ для просмотра");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
             if (dgvOrders.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить заказ? Это вернет товары на склад.", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        var idVal = dgvOrders.SelectedRows[0].Cells["OrderID"].Value ?? dgvOrders.SelectedRows[0].Cells["orderid"].Value;
                        if (idVal != null)
                        {
                            int id = Convert.ToInt32(idVal);
                            DeleteOrder(id);
                            LoadOrders();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка удаления: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для удаления");
            }
        }

        private void DeleteOrder(int orderId)
        {
            // Удаление заказа с возвратом товаров на склад
            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                using (var trans = con.BeginTransaction())
                {
                     try
                     {
                         // 1. Получить детали заказа для возврата на склад
                         string selectDetails = "SELECT ProductID, Quantity FROM OrderDetails WHERE OrderID = @OID";
                         using (NpgsqlCommand cmd = new NpgsqlCommand(selectDetails, con, trans))
                         {
                             cmd.Parameters.AddWithValue("@OID", orderId);
                             using (NpgsqlDataReader reader = cmd.ExecuteReader())
                             {
                                 // Считываем всё в память, чтобы потом обновить (т.к. reader держит соединение)
                                 DataTable dtDetails = new DataTable();
                                 dtDetails.Load(reader);
                                 
                                 foreach(DataRow row in dtDetails.Rows)
                                 {
                                     int pid = Convert.ToInt32(row["ProductID"]);
                                     int qty = Convert.ToInt32(row["Quantity"]);
                                     
                                     // Возврат на склад
                                     string returnStock = "UPDATE ProductStock SET Quantity = Quantity + @Q WHERE ProductID = @PID";
                                     using(NpgsqlCommand upd = new NpgsqlCommand(returnStock, con, trans))
                                     {
                                         upd.Parameters.AddWithValue("@Q", qty);
                                         upd.Parameters.AddWithValue("@PID", pid);
                                         upd.ExecuteNonQuery();
                                     }
                                 }
                             }
                         }

                         // 2. Удалить детали
                         // (Каскадно удалятся, но лучше явно если FK не cascade)
                         using (NpgsqlCommand delDet = new NpgsqlCommand("DELETE FROM OrderDetails WHERE OrderID = @OID", con, trans))
                         {
                             delDet.Parameters.AddWithValue("@OID", orderId);
                             delDet.ExecuteNonQuery();
                         }

                         // 3. Удалить заказ
                         using (NpgsqlCommand delOrd = new NpgsqlCommand("DELETE FROM Orders WHERE OrderID = @OID", con, trans))
                         {
                             delOrd.Parameters.AddWithValue("@OID", orderId);
                             delOrd.ExecuteNonQuery();
                         }

                         trans.Commit();
                     }
                     catch
                     {
                         trans.Rollback();
                         throw;
                     }
                }
            }
        }
    }
}
