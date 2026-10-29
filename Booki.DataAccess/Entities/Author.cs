using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booki.DataAccess.Entities
{
    public class Author
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int? BirthYear { get; set; }

        public int? DeathYear { get; set; }

        public string? Biography { get; set; }
    }
}
