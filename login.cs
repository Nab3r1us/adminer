using MySqlConnector;
using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace adminer
{
    public partial class login : Form
    {
        DatabaseManager databaseManager = new DatabaseManager();
        public login()
        {
            InitializeComponent();
            engine_comboBox.SelectedIndex = 0;
            server_text.Text = "192.168.117.128";
            login_text.Text = "test";
            password_text.Text = "test";
        }

        public void SetLabel(bool state)
        {
            engine_comboBox.Enabled = !state;
            server_text.Enabled = !state;
            login_text.Enabled = !state;
            password_text.Enabled = !state;
            dbname_text.Enabled = !state;
            loggin_button.Enabled = !state;
            toolStripProgressBar1.Visible = state;
            toolStripStatusLabel1.Text = state ? "Попытка подключения к " + server_text.Text : "";
        }

        private async void Loggin_button_Click(object sender, EventArgs e)
        {
            string engine = engine_comboBox.SelectedItem.ToString();
            string server = server_text.Text;
            string user = login_text.Text;
            string password = password_text.Text;
            string database = engine != "PostgreSQL" ? dbname_text.Text : "postgres";
            string ConnectionString;
            SetLabel(true);
            //try
            //{
                switch (engine)
                {
                    case "MySQL":
                        {
                            ConnectionString = String.Format("server={0};database={1};uid={2};pwd={3};", server, database, user, password);
                            await databaseManager.OpenDatabaseConnectionAndOpenFormAsync(ConnectionString, engine, this);
                            break;
                        }
                    case "PostgreSQL":
                        {
                            ConnectionString = String.Format("Host={0};Username={2};Password={3};Database={1}", server, database, user, password);
                            await databaseManager.OpenDatabaseConnectionAndOpenFormAsync(ConnectionString, engine, this);
                            break;
                        }
                }
            /*}
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении:\n\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetLabel(false);
            }*/
        }

        private void login_FormClosing(object sender, FormClosingEventArgs e)
        {
            databaseManager.CloseConnection();
        }
    }

    public class DatabaseManager
    {
        private IDbConnection connection;
        public async Task OpenDatabaseConnectionAndOpenFormAsync(string connectionString, string provider, login login)
        {
            if (provider == "MySQL")
            {
                var connection = new MySqlConnection(connectionString);
                this.connection = connection;
                await connection.OpenAsync();
                login.SetLabel(false);
                var main = new main(connection, login);
                login.Hide();
                main.Show();
            }
            else if (provider == "PostgreSQL")
            {
                var connection = new NpgsqlConnection(connectionString);
                this.connection = connection;
                await connection.OpenAsync();
                login.SetLabel(false);
                var main = new main(connection, login);
                login.Hide();
                main.Show();
            }
        }

        public void CloseConnection()
        {
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    MessageBox.Show("Соединение было открыто");
                    connection.Close();
                }
            }
        }
    }

}
