using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;

namespace ComputerPartsShop
{
    public partial class ProductsForm : Form
    {
        // Connection string - замените на вашу реальную строку подключения
        // Укажите верный пароль и имя пользователя
        private string connectionString = "Host=192.168.0.152;Port=5432;Username=postgres;Password=123;Database=computer_parts_sales;Command Timeout=3";
        private FlowLayoutPanel flowPanelProducts;
        private bool isTileView = false;
        private Button btnToggleView;

        public ProductsForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadData();
        }

        private void InitializeCustomComponents()
        {
            // Init Flow Panel
            flowPanelProducts = new FlowLayoutPanel();
            flowPanelProducts.Dock = DockStyle.Fill;
            flowPanelProducts.AutoScroll = true;
            flowPanelProducts.BackColor = System.Drawing.Color.WhiteSmoke;
            flowPanelProducts.Visible = false;
            this.Controls.Add(flowPanelProducts);
            flowPanelProducts.BringToFront();

            // Init Toggle Button
            btnToggleView = new Button();
            btnToggleView.Text = "Плитка";
            btnToggleView.Size = new System.Drawing.Size(80, 26);
            btnToggleView.Location = new System.Drawing.Point(370, 18);
            btnToggleView.FlatStyle = FlatStyle.Flat;
            btnToggleView.BackColor = System.Drawing.Color.FromArgb(0, 150, 136);
            btnToggleView.ForeColor = System.Drawing.Color.White;
            btnToggleView.Click += BtnToggleView_Click;
            
            // Add Button
            Button btnAdd = new Button();
            btnAdd.Text = "Добавить";
            btnAdd.Size = new System.Drawing.Size(80, 26);
            btnAdd.Location = new System.Drawing.Point(460, 18);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.BackColor = System.Drawing.Color.FromArgb(0, 150, 136);
            btnAdd.ForeColor = System.Drawing.Color.White;
            btnAdd.Click += BtnAdd_Click;

            // Edit Button
            Button btnEdit = new Button();
            btnEdit.Text = "Изменить";
            btnEdit.Size = new System.Drawing.Size(80, 26);
            btnEdit.Location = new System.Drawing.Point(550, 18);
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.BackColor = System.Drawing.Color.FromArgb(0, 150, 136);
            btnEdit.ForeColor = System.Drawing.Color.White;
            btnEdit.Click += BtnEdit_Click;

            // Delete Button
            Button btnDelete = new Button();
            btnDelete.Text = "Удалить";
            btnDelete.Size = new System.Drawing.Size(80, 26);
            btnDelete.Location = new System.Drawing.Point(640, 18);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.BackColor = System.Drawing.Color.FromArgb(220, 53, 69); // Red color
            btnDelete.ForeColor = System.Drawing.Color.White;
            btnDelete.Click += BtnDelete_Click;

            // Add to panelTop
            this.panelTop.Controls.Add(btnToggleView);
            this.panelTop.Controls.Add(btnAdd);
            this.panelTop.Controls.Add(btnEdit);
            this.panelTop.Controls.Add(btnDelete);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этот товар?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        int id = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
                        using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                        {
                            con.Open();
                            // First check dependencies or rely on cascade? 
                            // Usually safer to check, but for course work simple delete
                            string query = "DELETE FROM Products WHERE ProductID = @ID";
                            using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@ID", id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadData(txtSearch.Text.Trim());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка удаления (возможно, товар используется в заказах): " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления");
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ProductEditForm form = new ProductEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData(txtSearch.Text.Trim());
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
                ProductEditForm form = new ProductEditForm(id);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData(txtSearch.Text.Trim());
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования");
            }
        }

        private void BtnToggleView_Click(object sender, EventArgs e)
        {
            isTileView = !isTileView;
            if (isTileView)
            {
                btnToggleView.Text = "Таблица";
                dgvProducts.Visible = false;
                flowPanelProducts.Visible = true;
                LoadTiles();
            }
            else
            {
                btnToggleView.Text = "Плитка";
                dgvProducts.Visible = true;
                flowPanelProducts.Visible = false;
            }
        }


        private void LoadData(string filter = "")
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    string query = "SELECT p.ProductID, p.Name, c.CategoryName, p.Price, p.StockQuantity " +
                                   "FROM Products p " +
                                   "LEFT JOIN Categories c ON p.CategoryID = c.CategoryID";

                    if (!string.IsNullOrEmpty(filter))
                    {
                        query += " WHERE p.Name ILIKE @Filter OR c.CategoryName ILIKE @Filter";
                    }

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            // В Postgres для регистронезависимого поиска лучше использовать ILIKE
                            cmd.Parameters.AddWithValue("@Filter", "%" + filter + "%");
                        }

                        NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvProducts.DataSource = dt;

                        if (isTileView)
                        {
                            LoadTiles(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки показываем сообщение
                MessageBox.Show("Ошибка подключения к БД: " + ex.Message + "\nПроверьте пароль и доступность сервера.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Тестовые данные (если БД недоступна)
                if (dgvProducts.Rows.Count == 0 && string.IsNullOrEmpty(filter))
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ProductID");
                    dt.Columns.Add("Name");
                    dt.Columns.Add("CategoryName");
                    dt.Columns.Add("Price");
                    dt.Columns.Add("StockQuantity");
                    
                    dt.Rows.Add(1, "Intel Core i5-12400F", "Processors", 12000, 10);
                    dt.Rows.Add(2, "NVIDIA RTX 3060", "Graphics Cards", 35000, 5);
                    
                    dgvProducts.DataSource = dt;
                    if (isTileView) LoadTiles(dt);
                }
            }

            // Локализация заголовков
            if (dgvProducts.Columns["ProductID"] != null) dgvProducts.Columns["ProductID"].HeaderText = "ID";
            if (dgvProducts.Columns["productid"] != null) dgvProducts.Columns["productid"].HeaderText = "ID";
            
            if (dgvProducts.Columns["Name"] != null) dgvProducts.Columns["Name"].HeaderText = "Название";
            if (dgvProducts.Columns["name"] != null) dgvProducts.Columns["name"].HeaderText = "Название";

            if (dgvProducts.Columns["CategoryName"] != null) dgvProducts.Columns["CategoryName"].HeaderText = "Категория";
            if (dgvProducts.Columns["categoryname"] != null) dgvProducts.Columns["categoryname"].HeaderText = "Категория";

            if (dgvProducts.Columns["Price"] != null) dgvProducts.Columns["Price"].HeaderText = "Цена";
            if (dgvProducts.Columns["price"] != null) dgvProducts.Columns["price"].HeaderText = "Цена";

            if (dgvProducts.Columns["StockQuantity"] != null) dgvProducts.Columns["StockQuantity"].HeaderText = "Остаток";
            if (dgvProducts.Columns["stockquantity"] != null) dgvProducts.Columns["stockquantity"].HeaderText = "Остаток";
        }

        private void LoadTiles(DataTable dt = null)
        {
            if (dt == null)
            {
                dt = dgvProducts.DataSource as DataTable;
            }

            flowPanelProducts.Controls.Clear();
            if (dt == null) return;

            foreach (DataRow row in dt.Rows)
            {
                Panel card = new Panel();
                card.Size = new System.Drawing.Size(200, 150);
                card.BackColor = System.Drawing.Color.White;
                card.Margin = new Padding(10);
                
                Label lblName = new Label();
                lblName.Text = row["Name"].ToString();
                lblName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
                lblName.Dock = DockStyle.Top;
                lblName.Height = 40;
                
                Label lblPrice = new Label();
                lblName.Text = row["Name"].ToString();
                lblPrice.Text = $"{row["Price"]} руб.";
                lblPrice.ForeColor = System.Drawing.Color.Green;
                lblPrice.Dock = DockStyle.Bottom;
                
                Label lblCategory = new Label();
                lblCategory.Text = row["CategoryName"].ToString();
                lblCategory.Dock = DockStyle.Top;
                lblCategory.ForeColor = System.Drawing.Color.Gray;

                card.Controls.Add(lblCategory);
                card.Controls.Add(lblName);
                card.Controls.Add(lblPrice);
                
                flowPanelProducts.Controls.Add(card);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadData(txtSearch.Text.Trim());
            // Если мы в режиме плиток, нужно обновить их
            if (isTileView)
            {
                LoadTiles(dgvProducts.DataSource as DataTable);
            }
        }
    }
}
