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
        public string StrId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CoverImage { get; set; }
        public string Thumb { get; set; }


        public Channel(int id = 0, string name = "", string desc = "", string coverImage = "")
        {
            Id = id;
            Name = name;
            Description = desc;
            CoverImage = coverImage;
        }

        public bool Same(Channel toCompare) 
        {
            if (Equals(toCompare)) return true;
            return toCompare != null && Id == toCompare.Id && StrId == toCompare.StrId;
        }

    }
}
