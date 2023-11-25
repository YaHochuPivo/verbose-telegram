using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeTest
{
    internal class User
    {
        public User(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public int AvgMinute { get; set; }
        public int AvgSecond { get; set; }
    }
}
