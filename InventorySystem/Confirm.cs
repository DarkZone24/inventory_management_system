using MySqlConnector;
using System;
using System.Windows.Forms;

namespace InventorySystemCsharp
{
    public partial class Confirm : Form
    {
        #region Properties and Constants
        private const string CONNECTION_STRING = @"datasource=192.168.254.177;port=3308;SslMode=none;username=root;password=Khw67(js90qYjsp7;database=inv_management;";

        public Home MyParent { get; set; }
        #endregion

        #region Constructor and Form Events
        public Confirm()
        {
            InitializeComponent();
        }

        private void Confirm_Load(object sender, EventArgs e)
        {
            DisplayOrderSummary();
        }

        private void Confirm_FormClosed(object sender, FormClosedEventArgs e)
        {
            EnableParentForm();
        }
        #endregion

        #region UI Methods
        private void DisplayOrderSummary()
        {
            if (MyParent?.ItemList != null)
            {
                richTextBox1.Text = MyParent.ItemList;
            }
        }

        private void EnableParentForm()
        {
            if (MyParent != null)
            {
                MyParent.Enabled = true;
            }
        }
        #endregion

        #region Button Events
        private void button1_Click(object sender, EventArgs e)
        {
            CancelOrder();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ConfirmOrder();
        }
        #endregion

        #region Order Management
        private void CancelOrder()
        {
            if (MyParent != null)
            {
                ResetParentOrderData();
            }
            Close();
        }

        private void ConfirmOrder()
        {
            if (!ValidateOrder())
                return;

            try
            {
                ProcessOrder();
                ShowSuccessMessage();
                CancelOrder(); // Close and reset after successful order
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        private bool ValidateOrder()
        {
            if (MyParent == null)
            {
                MessageBox.Show("Error: Parent form not found!");
                return false;
            }

            if (string.IsNullOrEmpty(MyParent.ItemList))
            {
                MessageBox.Show("No items to order!");
                return false;
            }

            if (MyParent.TotalPrice <= 0)
            {
                MessageBox.Show("Invalid order total!");
                return false;
            }

            return true;
        }

        private void ProcessOrder()
        {
            var user = new userdetail();
            var username = user.getUname();

            using (var connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert the order
                        InsertOrder(connection, transaction, username);

                        // Update inventory
                        UpdateInventory(connection, transaction);

                        // Commit transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void InsertOrder(MySqlConnection connection, MySqlTransaction transaction, string username)
        {
            var insertQuery = "INSERT INTO orders (user, details, price, paid) VALUES (@user, @details, @price, @paid)";

            using (var command = new MySqlCommand(insertQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@user", username);
                command.Parameters.AddWithValue("@details", MyParent.ItemList);
                command.Parameters.AddWithValue("@price", MyParent.TotalPrice);
                command.Parameters.AddWithValue("@paid", "no"); // Default to unpaid

                command.ExecuteNonQuery();
            }
        }

        private void UpdateInventory(MySqlConnection connection, MySqlTransaction transaction)
        {
            if (string.IsNullOrEmpty(MyParent.UpdateQuery))
                return;

            using (var command = new MySqlCommand(MyParent.UpdateQuery, connection, transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        private void ResetParentOrderData()
        {
            MyParent.ItemList = "";
            MyParent.TotalPrice = 0;
            MyParent.UpdateQuery = "";
        }

        private void ShowSuccessMessage()
        {
            MessageBox.Show("Order placed successfully!", "Success",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowErrorMessage(Exception ex)
        {
            var errorMessage = $"Failed to place order: {ex.Message}";
            MessageBox.Show(errorMessage, "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion
    }
}