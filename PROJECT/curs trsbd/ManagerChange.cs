using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace curs_trsbd
{
    public partial class ManagerChange : Form
    {
        string connectionString = $"Server=ADCLG1;Database=_UP_Chalikyan;Trusted_connection=true;Encrypt=false;Integrated Security=true;";
        private int requestId;
        private int userId;
        private string userFio;
        private DateTime startDate;

        public ManagerChange(int requestId, int userId, string userFio)
        {
            InitializeComponent();
            this.requestId = requestId;
            this.userId = userId;
            this.userFio = userFio;

            LoadRequestDetails();
            LoadStaffToComboBox();
        }

        private void LoadRequestDetails()
        {
            string query = @"SELECT 
                                r.startDate,
                                r.completionDate,
                                r.problemDescryption,
                                s.fio AS staffFio
                             FROM 
                                Requests r
                             LEFT JOIN 
                                Staff s ON r.staffID = s.ID
                             WHERE 
                                r.ID = @requestId";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@requestId", requestId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                startDate = reader["startDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["startDate"])
                                    : DateTime.MinValue;

                                if (startDate != DateTime.MinValue)
                                {
                                    dateTimePicker1.MinDate = startDate;
                                    dateTimePicker1.Enabled = checkBox1.Checked;
                                    checkBox1.Enabled = true;

                                    if (reader["completionDate"] != DBNull.Value)
                                    {
                                        dateTimePicker1.Value = Convert.ToDateTime(reader["completionDate"]);
                                    }
                                }
                                else
                                {
                                    dateTimePicker1.Enabled = false;
                                    checkBox1.Enabled = false;
                                }

                                richTextBox1.Text = reader["problemDescryption"].ToString();
                                richTextBox1.ReadOnly = true;

                                if (reader["staffFio"] != DBNull.Value)
                                {
                                    comboBox1.SelectedItem = reader["staffFio"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных заявки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStaffToComboBox()
        {
            string query = "SELECT ID, fio FROM Staff WHERE [type] = 'Мастер'";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            DataTable staffTable = new DataTable();
                            staffTable.Load(reader);

                            comboBox1.DataSource = staffTable;
                            comboBox1.DisplayMember = "fio";
                            comboBox1.ValueMember = "ID";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка мастеров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null || dateTimePicker1.Value < startDate)
            {
                MessageBox.Show("Пожалуйста, выберите корректного мастера и дату завершения.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UpdateRequest();
        }

        private void UpdateRequest()
        {
            string query = @"
                UPDATE Requests
                SET 
                    completionDate = @completionDate,
                    staffID = @staffId
                WHERE 
                    ID = @requestId";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (dateTimePicker1.Enabled)
                        {
                            command.Parameters.AddWithValue("@completionDate", dateTimePicker1.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@completionDate", DBNull.Value);
                        }

                        command.Parameters.AddWithValue("@staffId", comboBox1.SelectedValue ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@requestId", requestId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Заявка успешно обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Hide();
                            Manager clientForm = new Manager(userId, userFio);
                            clientForm.ShowDialog();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить заявку. Попробуйте еще раз.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении заявки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Manager clientForm = new Manager(userId, userFio);
            clientForm.ShowDialog();
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dateTimePicker1.Enabled = checkBox1.Checked;
        }
    }
}
