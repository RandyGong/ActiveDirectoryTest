using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveDirectoryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var dirEntry = GetDirectoryObject();
            Console.WriteLine("Hello from outside. dirEntry.Name = " + dirEntry.Name);

            Console.WriteLine("Is g exist : " + ExistWinUser("g"));

            //CreateLocalUser("userFromCode", "123456", "");

            adduser("userFromCodeLater");
            Console.Read();
        }

        /// 获得DirectoryEntry对象实例,以管理员登陆AD
        /// </summary>
        /// <returns></returns>
        private static DirectoryEntry GetDirectoryObject()
        {
            DirectoryEntry entry = null;
            try
            {
                //entry = new DirectoryEntry("LDAP://localhost", "GongJiChang", "abcd1234*", AuthenticationTypes.Secure);
                entry = new DirectoryEntry("WinNT://localhost", "GongJiChang", "abcd1234*", AuthenticationTypes.Secure);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return entry;
        }

        /// <summary>
        /// 创建Windows帐户
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        public static void CreateLocalUser(string username, string password, string description)
        {
            //DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            var localMachine = GetDirectoryObject();
            var newUser = localMachine.Children.Add(username, "user");
            newUser.Invoke("SetPassword", new object[] { password });
            newUser.Invoke("Put", new object[] { "Description", description });
            newUser.CommitChanges();
            localMachine.Close();
            newUser.Close();
        }

        public static void adduser(string addaccount)
        {

            //DirectoryEntry decu = new DirectoryEntry("LDAP//", "用户名", "密码");
            DirectoryEntry decu = GetDirectoryObject();
            DirectoryEntries users = decu.Children;
            DirectoryEntry user = users.Add("CN=" + addaccount, "user");

            user.Properties["userPrincipalName"].Add(addaccount);
            user.Properties["samAccountName"].Add(addaccount);    //添加用户的帐号名称  
            user.Properties["pwdLastSet"].Value = 0;           //设置上一次登陆密码为空,用户在新登陆后需要重新设置密码  
            user.Properties["userAccountControl"].Value = 544;   //有效用户   ,应该设为512,却出错  
            user.CommitChanges();                                                 //确认改变,写入AD  

        }

        /// <summary>
        /// 判断Windows用户是否存在
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool ExistWinUser(string username)
        {
            try
            {
                using (DirectoryEntry localMachine = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                {
                    var user = localMachine.Children.Find(username, "user");
                    return user != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
