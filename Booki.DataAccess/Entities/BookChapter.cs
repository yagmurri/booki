using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booki.DataAccess.Entities
{
    public class BookChapter
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public Book? Book { get; set; }

        public int OrderNumber { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}