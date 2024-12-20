using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace curs_trsbd
{
    public partial class Logs : Form
    {
        string connectionString = $"Server=ADCLG1;Database=_UP_Chalikyan;Trusted_connection=true;Encrypt=false;Integrated Security=true;";
        public Logs()
        {
            InitializeComponent();
            InitializeDataGridView();
            FillLoginHistoryDataGridView();
        }
        private void InitializeDataGridView()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToResizeColumns = false;
        }
        private void FillLoginHistoryDataGridView()
        {
            string query = "SELECT loginHistoryId, logins, attemptDate, success FROM loginHistory";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
