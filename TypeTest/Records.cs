using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeTest
{
    internal static class Records
    {
        static Records()
        {
            _users = new List<User>();
        }

        public static string[] Users => _users.OrderByDescending(user => user.AvgMinute).Select(user => user.Name).ToArray();

        public static bool Load(string path)
        {
            _users.Clear();

            try
            {
                string content = File.ReadAllText(path);
                if (content.IndexOf('{') < 0 || content.LastIndexOf('}') < 0)
                    return false; 

                

                
                content = content.Remove(content.IndexOf('{'), 1);
                content = content.Remove(content.LastIndexOf('}'), 1);
                
                content = content.Replace("\n", string.Empty);
                content = content.Replace("\t", string.Empty);
                content = content.Replace(" ", string.Empty);

                bool gripQuote = false; 
                bool gripBraket = false; 

                string userName = string.Empty;
                int avgMinute = 0;
                int avgSecond = 0;

                string propBuffer = string.Empty;
                foreach (char ch in content)
                {
                    if (ch == '"')
                    {
                        if (gripQuote)
                        {
                            gripQuote = false;
                            
                            if (!gripBraket)
                                userName = propBuffer;
                        }
                        else
                        {
                            gripQuote = true;
                            propBuffer = string.Empty;
                        }
                    }
                    else if (ch == ',' && gripBraket)
                    {
                        
                        avgMinute = Int32.Parse(propBuffer.Split(':')[1].Trim());
                    }
                    else if (ch == '{')
                    {
                        gripBraket = true;
                    }
                    else if (ch == '}')
                    {
                        
                        avgSecond = Int32.Parse(propBuffer.Split(':')[1].Trim());

                        gripBraket = false;
                        User user = new(userName);
                        user.AvgMinute = avgMinute;
                        user.AvgSecond = avgSecond;
                        _users.Add(user);
                    }
                    else
                    {
                        propBuffer += ch;
                    }
                }


                return true;
            }
            catch
            {
                _users.Clear();
                return false;
            }
        }

        public static bool Dump(string path)
        {
            if (_users.Count == 0)
                return false;

           
            StringBuilder jsonStr = new();

            jsonStr.Append("{");
            foreach (User user in _users)
            {
                jsonStr.Append($"\n\t\"{user.Name}\": ");
                jsonStr.Append("{\n");
                jsonStr.Append($"\t\t \"avg minute\": {user.AvgMinute},\n");
                jsonStr.Append($"\t\t \"avg second\": {user.AvgSecond}\n");
                jsonStr.Append("\t},");

            }
            if (jsonStr[jsonStr.Length - 1] == ',')
                jsonStr.Remove(jsonStr.Length - 1, 1);
            jsonStr.Append("\n}");

            try
            {
                File.WriteAllText(path, jsonStr.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool GetUserData(string userName, out int avgMinute, out int avgSecond)
        {
            avgMinute = avgSecond = 0; 

            IEnumerable<User> users = _users.Where(user => user.Name == userName);
            if (users.Count() > 1)
                return false;

            User? user = users.FirstOrDefault();
            if (user is null)
                return false;

            avgMinute = user.AvgMinute;
            avgSecond = user.AvgSecond;
            return true;
        }

        public static bool AddUser(string name)
        {
            if (_users.Any(user => user.Name == name))
                return false;

            _users.Add(new User(name));
            return true;
        }

        public static bool SetRecord(string userName, int avgMinute, int avgSecond)
        {
            IEnumerable<User> users = _users.Where(user => user.Name == userName);
            if (users.Count() > 1)
                return false; 

            User? user = users.FirstOrDefault();
            if (user is null)
                return false; 

            user.AvgMinute = avgMinute;
            user.AvgSecond = avgSecond;
            return true;
        }

        private static List<User> _users;
    }
}
