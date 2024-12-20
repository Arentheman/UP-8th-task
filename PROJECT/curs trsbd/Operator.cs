using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace curs_trsbd
{ 
    public partial class Operator : Form
    {
        string connectionString = $"Server=ADCLG1;Database=_UP_Chalikyan;Trusted_connection=true;Encrypt=false;Integrated Security=true;";

        private int userId;
        private string userFio;
        private int totalRequests;
        private int currentRequests;
        public Operator(int userId, string userFio)
        {
            InitializeComponent();
            LoadTotalRequests();
            this.userId = userId;
            FillDataGridView();
            InitializeDataGridView();

            dataGridView3.CellClick += dataGridView3_CellDoubleClick;
            this.userFio = userFio;
            pictureBox1.Image = Image.FromFile(@"D:\4 курс\УП 02.01\curs_trsbd\curs trsbd\curs trsbd\Resources\progile.jpg");
            label1.Text = userFio;
            currentRequests = dataGridView3.Rows.Count;
            label3.Text = $"{currentRequests}/{totalRequests}";
        }
        private void LoadTotalRequests()
        {
            string query = "SELECT COUNT(*) FROM Requests";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        totalRequests = (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении общего количества заявок: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateAverageCompletionTime()
        {
            try
            {
                if (dataGridView3.Rows.Count > 0)
                {
                    TimeSpan totalDuration = TimeSpan.Zero;
                    int completedCount = 0;

                    foreach (DataGridViewRow row in dataGridView3.Rows)
                    {
                        if (row.Cells["completionDate"].Value != DBNull.Value && row.Cells["startDate"].Value != DBNull.Value)
                        {
                            DateTime startDate = Convert.ToDateTime(row.Cells["startDate"].Value);
                            DateTime completionDate = Convert.ToDateTime(row.Cells["completionDate"].Value);

                            totalDuration += (completionDate - startDate);
                            completedCount++;
                        }
                    }

                    if (completedCount > 0)
                    {
                        double avgDuration = totalDuration.TotalDays / completedCount;
                        label2.Text = $"Среднее время выполнения: {avgDuration:F1} дней";
                    }
                    else
                    {
                        label2.Text = "Среднее время выполнения: нет завершенных заявок";
                    }
                }
                else
                {
                    label2.Text = "Среднее время выполнения: нет данных";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при вычислении среднего времени выполнения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InitializeDataGridView()
        {
            dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView3.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dataGridView3.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            foreach (DataGridViewColumn column in dataGridView3.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            dataGridView3.ReadOnly = true;
            dataGridView3.AllowUserToAddRows = false;
            dataGridView3.RowHeadersVisible = false;
            dataGridView3.AllowUserToResizeColumns = false;
        }
        public void FillDataGridView()
        {
            try
            {
                string query = @"SELECT
                            r.ID,
                            r.startDate,
                            t.climateTechType,
                            t.climateTechModel,
                            r.problemDescryption,
                            rs.requestStatus,
                            r.completionDate,
                            r.repairParts,
                            s.fio AS staffFio,
                            c.fio AS clientFio
                        FROM 
                            Requests r
                        LEFT JOIN 
                            Technic t ON r.techincID = t.ID
                        LEFT JOIN 
                            RequestStatus rs ON r.requestStatusID = rs.ID
                        LEFT JOIN 
                            Staff s ON r.staffID = s.ID
                        LEFT JOIN 
                            Client c ON r.clientID = c.ID
                        ORDER BY
                            r.ID;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    
                    dataAdapter.Fill(dataTable);
                    label2.Text = AvarageTime.UpdateAverageCompletionTime(dataTable);
                    bindingSource.DataSource = dataTable;
                    dataGridView3.DataSource = bindingSource;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при загрузке данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int requestId = Convert.ToInt32(dataGridView3.Rows[e.RowIndex].Cells["ID"].Value);
                this.Hide();
                OperatorChange operatorChange = new OperatorChange(requestId, userId, userFio);
                operatorChange.ShowDialog();
                this.Close();
                FillDataGridView();
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Logs logs = new Logs();
            logs.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Authorization authorization = new Authorization();
            this.Hide();
            authorization.ShowDialog();
            this.Close();
        }

        private void bindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            try
            {
                if (dataGridView3.DataSource is BindingSource dataTable)
                {
                    label3.Text = $"{bindingSource.Count}/{totalRequests}";
                }
                else
                {
                    currentRequests = 0;
                    label3.Text = $"0/{totalRequests}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении строк после фильтрации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}