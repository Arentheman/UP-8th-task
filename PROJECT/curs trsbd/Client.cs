using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace curs_trsbd
{
    public partial class Client : Form
    {
        string connectionString = $"Server=ADCLG1;Database=_UP_Chalikyan;Trusted_connection=true;Encrypt=false;Integrated Security=true;";
        private int userId;
        private string userFio;

        public Client(int userId, string userFio)
        {
            InitializeComponent();
            pictureBox1.Image = Image.FromFile(@"D:\4 курс\УП 02.01\curs_trsbd\curs trsbd\curs trsbd\Resources\progile.jpg");
            this.userId = userId;
            FillDataGridView();
            dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;

            dataGridView1.RowPrePaint += dataGridView1_RowPrePaint;
            FillComboBoxWithModels();
            comboBox1.SelectedIndex = 0;
            dataGridView1.CellClick += DataGridView1_CellClick;
            this.userFio = userFio;
            label1 .Text = userFio;
        }

        public void FillDataGridView()
        {
            string query = @"
                SELECT r.ID as requestId, r.startDate, r.completionDate, t.climateTechModel, 
                       r.problemDescryption, rs.requestStatus
                FROM Requests r
                LEFT JOIN Technic t ON r.techincID = t.ID
                LEFT JOIN RequestStatus rs ON r.requestStatusID = rs.ID
                WHERE r.clientID = @userId;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridView1.DataSource = dataTable;

                        // Добавляем скрытую колонку для requestId
                        DataGridViewColumn requestIdColumn = new DataGridViewTextBoxColumn
                        {
                            Name = "requestId",
                            DataPropertyName = "requestId",
                            Visible = false // Скрываем колонку
                        };
                        dataGridView1.Columns.Add(requestIdColumn);

                        // Устанавливаем ширину для остальных колонок
                        foreach (DataGridViewColumn column in dataGridView1.Columns)
                        {
                            if (column.Name != "requestId") // Не меняем ширину для скрытой колонки
                            {
                                column.Width = 130;
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            }
                        }

                        dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        dataGridView1.RowHeadersVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            int totalWidth = 0;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
                totalWidth += column.Width;

            totalWidth += dataGridView1.RowHeadersWidth; 

            dataGridView1.Width = totalWidth + 20;

            int totalHeight = dataGridView1.Rows.Cast<DataGridViewRow>().Sum(r => r.Height) + dataGridView1.ColumnHeadersHeight;
            dataGridView1.Height = Math.Min(totalHeight, 600);
            this.Width = dataGridView1.Width + 40;
            this.Height = Math.Max(dataGridView1.Height + 60, 400);
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                if (!dataGridView1.Rows[e.RowIndex].IsNewRow)
                {
                    var status = dataGridView1.Rows[e.RowIndex].Cells["requestStatus"].Value?.ToString();

                    if (status == "Выполнено")
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                    else if (status == "В процессе")
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                    else if (status == "В ожидании")
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка форматирования строки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void FillComboBoxWithModels()
        {
            string query = "SELECT climateTechModel FROM Technic";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader["climateTechModel"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedModel = comboBox1.SelectedItem.ToString();
            string problemDescription = textBox1.Text.Trim();

            int technicID = GetTechnicIDByModel(selectedModel);
            if (textBox1.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Напишите описание поломки", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (technicID == -1)
            {
                MessageBox.Show("Выберите допустимую модель техники!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = @"
                INSERT INTO Requests 
                (clientID, techincID, problemDescryption, requestStatusID)
                VALUES 
                (@clientID, @techincID, @problemDescryption, @requestStatusID)";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@clientID", userId);
                        command.Parameters.AddWithValue("@techincID", technicID);
                        command.Parameters.AddWithValue("@problemDescryption", problemDescription);
                        command.Parameters.AddWithValue("@requestStatusID", 3);

                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Запрос успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            FillDataGridView();
            dataGridView1.RowPrePaint += dataGridView1_RowPrePaint;
        }

        private int GetTechnicIDByModel(string model)
        {
            string query = "SELECT ID FROM Technic WHERE climateTechModel = @model";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@model", model);
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении ID модели: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }


        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var requestId = dataGridView1.Rows[e.RowIndex].Cells["requestId"].Value?.ToString();

                var status = dataGridView1.Rows[e.RowIndex].Cells["requestStatus"].Value?.ToString();

                if (status == "В ожидании")
                {
                    if (requestId != null)
                    {
                        ClientChange editForm = new ClientChange(Convert.ToInt32(requestId), userId, userFio);
                        this.Hide();
                        editForm.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка: не найдено ID заявки.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Редактирование доступно только для заявок со статусом 'В ожидании'.", "Редактирование недоступно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Authorization authorization = new Authorization();
            this.Hide();
            authorization.ShowDialog();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            QR code = new QR();
            code.ShowDialog();
        }
    }
}