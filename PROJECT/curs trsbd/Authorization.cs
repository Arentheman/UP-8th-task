using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace curs_trsbd
{
    public partial class Authorization : Form
    {
        string connectionString = $"Server=ADCLG1;Database=_UP_Chalikyan;Trusted_connection=true;Encrypt=false;Integrated Security=true;";
        private string text = String.Empty;
        private int failedAttempts = 0;
        private Timer blockTimer;
        private int remainingTime;
        private bool isPermanentlyBlocked = false;
        private bool lastChance = true;

        public Authorization()
        {
            InitializeComponent();
            pictureBox1.Image = this.CreateImage(pictureBox1.Width, pictureBox1.Height);
            blockTimer = new Timer();
            blockTimer.Interval = 1;
            blockTimer.Tick += BlockTimer_Tick;

            textBox3.Visible = false;
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
        }

        private void TextBoxToLower(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int cursorPosition = textBox.SelectionStart;
                textBox.Text = textBox.Text.ToLower();
                textBox.SelectionStart = cursorPosition;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox3.Text) && failedAttempts > 0)
            {
                MessageBox.Show("Пожалуйста, введите капчу.","Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId;
            string userFio;
            string access = AuthenticateUser(textBox1.Text.Trim(), textBox2.Text.Trim(), out userId, out userFio);

            if (access != null)
            {
                LogLoginAttempt(textBox1.Text.Trim(), true);

                MessageBox.Show($"Здравствуйте, {userFio}. Ваш уровень доступа: {access}", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Hide();

                switch (access)
                {
                    case "полный":
                        new Operator(userId, userFio).ShowDialog();
                        break;
                    case "мастер":
                        new Master(userId, userFio).ShowDialog();
                        break;
                    case "менеджер":
                        new Manager(userId, userFio).ShowDialog();
                        break;
                    case "клиентский":
                        new Client(userId, userFio).ShowDialog();
                        break;
                }

                Close();
            }
            else
            {
                bool isLoginValid = CheckLoginExists(textBox1.Text.Trim());
                if (isLoginValid)
                {
                    LogLoginAttempt(textBox1.Text.Trim(), false);
                }

                HandleFailedLogin(textBox1.Text.Trim(), textBox2.Text.Trim());
            }
        }

        public string AuthenticateUser(string login, string password, out int userId, out string userFio)
        {
            userId = -1;
            userFio = null;

            if (AuthenticateFromStaff(login, password, out userId, out userFio, out string accessLevel))
            {
                return accessLevel;
            }

            if (AuthenticateFromClient(login, password, out userId, out userFio))
            {
                return "клиентский";
            }

            return null;
        }

        private bool AuthenticateFromStaff(string login, string password, out int userId, out string userFio, out string accessLevel)
        {
            userId = -1;
            userFio = null;
            accessLevel = null;

            string query = @"
                SELECT ID, fio, 
                       CASE 
                           WHEN [type] = 'Оператор' THEN 'полный'
                           WHEN [type] = 'Мастер' THEN 'мастер'
                           WHEN [type] = 'Менеджер' THEN 'менеджер'
                           ELSE 'клиентский'
                       END AS AccessLevel
                FROM Staff
                WHERE logins = @login AND passwords = @password";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId = Convert.ToInt32(reader["ID"]);
                            userFio = reader["fio"].ToString();
                            accessLevel = reader["AccessLevel"].ToString();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool AuthenticateFromClient(string login, string password, out int userId, out string userFio)
        {
            userId = -1;
            userFio = null;

            string query = @"
                SELECT ID, fio
                FROM Client
                WHERE logins = @login AND passwords = @password";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId = Convert.ToInt32(reader["ID"]);
                            userFio = reader["fio"].ToString();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void LogLoginAttempt(string login, bool success)
        {
            string query = @"
            INSERT INTO loginHistory (logins, attemptDate, success)
            VALUES (@login, @attemptDate, @success)";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@attemptDate", DateTime.Now);
                        command.Parameters.AddWithValue("@success", success);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка логирования: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool CheckLoginExists(string login)
        {
            string query = @"
                SELECT COUNT(1)
                FROM Staff
                WHERE logins = @login
                UNION ALL
                SELECT COUNT(1)
                FROM Client
                WHERE logins = @login";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);

                    int result = (int)command.ExecuteScalar();
                    return result > 0;
                }
            }
        }

        private void HandleFailedLogin(string login, string password)
        {
            if (failedAttempts > 0 && textBox3.Text != text)
            {
                MessageBox.Show("Неправильная капча", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            failedAttempts++;
            MessageBox.Show("Неправильный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (failedAttempts >= 3)
            {
                if (lastChance && failedAttempts < 4)
                {
                    button1.Enabled = false;
                    remainingTime = 180;
                    blockTimer.Start();
                    label4.Text = FormatTime(remainingTime);
                    lastChance = false;
                }
                else
                {
                    isPermanentlyBlocked = true;
                    MessageBox.Show("Вы заблокированы навсегда. Попробуйте перезайти.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button1.Enabled = false;
                }
            }

            if (failedAttempts >= 1)
            {
                textBox3.Visible = true;
                pictureBox1.Visible = true;
                pictureBox2.Visible = true;
                pictureBox1.Image = this.CreateImage(pictureBox1.Width, pictureBox1.Height);
            }
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            remainingTime--;
            label4.Text = FormatTime(remainingTime);

            if (remainingTime <= 0)
            {
                blockTimer.Stop();
                button1.Enabled = true;
                label4.Text = string.Empty;
            }
        }

        private string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            seconds %= 60;
            return $"{minutes}:{seconds:D2}";
        }

        public Bitmap CreateImage(int Width, int Height)
        {
            Random rnd = new Random();
            Bitmap result = new Bitmap(Width, Height);
            int Xpos = rnd.Next(0, Width - 50);
            int Ypos = rnd.Next(15, Height - 15);
            Brush[] colors = { Brushes.Black, Brushes.Red, Brushes.White };
            Graphics g = Graphics.FromImage((Image)result);
            g.Clear(Color.Gray);
            text = String.Empty;
            string ALF = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM";
            for (int i = 0; i < 5; ++i)
                text += ALF[rnd.Next(ALF.Length)];
            g.DrawString(text, new Font("Arial", 15), colors[rnd.Next(colors.Length)], new PointF(Xpos, Ypos));
            g.DrawLine(Pens.Black, new Point(0, 0), new Point(Width - 1, Height - 1));
            g.DrawLine(Pens.Black, new Point(0, Height - 1), new Point(Width - 1, 0));
            return result;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = this.CreateImage(pictureBox1.Width, pictureBox1.Height);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.PasswordChar == '*')
            {
                textBox2.PasswordChar = '\0';
            }
            else
            {
                textBox2.PasswordChar = '*';
            }
        }
    }
}