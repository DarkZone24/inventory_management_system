using MySqlConnector;
using System;
using System.Data;
using System.Windows.Forms;

namespace InventorySystemCsharp
{
    public partial class Home : Form
    {
        #region Properties and Constants
        private const string CONNECTION_STRING = @"datasource=192.168.254.177;port=3308;SslMode=none;username=root;password=Khw67(js90qYjsp7;database=inv_management;";

        public string ItemList { get; set; } = "";
        public float TotalPrice { get; set; } = 0;
        public string UpdateQuery { get; set; } = "";

        private readonly TextBox[] itemTextBoxes;
        #endregion

        #region Constructor and Initialization
        public Home()
        {
            InitializeComponent();
            // Initialize textbox array for easier management
            itemTextBoxes = new[] { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6 };
        }

        private void Home_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            FillComboBoxes();
            ResetOrderData();
            DisplayUsername();
        }

        private void ResetOrderData()
        {
            ItemList = "";
            TotalPrice = 0;
            UpdateQuery = "";
        }

        private void DisplayUsername()
        {
            var user = new userdetail();
            label9.Text = user.getUname();
        }
        #endregion

        #region Database Operations
        private DataTable ExecuteQuery(string query)
        {
            try
            {
                using (var conn = new MySqlConnection(CONNECTION_STRING))
                {
                    var adapter = new MySqlDataAdapter(query, conn);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}");
                return new DataTable();
            }
        }

        private void FillComboBoxes()
        {
            FillComboBox(comboBox1, "SELECT DISTINCT model FROM spareparts1", "model");
            FillComboBox(comboBox2, "SELECT DISTINCT part FROM spareparts1", "part");
        }

        private void FillComboBox(ComboBox comboBox, string query, string displayMember)
        {
            var dataTable = ExecuteQuery(query);
            if (dataTable.Rows.Count > 0)
            {
                comboBox.DataSource = dataTable;
                comboBox.DisplayMember = displayMember;
                comboBox.ValueMember = displayMember;
            }
        }
        #endregion

        #region Search and Display
        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox1.Text) || string.IsNullOrEmpty(comboBox2.Text))
            {
                MessageBox.Show("Please select both model and part!");
                return;
            }

            SearchItems();
        }

        private void SearchItems()
        {
            var query = $"SELECT * FROM spareparts1 WHERE model='{comboBox1.Text}' AND part='{comboBox2.Text}'";
            var results = ExecuteQuery(query);
            dataGridView1.DataSource = results;

            if (results.Rows.Count == 0)
            {
                MessageBox.Show("No items found matching the criteria.");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            PopulateItemDetails(e);
        }

        private void PopulateItemDetails(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dataGridView1.Rows[e.RowIndex];

            for (int i = 0; i < itemTextBoxes.Length && i < row.Cells.Count; i++)
            {
                itemTextBoxes[i].Text = row.Cells[i].Value?.ToString() ?? "";
            }
        }
        #endregion

        #region Cart Management
        private void button4_Click(object sender, EventArgs e)
        {
            AddToCart();
        }

        private void AddToCart()
        {
            if (!ValidateCartInput()) return;

            var stockQuantity = ParseInt(textBox6.Text);
            var requestedQuantity = ParseInt(textBox7.Text);

            if (stockQuantity < requestedQuantity)
            {
                MessageBox.Show("Not enough items in stock!");
                return;
            }

            ProcessCartItem(stockQuantity, requestedQuantity);
        }

        private bool ValidateCartInput()
        {
            if (AnyTextBoxEmpty(textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7))
            {
                MessageBox.Show("Please fill all item details and quantity!");
                return false;
            }

            if (!IsValidNumber(textBox5.Text) || !IsValidNumber(textBox6.Text) || !IsValidNumber(textBox7.Text))
            {
                MessageBox.Show("Please enter valid numbers for price, stock, and quantity!");
                return false;
            }

            return true;
        }

        private void ProcessCartItem(int stockQuantity, int requestedQuantity)
        {
            var itemPrice = ParseFloat(textBox5.Text);
            var totalItemPrice = itemPrice * requestedQuantity;

            // Build item description
            var itemDescription = $"{textBox2.Text} {textBox3.Text} {textBox4.Text} {textBox5.Text}*{textBox7.Text}";

            // Update cart data
            ItemList += itemDescription + Environment.NewLine;
            TotalPrice += totalItemPrice;
            UpdateQuery += $"UPDATE spareparts1 SET instock='{stockQuantity - requestedQuantity}' WHERE id='{textBox1.Text}';";

            // Show confirmation
            var confirmationMessage = $"{textBox1.Text} {textBox2.Text} {textBox3.Text} {textBox4.Text}*{textBox7.Text}";
            MessageBox.Show($"{confirmationMessage}{Environment.NewLine}Added to Cart");

            ClearItemSelection();
        }

        private void ClearItemSelection()
        {
            foreach (var textBox in itemTextBoxes)
            {
                textBox.Clear();
            }
            textBox7.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ProceedToCheckout();
        }

        private void ProceedToCheckout()
        {
            if (string.IsNullOrEmpty(ItemList))
            {
                MessageBox.Show("No items selected!");
                return;
            }

            ShowConfirmationDialog();
        }

        private void ShowConfirmationDialog()
        {
            var confirmDialog = new Confirm
            {
                MyParent = this
            };

            confirmDialog.Show();
            this.Enabled = false;
        }
        #endregion

        #region Navigation
        private void button1_Click(object sender, EventArgs e)
        {
            NavigateToOrders();
        }

        private void logout_btn_Click(object sender, EventArgs e)
        {
            NavigateToLogin();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NavigateToOrders()
        {
            var ordersForm = new MyOrders();
            Hide();
            ordersForm.Show();
        }

        private void NavigateToLogin()
        {
            var loginForm = new Login();
            Hide();
            loginForm.Show();
        }
        #endregion

        #region Helper Methods
        private bool AnyTextBoxEmpty(params TextBox[] textBoxes)
        {
            foreach (var textBox in textBoxes)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                    return true;
            }
            return false;
        }

        private bool IsValidNumber(string text)
        {
            return float.TryParse(text, out _);
        }

        private int ParseInt(string text)
        {
            return int.TryParse(text, out int result) ? result : 0;
        }

        private float ParseFloat(string text)
        {
            return float.TryParse(text, out float result) ? result : 0f;
        }
        #endregion
    }
}