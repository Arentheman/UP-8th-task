using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace curs_trsbd
{
    public partial class Master : Form
    {
        string connectionString = $"Server=ADCLG1;Database=_UP_Chalikyan;Trusted_connection=true;Encrypt=false;Integrated Security=true;";
        private int userid;
        private string userFio;

        public Master(int userid, string userFio)
        {
            InitializeComponent();
            pictureBox1.Image = Image.FromFile(@"D:\4 курс\УП 02.01\curs_trsbd\curs trsbd\curs trsbd\Resources\progile.jpg");
            this.userid = userid;
            FillDataGridView();
            InitializeDataGridView();
            dataGridView1.CellClick += DataGridView1_CellClick;
            this.userFio = userFio;
            label1 .Text = userFio;
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
            dataGridView1.AllowUserToResizeColumns = false;

            dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dataGridView1.MultiSelect = false;
        }

        public void FillDataGridView()
        {
            string query = @"
                SELECT r.ID, r.startDate, r.completionDate, t.climateTechModel, 
                       r.problemDescryption, rs.requestStatus, r.repairParts
                FROM Requests r
                LEFT JOIN Technic t ON r.techincID = t.ID
                LEFT JOIN RequestStatus rs ON r.requestStatusID = rs.ID
                WHERE r.staffID = @userId;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userid);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridView1.DataSource = dataTable;

                        dataGridView1.Columns["ID"].Width = 0;
                        dataGridView1.Columns["ID"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                int requestId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value);
                this.Hide();
                
                MasterChange requestDetailsForm = new MasterChange(requestId, userid, userFio);
                requestDetailsForm.ShowDialog();
                this.Close();

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Authorization authorization = new Authorization();
            this.Hide();
            authorization.ShowDialog();
            this.Close();
        }
    }
}
