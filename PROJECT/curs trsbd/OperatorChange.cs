using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace curs_trsbd
{
    public partial class OperatorChange : Form
    {
        private int requestId;
        private int userId;
        private string userFio;
        private string connectionString = $"Server=ADCLG1;Database=_UP_Chalikyan;Trusted_connection=true;Encrypt=false;Integrated Security=true;";

        public OperatorChange(int requestId, int userId, string userFio)
        {
            InitializeComponent();
            this.requestId = requestId;
            this.userId = userId;

            LoadStaffComboBox();
            LoadRequestDetails();
            this.userFio = userFio;
        }

        private void LoadStaffComboBox()
        {
            string query = "SELECT ID, fio FROM Staff";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                    DataTable staffTable = new DataTable();
                    dataAdapter.Fill(staffTable);

                    comboBox1.DisplayMember = "fio";
                    comboBox1.ValueMember = "ID";
                    comboBox1.DataSource = staffTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка сотрудников: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRequestDetails()
        {
            string query = @"
                SELECT 
                    r.completionDate,
                    c.[message]
                FROM 
                    Requests r
                LEFT JOIN 
                    Comments c ON r.ID = c.requestID
                WHERE 
                    r.ID = @requestID";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@requestID", requestId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                richTextBox1.Text = reader.IsDBNull(1) ? "Комментарий отсутствует" : reader.GetString(1);
                            }
                        }
                    }
                }

                richTextBox1.ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            string updateQuery = @"
                UPDATE Requests
                SET 
                    staffID = @staffID
                WHERE ID = @requestID";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@staffID", comboBox1.SelectedValue);
                        command.Parameters.AddWithValue("@requestID", requestId);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();
                Operator operatorForm = new Operator(userId, userFio);
                operatorForm.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Operator operatorForm = new Operator(userId, userFio);
            operatorForm.ShowDialog();
            this.Close();
        }
    }
}