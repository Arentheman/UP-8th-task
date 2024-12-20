using QRCoder;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace curs_trsbd
{
    public partial class QR : Form
    {
        private string url = "https://code-qr.ru/blog/qr-code-survey";
        public QR()
        {
            InitializeComponent();
            GenerateQRCode(url);
        }

        private void GenerateQRCode(string url)
        {
            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

                    using (QRCode qrCode = new QRCode(qrCodeData))
                    {
                        int pictureBoxWidth = pictureBox1.Width;
                        int pictureBoxHeight = pictureBox1.Height;

                        Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true);

                        Bitmap resizedQRCodeImage = new Bitmap(qrCodeImage, new Size(pictureBoxWidth, pictureBoxHeight));

                        pictureBox1.Image = resizedQRCodeImage;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации QR-кода: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
