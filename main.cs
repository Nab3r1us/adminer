using MySqlConnector;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace adminer
{
    public partial class main : Form
    {
        private MySqlConnection mySqlConnection;
        private NpgsqlConnection pSqlConnection;
        private login login;
        private string currentTable;

        public main(MySqlConnection connection, login login)
        {
            mySqlConnection = connection;
            this.login = login;
            InitializeComponent();
            string query = "SHOW DATABASES";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    currentDB.Items.Clear();
                    while (reader.Read())
                    {
                        currentDB.Items.Add(reader.GetString(0));
                    }
                }
            }
            string currentDatabase = connection.Database;
            currentDB.SelectedItem = currentDatabase;
            if (currentDatabase != "")
            {
                query = "SHOW TABLES";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        listTables.Items.Clear();
                        while (reader.Read())
                        {
                            listTables.Items.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }

        public main(NpgsqlConnection connection, login login)
        {
            pSqlConnection = connection;
            this.login = login;
            InitializeComponent();
            string query = "SELECT datname FROM pg_database";
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    currentDB.Items.Clear();
                    while (reader.Read())
                    {
                        string dbName = reader.GetString(0);
                        currentDB.Items.Add(dbName);
                    }
                }
            }
            string currentDatabase = connection.Database;
            currentDB.SelectedItem = currentDatabase;
            if (currentDatabase != "")
            {
                DataTable schema = connection.GetSchema("Tables");
                listTables.Items.Clear();
                foreach (DataRow row in schema.Rows)
                {
                    listTables.Items.Add(row["table_name"].ToString());
                }
            }
        }

        private void main_FormClosed(object sender, FormClosedEventArgs e)
        {
            login.SetLabel(false);
            mySqlConnection?.Close();
            pSqlConnection?.Close();
            login.Show();
            login.Focus();
        }

        private void FillListTables()
        {
            if (mySqlConnection != null)
            {
                string query = "SHOW TABLES";
                using (MySqlCommand command = new MySqlCommand(query, mySqlConnection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        listTables.Items.Clear();
                        while (reader.Read())
                        {
                            listTables.Items.Add(reader.GetString(0));
                        }
                    }
                }
            }
            else if (pSqlConnection != null)
            {
                DataTable schema = pSqlConnection.GetSchema("Tables");
                listTables.Items.Clear();
                foreach (DataRow row in schema.Rows)
                {
                    listTables.Items.Add(row["table_name"].ToString());
                }
            }
        }

        private void FillColumnsTable(string tableName)
        {
            if (mySqlConnection != null)
            {
                DataTable schema = mySqlConnection.GetSchema("Columns", new string[] { null, null, tableName });
                DataTable fieldsTable = new DataTable();
                fieldsTable.Columns.Add("Ключ", typeof(string));
                fieldsTable.Columns.Add("Поле", typeof(string));
                fieldsTable.Columns.Add("Тип(Длина)", typeof(string));
                fieldsTable.Columns.Add("NULL", typeof(string));
                fieldsTable.Columns.Add("Extra", typeof(string));
                foreach (DataRow row in schema.Rows)
                {
                    string key = row["column_key"].ToString();
                    string columnName = row["column_name"].ToString();
                    string columnType = row["column_type"].ToString();
                    string nullable = row["is_nullable"].ToString();
                    string extra = row["extra"].ToString();
                    fieldsTable.Rows.Add(key, columnName, columnType, nullable, extra);
                }
                dataGridView1.DataSource = fieldsTable;
            }
            else if (pSqlConnection != null)
            {
                DataTable schema = pSqlConnection.GetSchema("Columns", new string[] { null, "public", tableName });
                DataTable fieldsTable = new DataTable();
                fieldsTable.Columns.Add("Поле", typeof(string));
                fieldsTable.Columns.Add("Тип(Длина)", typeof(string));
                fieldsTable.Columns.Add("NULL", typeof(string));
                foreach (DataRow row in schema.Rows)
                {
                    string columnName = row["column_name"].ToString();
                    string columnType = row["data_type"].ToString() + (row["character_maximum_length"].ToString() != "" ? $"({row["character_maximum_length"]})" : "");
                    string nullable = row["is_nullable"].ToString();
                    fieldsTable.Rows.Add(columnName, columnType, nullable);
                }
                dataGridView1.DataSource = fieldsTable;
            }
        }

        private void FillDataTable(string tableName)
        {

            if (mySqlConnection != null)
            {
                string query = $"SELECT * FROM {tableName}";
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, mySqlConnection);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                dataGridView2.ReadOnly = true;
                dataGridView2.DataSource = dataSet.Tables[0];
            }
            else if (pSqlConnection != null)
            {
                string query = $"SELECT * FROM {tableName}";
                NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, pSqlConnection);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                dataGridView2.ReadOnly = true;
                dataGridView2.DataSource = dataSet.Tables[0];
            }
        }

        private void currentDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            mySqlConnection?.ChangeDatabase(currentDB.SelectedItem.ToString());
            pSqlConnection?.ChangeDatabase(currentDB.SelectedItem.ToString());
            FillListTables();
            dataGridView1.DataSource = null;
        }

        private void listTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentTable = listTables.SelectedItem != null ? listTables.SelectedItem.ToString() : "";
            FillColumnsTable(currentTable);
            FillDataTable(currentTable);
        }

        private void OpenSqlQuery(string example)
        {
            if (example.LastIndexOf("DATABASE") <= 0)
            {
                MessageBox.Show("Не выбрана база данных!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (mySqlConnection != null)
            {
                sql_query sql_Query = new sql_query(mySqlConnection, example);
                sql_Query.Show();
            }
            else if (pSqlConnection != null)
            {
                sql_query sql_Query = new sql_query(pSqlConnection, example);
                sql_Query.Show();
            }
        }

        private void dATABASEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSqlQuery("CREATE DATABASE");
        }

        private void tABLEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSqlQuery("CREATE TABLE");
        }

        private void iNSERTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSqlQuery("INSERT");
        }
    }
}
