using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booki.DataAccess.Entities
{
    public class Book
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public int? PublishedYear { get; set; }

        public string? CoverImageUrl { get; set; }

        public int AuthorId { get; set; }

        public Author? Author { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public string? SourceName { get; set; }

        public string? SourceUrl { get; set; }

        public string? CopyrightStatus { get; set; }

        public string? Translator { get; set; }

        public string? TranslationCopyrightStatus { get; set; }
    }
}
