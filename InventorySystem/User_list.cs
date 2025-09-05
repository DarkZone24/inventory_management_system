using System;
using System.Data;
using System.Windows.Forms;
using MySqlConnector;

namespace InventorySystemCsharp
{
    public partial class User_list : Form
    {
        private const string CONNECTION_STRING = @"datasource=192.168.254.177;port=3308;SslMode=none;username=root;password=Khw67(js90qYjsp7;database=inv_management;";
        private const string USER_QUERY = "SELECT first, last, username, phone, usertype FROM users";

        public User_list()
        {
            InitializeComponent();
        }

        private void User_list_Load(object sender, EventArgs e) => FillUserList();

        private void close_btn_Click(object sender, EventArgs e)
        {
            new Admin_home().Show();
            Close();
        }

        private void FillUserList()
        {
            using (var conn = new MySqlConnection(CONNECTION_STRING))
            using (var adapter = new MySqlDataAdapter(USER_QUERY, conn))
            {
                var dataTable = new DataTable();

                adapter.Fill(dataTable);
                users_list.DataSource = dataTable;
            }
        }
    }
}