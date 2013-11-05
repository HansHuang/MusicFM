using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Model
{
    public class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        public Channel(int id = 0, string name = "", string des = "")
        {
            Id = id;
            Name = Name;
            Description = des;
        }
    }
}
