using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FilleDataBase
{
    public class Poem
    {
        [Key]
        public int Id { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }

        // This is the backing field that will be stored in the database
        public string LinesJson { get; set; } = "[]";

        [NotMapped] // This tells EF Core not to try to map this property directly
        public List<string> Lines
        {
            get => LinesJson == null ? new List<string>() : JsonSerializer.Deserialize<List<string>>(LinesJson);
            set => LinesJson = value == null ? "[]" : JsonSerializer.Serialize(value);
        }

        public int LinesCount { get; set; }

        // New fields for WikiData
        public string Description { get; set; }
        public string PublicationDate { get; set; }

    }
}