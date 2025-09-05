using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;

namespace InventorySystemCsharp
{
    public partial class Login : Form
    {
        private const string CONNECTION_STRING = @"datasource=192.168.254.177;port=3308;SslMode=none;username=root;password=Khw67(js90qYjsp7;database=inv_management;";

        public Login()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            NavigateToSignUp();
        }

        private async void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            await AuthenticateUserAsync();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Private Methods

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(bunifuMetroTextbox1.Text))
            {
                ShowErrorMessage("Please enter your username");
                bunifuMetroTextbox1.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(bunifuMetroTextbox2.Text))
            {
                ShowErrorMessage("Please enter your password");
                bunifuMetroTextbox2.Focus();
                return false;
            }

            return true;
        }

        private async Task AuthenticateUserAsync()
        {
            try
            {
                var credentials = new LoginCredentials
                {
                    Username = bunifuMetroTextbox1.Text.Trim(),
                    Password = bunifuMetroTextbox2.Text.Trim()
                };

                var user = await ValidateUserCredentialsAsync(credentials);

                if (user != null)
                {
                    SetCurrentUser(user);
                    NavigateToUserDashboard(user.UserType);
                }
                else
                {
                    ShowErrorMessage("Invalid username or password. Please try again.");
                    ClearPasswordField();
                }
            }
            catch (MySqlException ex)
            {
                ShowErrorMessage($"Database connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"An unexpected error occurred: {ex.Message}");
            }
        }

        private async Task<UserModel> ValidateUserCredentialsAsync(LoginCredentials credentials)
        {
            const string query = @"
                SELECT username, usertype, first, last 
                FROM users 
                WHERE username = @username AND password = @password 
                LIMIT 1";

            using (var connection = new MySqlConnection(CONNECTION_STRING))
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", credentials.Username);
                command.Parameters.AddWithValue("@password", HashPassword(credentials.Password));

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new UserModel
                        {
                            Username = reader["username"].ToString(),
                            UserType = reader["usertype"].ToString().ToLower(),
                            FirstName = reader["first"].ToString(),
                            LastName = reader["last"].ToString()
                        };
                    }
                }
            }

            return null;
        }

        private void SetCurrentUser(UserModel user)
        {
            var userDetail = new userdetail();
            userDetail.setUname(user.Username);
        }

        private void NavigateToUserDashboard(string userType)
        {
            Form targetForm;

            switch (userType)
            {
                case "member":
                    targetForm = new Home();
                    break;
                case "manager":
                    targetForm = new Manager_home();
                    break;
                case "admin":
                    targetForm = new Admin_home();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown user type: {userType}");
            }

            this.Hide();
            targetForm.Show();
        }

        private void NavigateToSignUp()
        {
            var signUp = new SignUp();
            this.Hide();
            signUp.Show();
        }

        private void ClearPasswordField()
        {
            bunifuMetroTextbox2.Text = string.Empty;
            bunifuMetroTextbox2.Focus();
        }

        private static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static string HashPassword(string password)
        {
            // Note: Consider upgrading to SHA256 for better security
            // This maintains compatibility with existing MD5 hashes
            using (var md5 = MD5.Create())
            {
                var hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hash = new StringBuilder();

                foreach (var b in hashedBytes)
                {
                    hash.Append(b.ToString("x2"));
                }

                return hash.ToString();
            }
        }

        #endregion
    }

    #region Data Models

    public class LoginCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserModel
    {
        public string Username { get; set; }
        public string UserType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    #endregion
}