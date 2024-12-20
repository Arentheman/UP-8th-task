using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace curs_trsbd
{
    public partial class ClientChange : Form
    {
        private int requestId; 
        private int userid; 
        private string userFio; 
        string connectionString = $"Server=ADCLG1;Database=_UP_Chalikyan;Trusted_connection=true;Encrypt=false;Integrated Security=true;";
        public ClientChange(int requestId, int userid, string userFio)
        {
            InitializeComponent();
            this.requestId = requestId; 
            LoadData();
            FillComboBoxWithModels();
            this.userid = userid;
            comboBox1.SelectedIndex = 0;
            this.userFio = userFio;
            label2.Text = "";
        }
       
        private void LoadData()
        {
            string query = "SELECT techincID, problemDescryption FROM Requests WHERE ID = @requestId";

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
                                // Заполняем форму данными заявки
                                int technicId = reader.GetInt32(reader.GetOrdinal("techincID"));
                                string problemDescription = reader["problemDescryption"].ToString();

                                // Ищем выбранную модель по технике
                                comboBox1.SelectedItem = GetModelByTechnicId(technicId);
                                richTextBox1.Text = problemDescription;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FillComboBoxWithModels()
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
                MessageBox.Show($"Ошибка при заполнении ComboBox: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetModelByTechnicId(int technicId)
        {
            string query = "SELECT climateTechModel FROM Technic WHERE ID = @technicId";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@technicId", technicId);
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString();
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении модели: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedModel = comboBox1.SelectedItem?.ToString();
            string problemDescription = richTextBox1.Text.Trim();

            if (string.IsNullOrEmpty(selectedModel))
            {
                MessageBox.Show("Выберите модель техники.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(problemDescription))
            {
                MessageBox.Show("Опишите проблему.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int technicID = GetTechnicIDByModel(selectedModel);
            if (technicID == -1)
            {
                MessageBox.Show("Выберите допустимую модель техники.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = @"
            UPDATE Requests 
            SET techincID = @techincID, problemDescryption = @problemDescryption
            WHERE ID = @requestId";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@requestId", requestId);
                        command.Parameters.AddWithValue("@techincID", technicID);
                        command.Parameters.AddWithValue("@problemDescryption", problemDescription);

                        command.ExecuteNonQuery();
                        MessageBox.Show("Данные успешно обновлены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Hide();
                        Client clientForm = new Client(userid, userFio);
                        clientForm.ShowDialog();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Client clientForm = new Client(userid, userFio);
            clientForm.ShowDialog();
            this.Close();
        }
    }
}
