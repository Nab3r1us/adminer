using MySqlConnector;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace adminer
{
    public partial class sql_query : Form
    {
        private MySqlConnection mySqlConnection;
        private NpgsqlConnection pSqlConnection;
        private Dictionary<string, string> examples = new Dictionary<string, string>()
        {
            { "CREATE DATABASE", "CREATE DATABASE newDatabaseName" },
            { "CREATE TABLE", "CREATE TABLE newTableName (\n\tcolumn1 datatype,\n\tcolumn2 datatype,\n\t...\n)" },
            { "INSERT", "INSERT INTO tableName (column1, column2, ...)\nVALUES (value1, value2, ...)" },
            { "DELETE", "DELETE FROM tableName WHERE condition;" },
            { "DROP TABLE", "DROP TABLE tableName" },
            { "DROP DATABASE", "DROP DATABASE DatabaseName" },
        };

        public sql_query(MySqlConnection connection, string example)
        {
            mySqlConnection = connection;
            InitializeComponent();
            richTextBox1.Text = examples[example];
        }

        public sql_query(NpgsqlConnection connection, string example)
        {
            pSqlConnection = connection;
            InitializeComponent();
            richTextBox1.Text = examples[example];
        }
    }
}
