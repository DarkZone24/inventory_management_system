using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;

namespace InventorySystemCsharp
{
    public partial class SignUp : Form
    {
        private const string CONNECTION_STRING = @"datasource=192.168.254.177;port=3308;SslMode=none;username=root;password=Khw67(js90qYjsp7;database=inv_management;";

        public SignUp()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            NavigateToLogin();
        }

        private async void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            if (!ValidateInputFields())
                return;

            if (!ValidatePasswordMatch())
            {
                ShowMessage("Passwords don't match");
                return;
            }

            await RegisterUserAsync();
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Private Methods

        private bool ValidateInputFields()
        {
            var textBoxes = new[]
            {
                bunifuMetroTextbox1, bunifuMetroTextbox2, bunifuMetroTextbox3,
                bunifuMetroTextbox4, bunifuMetroTextbox5, bunifuMetroTextbox6
            };

            foreach (var textBox in textBoxes)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    ShowMessage("Please fill all fields");
                    return false;
                }
            }

            return true;
        }

        private bool ValidatePasswordMatch()
        {
            return bunifuMetroTextbox5.Text.Trim() == bunifuMetroTextbox6.Text.Trim();
        }

        private async Task RegisterUserAsync()
        {
            try
            {
                var user = CreateUserFromForm();
                await InsertUserToDatabase(user);

                ShowMessage("Registration successful!");
                NavigateToLogin();
            }
            catch (MySqlException ex)
            {
                ShowMessage($"Database Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowMessage($"An error occurred: {ex.Message}");
            }
        }

        private UserRegistrationModel CreateUserFromForm()
        {
            return new UserRegistrationModel
            {
                FirstName = bunifuMetroTextbox1.Text.Trim(),
                LastName = bunifuMetroTextbox2.Text.Trim(),
                Username = bunifuMetroTextbox3.Text.Trim(),
                Phone = bunifuMetroTextbox4.Text.Trim(),
                Password = HashPassword(bunifuMetroTextbox5.Text.Trim()),
                UserType = bunifuMetroTextbox7.Text.Trim()
            };
        }

        private async Task InsertUserToDatabase(UserRegistrationModel user)
        {
            const string query = @"
                INSERT INTO users (first, last, username, phone, password, usertype) 
                VALUES (@first, @last, @username, @phone, @password, @usertype)";

            using (var connection = new MySqlConnection(CONNECTION_STRING))
            using (var command = new MySqlCommand(query, connection))
            {
                // Use parameterized queries to prevent SQL injection
                command.Parameters.AddWithValue("@first", user.FirstName);
                command.Parameters.AddWithValue("@last", user.LastName);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@phone", user.Phone);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@usertype", user.UserType);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
        private void NavigateToLogin()
        {
            var login = new Login();
            this.Hide();
            login.Show();
        }

        private static void ShowMessage(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        #endregion
    }

    #region Data Models

    public class UserRegistrationModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
    }

    #endregion
}