using System;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerPartsShop
{
    public partial class MainForm : Form
    {
        private Button currentButton;
        private Form activeForm;

        public MainForm()
        {
            InitializeComponent();
        }

        private void ActivateButton(object btnSender)
        {
            if (btnSender != null)
            {
                if (currentButton != (Button)btnSender)
                {
                    DisableButton();
                    currentButton = (Button)btnSender;
                    currentButton.BackColor = Color.FromArgb(73, 75, 110);
                    currentButton.ForeColor = Color.White;
                    currentButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                    panelTitleBar.BackColor = Color.FromArgb(0, 150, 136); // Or dynamic color
                    lblTitle.Text = currentButton.Text.ToUpper();
                }
            }
        }

        private void DisableButton()
        {
            foreach (Control previousBtn in panelMenu.Controls)
            {
                if (previousBtn.GetType() == typeof(Button))
                {
                    previousBtn.BackColor = Color.FromArgb(51, 51, 76);
                    previousBtn.ForeColor = Color.Gainsboro;
                    previousBtn.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                }
            }
        }

        private void OpenChildForm(Form childForm, object btnSender)
        {
            if (activeForm != null)
                activeForm.Close();

            ActivateButton(btnSender);
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            this.panelDesktop.Controls.Add(childForm);
            this.panelDesktop.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            OpenChildForm(new ProductsForm(), sender);
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            // MessageBox.Show("Форма заказов пока не реализована", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            OpenChildForm(new OrdersForm(), sender);
        }

        private void btnClients_Click(object sender, EventArgs e)
        {
            OpenChildForm(new CustomersForm(), sender);
        }

        private void btnEmployees_Click(object sender, EventArgs e)
        {
            OpenChildForm(new EmployeesForm(), sender);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
