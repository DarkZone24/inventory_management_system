using System;
using System.Data;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using MySqlConnector;

namespace InventorySystemCsharp
{
    public partial class Admin_home : Form
    {
        private Panel user_list_panel;

        #region Constants
        private const string CONNECTION_STRING = @"datasource=192.168.254.177;port=3308;SslMode=none;username=root;password=Khw67(js90qYjsp7;database=inv_management;";
        #endregion

        #region Constructor and Initialization
        public Admin_home()
        {
            InitializeComponent();
            CreateUserListPanel();
            InitializeNavigation();
        }
        private void CreateUserListPanel()
        {
            user_list_panel = new Panel();
            user_list_panel.Size = new Size(dashboard_panel.Width + 20, dashboard_panel.Height + 25);
            user_list_panel.Location = dashboard_panel.Location; // Same position
            user_list_panel.BackColor = Color.DarkSlateGray;
            user_list_panel.Visible = false; // Initially hidden

            // Add controls for user list here
            CreateUserListControls();

            // Add to the main container (same parent as dashboard_panel)
            this.Controls.Add(user_list_panel);
        }

        private void CreateUserListControls()
        {
            var titleLabel = CreateTitleLabel();
            var usersGrid = CreateUsersGrid();

            user_list_panel.Controls.AddRange(new Control[] { titleLabel, usersGrid });
            LoadUsersData(usersGrid);
        }

        private Label CreateTitleLabel()
        {
            return new Label
            {
                Text = "Users List",
                Font = new Font("Arial", 16, FontStyle.Regular),
                Location = new Point(20, 20),
                Size = new Size(200, 30),
                ForeColor = Color.White
            };
        }

        private DataGridView CreateUsersGrid()
        {
            return new DataGridView
            {
                Name = "usersGrid",
                Location = new Point(20, 60),
                Size = new Size(user_list_panel.Width - 40, user_list_panel.Height - 100),

                // Auto-sizing
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,

                // Formatting
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
        }
        private void LoadUsersData(DataGridView grid)
        {
            try
            {
                using (var connection = new MySqlConnection(CONNECTION_STRING))
                {
                    var query = "SELECT first, last, username, phone, usertype FROM users";
                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        grid.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InitializeNavigation()
        {
            NavigateToPanel(dashboard_btn, dashboard_panel);
        }
        #endregion

        #region Navigation
        private void NavigateToPanel(Button button, Panel panel)
        {
            if (slide_panel != null)
            {
                slide_panel.Height = button.Height;
                slide_panel.Top = button.Top;
            }

            // Hide all panels first
            dashboard_panel.Visible = false;
            add_manager_panel.Visible = false;
            user_list_panel.Visible = false;

            // Show the selected panel
            if (panel != null)
            {
                panel.Visible = true;
                panel.BringToFront();
            }
        }

        private void dashboard_btn_Click(object sender, EventArgs e) =>
            NavigateToPanel(dashboard_btn, dashboard_panel);

        private void add_manager_btn_Click(object sender, EventArgs e)
        {
            NavigateToPanel(add_manager_btn, add_manager_panel);
            CloseUserListIfOpen();
        }

        private void user_list_btn_Click(object sender, EventArgs e)
        {
            NavigateToPanel(user_list_btn, user_list_panel);
            RefreshUsersList();
        }
        private void RefreshUsersList()
        {
            var grid = user_list_panel.Controls["usersGrid"] as DataGridView;
            if (grid != null)
            {
                LoadUsersData(grid);
            }
        }

        private void CloseUserListIfOpen()
        {
            var userList = new User_list();
            userList.Close();
        }

        private void ShowUserList()
        {
            var usersList = new User_list();
            usersList.Show();
            Hide();
        }
        #endregion

        #region Form Actions
        private void logout_btn_Click(object sender, EventArgs e)
        {
            Logout();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Logout()
        {
            CloseUserListIfOpen();
            Close();

            var login = new Login();
            login.Show();
        }
        #endregion

        #region Dashboard Actions
        private void user_home_btn_Click(object sender, EventArgs e) =>
            ShowForm<Home>();

        private void manager_home_btn_Click(object sender, EventArgs e) =>
            ShowForm<Manager_home>();

        private void item_check_btn_Click(object sender, EventArgs e) =>
            ShowForm<Manager_home>();

        private void check_orders_btn_Click(object sender, EventArgs e) =>
            ShowForm<Manager_home>();

        private void ShowForm<T>() where T : Form, new()
        {
            var form = new T();
            form.Show();
        }
        #endregion

        #region Manager Registration
        private void Mregister_btn_Click(object sender, EventArgs e)
        {
            CreateManagerAccount();
        }

        private void CreateManagerAccount()
        {
            if (!ValidateManagerInput())
                return;

            if (!ValidatePasswordMatch())
                return;

            try
            {
                RegisterManager();
                ShowSuccessMessage();
                ClearManagerFields();
            }
            catch (MySqlException)
            {
                ShowUsernameExistsError();
            }
            catch (Exception ex)
            {
                ShowGenericError(ex);
            }
        }

        private bool ValidateManagerInput()
        {
            var requiredFields = new[]
            {
                MfnameTxt, MlnameTxt, MusernameTxt,
                MphonenumTxt, MpassTxt, MrepassTxt
            };

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Text))
                {
                    MessageBox.Show("Please fill all required fields!", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(typecomboTxt.Text))
            {
                MessageBox.Show("Please select a user type!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool ValidatePasswordMatch()
        {
            if (MpassTxt.Text != MrepassTxt.Text)
            {
                MessageBox.Show("Passwords don't match!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void RegisterManager()
        {
            var hashedPassword = HashPassword(MpassTxt.Text.Trim());

            using (var connection = new MySqlConnection(CONNECTION_STRING))
            {
                var query = @"INSERT INTO users (first, last, username, phone, password, usertype) 
                             VALUES (@first, @last, @username, @phone, @password, @usertype)";

                using (var command = new MySqlCommand(query, connection))
                {
                    AddManagerParameters(command, hashedPassword);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddManagerParameters(MySqlCommand command, string hashedPassword)
        {
            command.Parameters.AddWithValue("@first", MfnameTxt.Text.Trim());
            command.Parameters.AddWithValue("@last", MlnameTxt.Text.Trim());
            command.Parameters.AddWithValue("@username", MusernameTxt.Text.Trim());
            command.Parameters.AddWithValue("@phone", MphonenumTxt.Text.Trim());
            command.Parameters.AddWithValue("@password", hashedPassword);
            command.Parameters.AddWithValue("@usertype", typecomboTxt.Text.Trim());
        }

        private void ClearManagerFields()
        {
            var fieldsToClear = new[]
            {
                MfnameTxt, MlnameTxt, MusernameTxt,
                MphonenumTxt, MpassTxt, MrepassTxt
            };

            foreach (var field in fieldsToClear)
            {
                field.Text = "";
            }

            typecomboTxt.SelectedIndex = -1;
        }

        private void ShowSuccessMessage()
        {
            MessageBox.Show("New manager account created successfully!", "Success",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowUsernameExistsError()
        {
            MessageBox.Show("This username is already taken! Please choose a different username.",
                          "Username Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowGenericError(Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        #region Security
        private string HashPassword(string password)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = md5.ComputeHash(inputBytes);

                var hash = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    hash.Append(b.ToString("x2"));
                }

                return hash.ToString();
            }
        }

        // Deprecated: Use HashPassword instead
        [Obsolete("Use HashPassword method instead")]
        public static string MD5Hash(string input)
        {
            var hash = new StringBuilder();
            using (var md5Provider = new MD5CryptoServiceProvider())
            {
                var bytes = md5Provider.ComputeHash(Encoding.UTF8.GetBytes(input));
                foreach (var b in bytes)
                {
                    hash.Append(b.ToString("x2"));
                }
            }
            return hash.ToString();
        }
        #endregion
    }
}