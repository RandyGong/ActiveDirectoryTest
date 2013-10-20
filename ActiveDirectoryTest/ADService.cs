//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.DirectoryServices;
////using Previa.Common.Interfaces;
////using Previa.Common.Models.AD;

//namespace Previa.Common.Services
//{
//    public class ADService //: IADService
//    {
//        //TODO: remove configmanager

//        private List<string> _adPaths;
//        private string _adUserName;
//        private string _adUserPassWord;

//        public ADService(List<string> adPaths, string adUserName, string adUserPassWord)
//        {
//            _adPaths = adPaths;
//            _adUserName = adUserName;
//            _adUserPassWord = adUserPassWord;
//        }

//        public List<User> GetUsers()
//        {
//            var userList = new List<User>();

//            _adPaths.ForEach(path =>
//            {
//                userList.AddRange(GetADUsers(path, _adUserName, _adUserPassWord));
//            });

//            return userList.Distinct(new CustomerComparer<User>("UserName")).ToList();
//        }

//        private List<User> GetADUsers(string AdPath, string userName, string passWord)
//        {
//            DirectorySearcher search = new DirectorySearcher(
//                new DirectoryEntry(AdPath, userName, passWord),
//                "(&(objectClass=user)(objectCategory=person)(!(userAccountControl:1.2.840.113556.1.4.803:=2))((mail=*)))", // filter out disabled accounts
//                new string[] { "givenName", "sn", "mail", "telephoneNumber", "l", "company", "memberOf", "userPrincipalName", "userAccountControl", "lastLogonTimestamp", "departmentNumber", "department", "description", "info", "mobile" }, SearchScope.OneLevel); // search only the current level

//            return search.FindAll().Cast<SearchResult>().Select(x =>
//            {
//                return new User
//                {
//                    FirstName = x.Properties.Contains("givenName") ? x.Properties["givenName"][0].ToString() : string.Empty,
//                    LastName = x.Properties.Contains("sn") ? x.Properties["sn"][0].ToString() : string.Empty,
//                    Email = x.Properties.Contains("mail") ? x.Properties["mail"][0].ToString() : string.Empty,
//                    Phone = x.Properties.Contains("telephoneNumber") ? x.Properties["telephoneNumber"][0].ToString() : string.Empty,
//                    City = x.Properties.Contains("l") ? x.Properties["l"][0].ToString() : string.Empty,
//                    Company = x.Properties.Contains("company") ? x.Properties["company"][0].ToString() : string.Empty,
//                    Groups = x.Properties.Contains("memberOf") ? parseGroups(x.Properties["memberOf"]) : new List<string>(),
//                    UserName = x.Properties.Contains("userPrincipalName") ? x.Properties["userPrincipalName"][0].ToString() : string.Empty,
//                    UserAccountControl = x.Properties.Contains("userAccountControl") ? (int)x.Properties["userAccountControl"][0] : 0,
//                    LastLogonTimestamp = x.Properties.Contains("lastLogonTimestamp") ? DateTime.FromFileTime((long)(x.Properties["lastLogonTimestamp"][0])) : DateTime.MinValue,
//                    Department = x.Properties.Contains("department") ? x.Properties["department"][0].ToString() : string.Empty,
//                    DepartmentNumber = x.Properties.Contains("departmentNumber") ? x.Properties["departmentNumber"][0].ToString() : string.Empty,
//                    DepartmentDescription = x.Properties.Contains("description") ? x.Properties["description"][0].ToString() : string.Empty,
//                    Info = x.Properties.Contains("info") ? x.Properties["info"][0].ToString() : string.Empty,
//                    Mobile = x.Properties.Contains("mobile") ? x.Properties["mobile"][0].ToString() : string.Empty
//                };
//            }).OrderBy(x => x.UserName).ToList();
//        }

//        public User GetUser(string adAccountName)
//        {
//            var userList = new List<User>();

//            _adPaths.ForEach(path =>
//            {
//                userList.AddRange(GetADUser(path, _adUserName, _adUserPassWord, adAccountName));
//            });

//            userList = userList.Distinct(new CustomerComparer<User>("UserName")).ToList();

//            // just to make sure we get a hit
//            if (userList.Count > 0)
//                return userList[0];
//            else
//                return null;
//        }

//        private List<User> GetADUser(string AdPath, string userName, string passWord, string adAccountName)
//        {
//            DirectorySearcher search = new DirectorySearcher(
//                new DirectoryEntry(AdPath, userName, passWord),
//                "(&(objectClass=user)(objectCategory=person)(sAMAccountName=" + adAccountName + ")(!(userAccountControl:1.2.840.113556.1.4.803:=2)))", // filter out disabled accounts
//                new string[] { "givenName", "sn", "mail", "telephoneNumber", "l", "company", "memberOf", "userPrincipalName", "userAccountControl", "lastLogonTimestamp", "departmentNumber", "department", "description", "info", "mobile" }, SearchScope.OneLevel); // search only the current level

//            return search.FindAll().Cast<SearchResult>().Select(x =>
//            {
//                return new User
//                {
//                    FirstName = x.Properties.Contains("givenName") ? x.Properties["givenName"][0].ToString() : string.Empty,
//                    LastName = x.Properties.Contains("sn") ? x.Properties["sn"][0].ToString() : string.Empty,
//                    Email = x.Properties.Contains("mail") ? x.Properties["mail"][0].ToString() : string.Empty,
//                    Phone = x.Properties.Contains("telephoneNumber") ? x.Properties["telephoneNumber"][0].ToString() : string.Empty,
//                    City = x.Properties.Contains("l") ? x.Properties["l"][0].ToString() : string.Empty,
//                    Company = x.Properties.Contains("company") ? x.Properties["company"][0].ToString() : string.Empty,
//                    Groups = x.Properties.Contains("memberOf") ? parseGroups(x.Properties["memberOf"]) : new List<string>(),
//                    UserName = x.Properties.Contains("userPrincipalName") ? x.Properties["userPrincipalName"][0].ToString() : string.Empty,
//                    UserAccountControl = x.Properties.Contains("userAccountControl") ? (int)x.Properties["userAccountControl"][0] : 0,
//                    LastLogonTimestamp = x.Properties.Contains("lastLogonTimestamp") ? DateTime.FromFileTime((long)(x.Properties["lastLogonTimestamp"][0])) : DateTime.MinValue,
//                    Department = x.Properties.Contains("department") ? x.Properties["department"][0].ToString() : string.Empty,
//                    DepartmentNumber = x.Properties.Contains("departmentNumber") ? x.Properties["departmentNumber"][0].ToString() : string.Empty,
//                    DepartmentDescription = x.Properties.Contains("description") ? x.Properties["description"][0].ToString() : string.Empty,
//                    Info = x.Properties.Contains("info") ? x.Properties["info"][0].ToString() : string.Empty,
//                    Mobile = x.Properties.Contains("mobile") ? x.Properties["mobile"][0].ToString() : string.Empty
//                };
//            }).ToList();
//        }

//        private List<string> parseGroups(ResultPropertyValueCollection results)
//        {
//            var groups = new List<string>();

//            for (int i = 0; i < results.Count; i++)
//            {
//                groups.Add(parseGroupName(results[i].ToString()));
//            }

//            return groups.OrderBy(x => x).ToList();
//        }

//        private string parseGroupName(string name)
//        {
//            int start = name.IndexOf("=");
//            int end = name.IndexOf(",");
//            return name.Substring(start + 1, end - start - 1);
//        }
//    }

//    public class CustomerComparer<T> : IEqualityComparer<T>
//    {
//        private Func<T, Object> getPropertyValueFunc = null;

//        /// <summary>
//        /// Get property by property name
//        /// </summary>
//        /// <param name="propertyName"></param>
//        public CustomerComparer(string propertyName)
//        {
//            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName,
//            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
//            if (propertyInfo == null)
//            {
//                throw new ArgumentException(string.Format("{0} is not a property of type {1}.",
//                    propertyName, typeof(T)));
//            }

//            ParameterExpression expPara = Expression.Parameter(typeof(T), "obj");
//            MemberExpression me = Expression.Property(expPara, propertyInfo);
//            getPropertyValueFunc = Expression.Lambda<Func<T, object>>(me, expPara).Compile();
//        }

//        public bool Equals(T x, T y)
//        {
//            object xValue = getPropertyValueFunc(x);
//            object yValue = getPropertyValueFunc(y);

//            if (xValue == null)
//                return yValue == null;
//            return xValue.Equals(yValue);
//        }

//        public int GetHashCode(T obj)
//        {
//            object propertyValue = getPropertyValueFunc(obj);

//            if (propertyValue == null)
//                return 0;
//            return propertyValue.GetHashCode();
//        }
//    }
//}
