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

            Console.WriteLine("Is userFromCode exist : " + ExistWinUser("userFromCode"));

            //CreateLocalUser("userFromCode", "abcd1234*", "");
            //SetEnable("userFromCode");
            //EnableUser("userFromCode");
            //adduser("userFromCodeLater");
            //GetUser();
            //EnumComputers();
            //EnumUsers();
            ListUsers();
            //changeSystePassWord("admin", "1234abcd*");
            //var lenovo = GetDirectoryEntryByAccount("admin");
            //FindUser("lenovo");
            Console.Read();
        }

        public static string userName = "admin";
        public static string pwd = "abcd1234*";

        public static void GetUser()
        {
            DirectorySearcher search = new DirectorySearcher(
               new DirectoryEntry("WinNT://LENOVO-PC", userName, pwd),
               "(&(objectClass=user)(objectCategory=person)(!(userAccountControl:1.2.840.113556.1.4.803:=2))((mail=*)))", // filter out disabled accounts
               new string[] { "givenName", "sn", "mail", "telephoneNumber", "l", "company", "memberOf", "userPrincipalName", "userAccountControl", "lastLogonTimestamp", "departmentNumber", "department", "description", "info", "mobile" }, SearchScope.OneLevel); // search only the current level
            var res = search.FindAll();

        }

        static void EnumComputers()
        {
            using (DirectoryEntry root = new DirectoryEntry("WinNT:"))
            {
                foreach (DirectoryEntry domain in root.Children)
                {
                    Console.WriteLine("Domain | WorkGroup:\t" + domain.Name);
                    foreach (DirectoryEntry computer in domain.Children)
                    {
                        Console.WriteLine("Computer:\t" + computer.Name);
                    }
                }
            }
        }

        static void EnumUsers()
        {
            //System.DirectoryServices.DirectoryEntry pEntry = new System.DirectoryServices.DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            //System.DirectoryServices.DirectoryEntry ADuser = pEntry.Children.Find(username, "user");
            using (DirectoryEntry root = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                foreach (DirectoryEntry domain in root.Children)
                {
                    if (domain.SchemaClassName == "User")
                        Console.WriteLine("Domain | Name:\t" + domain.Name);

                }
            }
        }

        /// 要修改的用户的用户名
        /// 修改后的新密码
        static public void changeSystePassWord(string username, string newpassword)
        {
            System.DirectoryServices.DirectoryEntry pEntry = new System.DirectoryServices.DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            System.DirectoryServices.DirectoryEntry ADuser = pEntry.Children.Find(username, "user");
            if (ADuser != null && ADuser.SchemaClassName == "User")
            {
                ADuser.Password = newpassword;
                ADuser.CommitChanges();
            }
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
                entry = new DirectoryEntry("WinNT://LENOVO-PC", userName, pwd, AuthenticationTypes.Secure);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return entry;
        }

        public static void ListUsers()
        {
            //DirectoryEntry entry = new DirectoryEntry(path);
            var entry = GetDirectoryObject();
            DirectorySearcher searcher = new DirectorySearcher(entry);
            searcher.Filter = "(objectClass=*)";
            searcher.PropertiesToLoad.Clear();
            SearchResultCollection searchResultCollection = searcher.FindAll();
            IList<Users> list = VisitSearchResultCollection(searchResultCollection);

            foreach (var s in list)
            {
                Console.WriteLine(s);
            }
        }

        public static IList<Users> VisitSearchResultCollection(SearchResultCollection resultCollection)
        {
            IList<Users> userList = new List<Users>();
            foreach (SearchResult result in resultCollection)
            {
                string userName = string.Empty;
                string displayName = string.Empty;
                if (result.Properties.Contains("samaccountname"))
                {
                    ResultPropertyValueCollection resultValue = result.Properties["samaccountname"];
                    if (resultValue != null && resultValue.Count > 0 && resultValue[0] != null)
                    {
                        userName = resultValue[0].ToString();
                    }
                }
                if (result.Properties.Contains("displayname"))
                {
                    ResultPropertyValueCollection resultValue = result.Properties["displayname"];
                    if (resultValue != null && resultValue.Count > 0 && resultValue[0] != null)
                    {
                        displayName = resultValue[0].ToString();
                    }
                }
                userList.Add(new Users(userName, displayName));
            }
            return userList;
        }

        public class Users
        {
            public Users(string un, string dn)
            {
                userName = un;
                displayName = dn;
            }
            public string userName { get; set; }
            public string displayName { get; set; }
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
            //newUser.Properties["userAccountControl"].Value = 544;
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

            //user.Properties["userPrincipalName"].Add(addaccount);
            //user.Properties["sAMAccountName"].Add(addaccount);    //添加用户的帐号名称  
            //user.Properties["sAMAccountName"].Value = addaccount; 
            //user.Properties["pwdLastSet"].Value = 0;           //设置上一次登陆密码为空,用户在新登陆后需要重新设置密码  
            //user.Properties["userAccountControl"].Value = 544;   //有效用户 启用用户  ,应该设为512,却出错  
            user.CommitChanges();                                                 //确认改变,写入AD  
            user.Close();
            decu.Close();
        }

        public static void SetEnable(string user)
        {
            DirectoryEntry ude = new DirectoryEntry("WinNT://localhost", user, "abcd1234*", AuthenticationTypes.Secure);
            ude.Properties["userAccountControl"].Value = 544;
            ude.CommitChanges();
            ude.Close();

        }

        ///  
        ///根据用户公共名称取得用户的 对象  
        ///  
        ///  用户公共名称   
        ///如果找到该用户，则返回用户的 对象；否则返回 null  
        public static DirectoryEntry GetDirectoryEntry(string commonName)
        {
            DirectoryEntry de = GetDirectoryObject();
            DirectorySearcher deSearch = new DirectorySearcher(de);
            deSearch.Filter = "(&(&(objectCategory=person)(objectClass=user))(cn=" + commonName + "))";
            deSearch.SearchScope = SearchScope.Subtree;
            try
            {
                SearchResult result = deSearch.FindOne();
                de = new DirectoryEntry(result.Path);
                return de;
            }
            catch
            {
                return null;
            }
        }

        private static IdentityImpersonation impersonate = new IdentityImpersonation("Administrator", "abcd1234*", "localhost");

        ///  
        ///启用指定公共名称的用户  
        ///  
        ///  用户公共名称   
        public static void EnableUser(string commonName)
        {
            EnableUser(GetDirectoryEntry(commonName));
        }
        ///  
        ///启用指定 的用户  
        ///  
        ///  
        public static void EnableUser(DirectoryEntry de)
        {
            impersonate.BeginImpersonate();
            de.Properties["userAccountControl"][0] = ADS_USER_FLAG_ENUM.ADS_UF_NORMAL_ACCOUNT | ADS_USER_FLAG_ENUM.ADS_UF_DONT_EXPIRE_PASSWD;
            de.CommitChanges();
            impersonate.StopImpersonate();
            de.Close();
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

        /// 

        /// 根据用户帐号称取得用户的 对象

        /// 

        /// 用户帐号名

        /// 如果找到该用户，则返回用户的 对象；否则返回 null

        public static DirectoryEntry GetDirectoryEntryByAccount(string sAMAccountName)
        {

            DirectoryEntry de = GetDirectoryObject();

            DirectorySearcher deSearch = new DirectorySearcher(de);

            deSearch.Filter = "(&(&(objectCategory=person)(objectClass=user))(sAMAccountName=" + sAMAccountName + "))";

            deSearch.SearchScope = SearchScope.Subtree;



            try
            {

                SearchResult result = deSearch.FindOne();

                de = new DirectoryEntry(result.Path);

                return de;

            }

            catch
            {

                return null;

            }

        }

        public static void FindUser(string username)
        {
            System.DirectoryServices.DirectoryEntry pEntry = new System.DirectoryServices.DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
            System.DirectoryServices.DirectoryEntry ADuser = pEntry.Children.Find(username, "user");


        }
    }
}
