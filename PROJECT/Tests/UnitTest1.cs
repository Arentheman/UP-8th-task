using Microsoft.VisualStudio.TestTools.UnitTesting;
using curs_trsbd;
using System;
using System.Drawing;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void AuthenticateUser_ValidStaffCredentials_ReturnsAccessLevel()
        {
            var auth = new Authorization();
            int userId;
            string userFio;
            string login = "login2";
            string password = "pass2";

            string result = auth.AuthenticateUser(login, password, out userId, out userFio);

            Assert.IsNotNull(result, "Access level should not be null for valid staff credentials.");
            Assert.AreEqual("мастер", result, "Expected access level for valid staff is 'полный'.");
        }

        [TestMethod]
        public void AuthenticateUser_InvalidCredentials_ReturnsNull()
        {
            var auth = new Authorization();
            int userId;
            string userFio;
            string login = "invalid_login";
            string password = "invalid_password";

            string result = auth.AuthenticateUser(login, password, out userId, out userFio);

            Assert.IsNull(result, "Access level should be null for invalid credentials.");
        }

        [TestMethod]
        public void LogLoginAttempt_ValidLogin_SuccessLogged()
        {
            var auth = new Authorization();
            string login = "test_user";
            bool success = true;

            try
            {
                auth.LogLoginAttempt(login, success);

                Assert.IsTrue(true, "LogLoginAttempt executed without exceptions.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"LogLoginAttempt threw an exception: {ex.Message}");
            }
        }

        [TestMethod]
        public void CheckLoginExists_ExistingLogin_ReturnsTrue()
        {
            var auth = new Authorization();
            string existingLogin = "login1";

            bool exists = auth.CheckLoginExists(existingLogin);

            Assert.IsTrue(exists, "CheckLoginExists should return true for existing login.");
        }

        [TestMethod]
        public void CreateImage_ValidDimensions_ReturnsBitmap()
        {
            var auth = new Authorization();
            int width = 100;
            int height = 50;

            Bitmap image = auth.CreateImage(width, height);

            Assert.IsNotNull(image, "CreateImage should return a non-null Bitmap.");
            Assert.AreEqual(width, image.Width, "Image width does not match input width.");
            Assert.AreEqual(height, image.Height, "Image height does not match input height.");
        }
    }
}