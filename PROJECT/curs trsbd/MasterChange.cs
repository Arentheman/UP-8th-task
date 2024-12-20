using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace curs_trsbd
{
    public partial class MasterChange : Form
    {
        private int requestId;
        private int userid;
        private string userFio;
        private DateTime? startDate;
        private DateTime? completionDate;
        private string connectionString = $"Server=ADCLG1;Database=_UP_Chalikyan;Trusted_connection=true;Encrypt=false;Integrated Security=true;";

        public MasterChange(int requestId, int userid, string userFio)
        {
            InitializeComponent();
            this.requestId = requestId;
            this.userid = userid;
            LoadComment();
            LoadRequestDetails();
            UpdateControlStates();
            this.userFio = userFio;
        }
        private void LoadComment()
        {
            string query = @"
                SELECT [message]
                FROM Comments
                WHERE requestID = @requestID AND staffID = @staffID";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@requestID", requestId);
                        command.Parameters.AddWithValue("@staffID", userid);

                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            richTextBox2.Text = result.ToString();
                        }
                        else
                        {
                            richTextBox2.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки комментария: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateControlStates()
        {
            bool hasStartDate = startDate.HasValue;
            bool isInProgress = GetRequestStatus()==2;

            button3.Enabled = !hasStartDate; 
            button4.Enabled = isInProgress; 

            richTextBox1.ReadOnly = isInProgress;
        }
        private int GetRequestStatus()
        {
            string query = "SELECT requestStatusID FROM Requests WHERE ID = @requestId";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@requestId", requestId);

                        object result = command.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int statusId))
                        {
                            return statusId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения статуса заявки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return -1;
        }

        private void UpdateRequestStatus(int statusId, bool setCompletionDate, bool setStartDate = false)
        {
            string updateRequestQuery = @"
                UPDATE Requests
                SET 
                    requestStatusID = @statusID,
                    completionDate = @completionDate" +
                            (setStartDate ? ", startDate = @startDate" : "") + @"
                WHERE ID = @requestID";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand updateCommand = new SqlCommand(updateRequestQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@statusID", statusId);
                        updateCommand.Parameters.AddWithValue("@requestID", requestId);

                        if (setCompletionDate)
                        {
                            updateCommand.Parameters.AddWithValue("@completionDate", DateTime.Now);
                        }
                        else
                        {
                            updateCommand.Parameters.AddWithValue("@completionDate", DBNull.Value);
                        }

                        if (setStartDate)
                        {
                            updateCommand.Parameters.AddWithValue("@startDate", DateTime.Now);
                        }

                        updateCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Статус заявки успешно обновлён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRequestDetails()
        {
            string query = "SELECT startDate, completionDate, repairParts FROM Requests WHERE ID = @requestId";

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
                                richTextBox1.Text = reader["repairParts"].ToString();

                                completionDate = reader["completionDate"] as DateTime?;
                                startDate = reader["startDate"] as DateTime?;

                                UpdateControlStates();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveChanges();
            this.Hide();
            Master masterForm = new Master(userid, userFio);
            masterForm.ShowDialog();
            this.Close();
        }

        private void SaveChanges()
        {
            string selectCommentQuery = @"
        SELECT COUNT(1)
        FROM Comments
        WHERE requestID = @requestID AND staffID = @staffID";

            string insertCommentQuery = @"
        INSERT INTO Comments ([message], staffID, requestID)
        VALUES (@message, @staffID, @requestID)";

            string updateCommentQuery = @"
        UPDATE Comments
        SET [message] = @message
        WHERE requestID = @requestID AND staffID = @staffID";

            string updateRepairPartsQuery = @"
        UPDATE Requests
        SET repairParts = @repairParts
        WHERE ID = @requestID";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand selectCommand = new SqlCommand(selectCommentQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@requestID", requestId);
                        selectCommand.Parameters.AddWithValue("@staffID", userid);

                        int commentExists = (int)selectCommand.ExecuteScalar();

                        if (!string.IsNullOrWhiteSpace(richTextBox2.Text))
                        {
                            if (commentExists > 0)
                            {
                                using (SqlCommand updateCommentCommand = new SqlCommand(updateCommentQuery, connection))
                                {
                                    updateCommentCommand.Parameters.AddWithValue("@message", richTextBox2.Text);
                                    updateCommentCommand.Parameters.AddWithValue("@staffID", userid);
                                    updateCommentCommand.Parameters.AddWithValue("@requestID", requestId);

                                    updateCommentCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                using (SqlCommand insertCommentCommand = new SqlCommand(insertCommentQuery, connection))
                                {
                                    insertCommentCommand.Parameters.AddWithValue("@message", richTextBox2.Text);
                                    insertCommentCommand.Parameters.AddWithValue("@staffID", userid);
                                    insertCommentCommand.Parameters.AddWithValue("@requestID", requestId);

                                    insertCommentCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(richTextBox1.Text))
                    {
                        using (SqlCommand updateRepairPartsCommand = new SqlCommand(updateRepairPartsQuery, connection))
                        {
                            updateRepairPartsCommand.Parameters.AddWithValue("@repairParts", richTextBox1.Text);
                            updateRepairPartsCommand.Parameters.AddWithValue("@requestID", requestId);

                            updateRepairPartsCommand.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            UpdateRequestStatus(2, false, true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UpdateRequestStatus(1, true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Master masterForm = new Master(userid, userFio);
            masterForm.ShowDialog();
            this.Close();
        }
    }
}
