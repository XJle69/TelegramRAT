using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace RATClassLibrary
{
    public class LoginController
    {
        public static string GetHash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public bool Registration(string email, string username, string password)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                string passwordHash = GetHash(password);

                var emailOrig = db.Users.SingleOrDefault(u => u.Email.ToLower() == email.ToLower());
                var userOrig  = db.Users.SingleOrDefault(u => u.Username.ToLower() == username.ToLower());
                if (emailOrig == null && userOrig == null)
                {
                    User u = new User { Email = email, Username = username, PasswordHash = passwordHash };
                    db.Users.AddRange(u);
                    db.SaveChanges();
                    return true;
                }
                return false;
                
            }
        }

        public User Login(string login, string password)
        {

            using (ApplicationContext db = new ApplicationContext())
            {
                User user = null;
                if (IsValidEmail(login))
                {
                    user = db.Users.SingleOrDefault(u => u.Email.ToLower() == login.ToLower());
                    if (user != null)
                    {
                        if (user.PasswordHash == GetHash(password))
                        {
                            Console.WriteLine("Вход выполнен");
                            return user;
                        }
                    }
                    else Console.WriteLine("Пользыватель не найден");

                }
                else
                {
                    user = db.Users.SingleOrDefault(u => u.Username.ToLower() == login.ToLower());
                    if (user != null)
                    {
                        if (user.PasswordHash == GetHash(password))
                        {
                            Console.WriteLine("Вход выполнен");
                            return user;
                        }
                    }
                    else Console.WriteLine("Пользыватель не найден");
                }
                user = null;
                return user;
            }
        }
        public bool ChangePassword(string oldPasword, string newPassword, User user)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                if (GetHash(oldPasword) == user.PasswordHash)
                {
                    user.PasswordHash = GetHash(newPassword);
                    db.Users.Attach(user);
                    db.Entry(user).Property(x => x.PasswordHash).IsModified = true;
                    db.SaveChanges();
                    return true;
                }
                else return (false);
            }
        }

        public bool ResetPassword(string email)
        {
            User user = null;
            var password = CreatePassword(12);
            try
            {
                using (MailMessage mm = new MailMessage("rat_bot@mail.ru", email))
                {
                    mm.Subject = "RAT Telegram | Смена пароля!";
                    mm.Body =
                    $"Вы успешно сменили пароль!\n" +
                    $"Новый пароль : {password}";
                    mm.IsBodyHtml = false;
                    using (SmtpClient sc = new SmtpClient("smtp.mail.ru", 25))
                    {
                        sc.EnableSsl = true;
                        sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                        sc.UseDefaultCredentials = false;
                        sc.Credentials = new NetworkCredential("rat_bot@mail.ru", "zHp6Us$!H@2*,2E");
                        sc.Send(mm);
                        
                    }
                }
                using (ApplicationContext db = new ApplicationContext())
                {
                    user = db.Users.SingleOrDefault(u => u.Email.ToLower() == email.ToLower());
                    user.PasswordHash = GetHash(password);
                    db.Users.Attach(user);
                    db.Entry(user).Property(x => x.PasswordHash).IsModified = true;
                    db.SaveChanges();
                    return true;
                }
                
            }

            catch (Exception)
            {
                return (false);
            }
            
           
        }
        
    }
}
