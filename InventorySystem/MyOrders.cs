using MySqlConnector;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorySystemCsharp
{
    public partial class MyOrders : Form
    {
        private const string CONNECTION_STRING = @"datasource=192.168.254.177;port=3308;SslMode=none;username=root;password=Khw67(js90qYjsp7;database=inv_management;";
        private string currentUsername;

        public MyOrders()
        {
            InitializeComponent();
        }

        private async void MyOrders_Load(object sender, EventArgs e)
        {
            InitializeUserDetails();
            await LoadMyOrdersAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NavigateToHome();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NavigateToLogin();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await DeleteSelectedOrderAsync();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            this.Close();
            NavigateToHome();
        }

        #region Private Methods

        private void InitializeUserDetails()
        {
            try
            {
                var user = new userdetail();
                currentUsername = user.getUname();
                label9.Text = currentUsername;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading user details: {ex.Message}");
            }
        }

        private async Task LoadMyOrdersAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(currentUsername))
                {
                    ShowErrorMessage("User not identified");
                    return;
                }

                var orders = await GetUserOrdersAsync(currentUsername);
                dataGridView1.DataSource = orders;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading orders: {ex.Message}");
            }
        }

        private async Task<DataTable> GetUserOrdersAsync(string username)
        {
            const string query = "SELECT id, details, price, paid FROM orders WHERE user = @username";

            using (var connection = new MySqlConnection(CONNECTION_STRING))
            using (var adapter = new MySqlDataAdapter(query, connection))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@username", username);

                var dataTable = new DataTable();
                await Task.Run(() => adapter.Fill(dataTable));
                return dataTable;
            }
        }

        private async Task DeleteSelectedOrderAsync()
        {
            try
            {
                if (!IsOrderSelected())
                {
                    ShowInfoMessage("Please select an order to delete");
                    return;
                }

                var selectedOrder = GetSelectedOrder();
                if (selectedOrder == null) return;

                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to delete this order?\n\nDetails: {selectedOrder.Details}\nPrice: {selectedOrder.Price:C}",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult == DialogResult.Yes)
                {
                    await ProcessOrderDeletionAsync(selectedOrder);
                    await LoadMyOrdersAsync();
                    ShowInfoMessage("Order deleted successfully");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting order: {ex.Message}");
            }
        }

        private bool IsOrderSelected()
        {
            return dataGridView1.CurrentRow != null &&
                   dataGridView1.CurrentRow.Cells[0].Value != null;
        }

        private OrderModel GetSelectedOrder()
        {
            try
            {
                var row = dataGridView1.CurrentRow;
                return new OrderModel
                {
                    Id = Convert.ToInt32(row.Cells[0].Value),
                    Details = row.Cells[1].Value?.ToString() ?? "",
                    Price = Convert.ToDecimal(row.Cells[2].Value ?? 0),
                    IsPaid = Convert.ToBoolean(row.Cells[3].Value ?? false)
                };
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error reading selected order: {ex.Message}");
                return null;
            }
        }

        private async Task ProcessOrderDeletionAsync(OrderModel order)
        {
            using (var connection = new MySqlConnection(CONNECTION_STRING))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Restore stock if order was confirmed but not paid
                        if (order.Price > 0 && !order.IsPaid)
                        {
                            await RestoreStockAsync(connection, transaction, order);
                        }

                        // Delete the order
                        await DeleteOrderAsync(connection, transaction, order.Id);

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        private async Task RestoreStockAsync(MySqlConnection connection, MySqlTransaction transaction, OrderModel order)
        {
            const string updateStockQuery = "UPDATE spareparts1 SET instock = instock + @quantity WHERE id = @partId";

            using (var command = new MySqlCommand(updateStockQuery, connection, transaction))
            {
                // Assuming Details contains the quantity and we need to parse the part ID
                if (int.TryParse(order.Details, out int quantity))
                {
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@partId", order.Id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task DeleteOrderAsync(MySqlConnection connection, MySqlTransaction transaction, int orderId)
        {
            const string deleteQuery = "DELETE FROM orders WHERE id = @orderId";

            using (var command = new MySqlCommand(deleteQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@orderId", orderId);
                await command.ExecuteNonQueryAsync();
            }
        }

        private void NavigateToHome()
        {
            var home = new Home();
            this.Hide();
            home.Show();
        }

        private void NavigateToLogin()
        {
            var login = new Login();
            this.Hide();
            login.Show();
        }

        private static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }

    #region Data Models

    public class OrderModel
    {
        public int Id { get; set; }
        public string Details { get; set; }
        public decimal Price { get; set; }
        public bool IsPaid { get; set; }
    }

    #endregion
}