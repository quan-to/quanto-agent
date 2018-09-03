using System;
using System.IO;
using QuantoAgent.Exceptions;
using QuantoAgent.Log;
using SQLite;

namespace QuantoAgent.Database {
    public static class UserManager {
        static readonly string FileName = Path.Combine("db","users.db");
        static readonly SQLiteConnection conn;

        static UserManager() {
            Directory.CreateDirectory("db");
            conn = new SQLiteConnection(FileName);
            var x = conn.GetTableInfo("DBUser");
            if (x.Count == 0) {
                conn.CreateTable<DBUser>();
            }

            if (GetUser("admin") == null) {
                Logger.Log("UserManager", "Admin user does not exists. Creating default admin with password admin");
                AddUser("Administrator", "admin", "admin");
            }

            AppDomain.CurrentDomain.ProcessExit += (sender, e) => UserManagerDestructor();
        }

        public static void AddUser(string name, string username, string password) {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            string id = Guid.NewGuid().ToString();

            var user = _GetUser(username);
            if (user == null) {
                conn.Insert(new DBUser { UserId = id, UserName = username, Name = name, Password = hashedPassword });
            } else {
                throw new UserAlreadyExists();
            }
            conn.Commit();
        }

        public static DBUser CheckUser(string username, string password) {
            var user = _GetUser(username);
            if (user == null) {
                return null;
            }

            var hash = user.Password;
            user.Password = "";

            return BCrypt.Net.BCrypt.Verify(password, hash) ? user : null;
        }

        public static void ChangePassword(string username, string password) {
            var user = GetUser(username);
            if (user == null) {
                throw new Exception("User not found");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            conn.Update(user);
            conn.Commit();
        }

        public static DBUser GetUser(string username) {
            var user = _GetUser(username);
            if (user == null) {
                return null;
            }

            user.Password = "";

            return user;
        }

        static DBUser _GetUser(string username) {
            var res = conn.Table<DBUser>().Where(a => a.UserName == username);
            return res.Count() > 0 ? res.First() : null;
        }

        static void UserManagerDestructor() {
            conn.Close();
        }
    }
}