using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FilleDataBase
{
    public class DBpediaService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string DBPEDIA_ENDPOINT = "https://dbpedia.org/sparql";
        private const string DEFAULT_FORMAT = "application/json";

        public async Task<(string description, string publicationDate)> GetPoemMetadata(string title, string author)
        {
            string description = null;
            string publicationDate = null;

            // Try different approaches to find metadata
            var result = await TryFindPoemByTitleAndAuthor(title, author);

            if (result.description == null && result.publicationDate == null)
            {
                // If no results, try just by title
                result = await TryFindPoemByTitleOnly(title);
            }

            if (result.description == null && result.publicationDate == null)
            {
                // Try to find author information instead
                result = await TryFindAuthorInformation(author);
            }

            return result;
        }

        private async Task<(string description, string publicationDate)> TryFindPoemByTitleAndAuthor(string title, string author)
        {
            string description = null;
            string publicationDate = null;

            // Sanitize inputs for the SPARQL query
            string sanitizedTitle = SanitizeForSparql(title);
            string sanitizedAuthor = SanitizeForSparql(author);

            Console.WriteLine($"Searching DBpedia for poem: \"{sanitizedTitle}\" by {sanitizedAuthor}");

            // Build a more flexible SPARQL query
            string query = $@"
                PREFIX dbo: <http://dbpedia.org/ontology/>
                PREFIX dbp: <http://dbpedia.org/property/>
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                PREFIX foaf: <http://xmlns.com/foaf/0.1/>
                
                SELECT DISTINCT ?abstract ?date WHERE {{
                  {{
                    # Try to match written works
                    ?poem a dbo:WrittenWork ;
                          rdfs:label ?label .
                    ?poem dbo:author ?authorRes .
                    ?authorRes rdfs:label ?authorName .
                    
                    FILTER(REGEX(LCASE(?label), LCASE('{sanitizedTitle}'), 'i'))
                    FILTER(REGEX(LCASE(?authorName), LCASE('{sanitizedAuthor}'), 'i'))
                    FILTER(LANG(?label) = 'en')
                    
                    OPTIONAL {{ ?poem dbo:abstract ?abstract . FILTER(LANG(?abstract) = 'en') }}
                    OPTIONAL {{ ?poem dbp:published ?date }}
                    OPTIONAL {{ ?poem dbo:publicationDate ?date }}
                  }}
                  UNION
                  {{
                    # Try to match literary works
                    ?poem a dbo:LiteraryWork ;
                          rdfs:label ?label .
                    ?poem dbo:author ?authorRes .
                    ?authorRes rdfs:label ?authorName .
                    
                    FILTER(REGEX(LCASE(?label), LCASE('{sanitizedTitle}'), 'i'))
                    FILTER(REGEX(LCASE(?authorName), LCASE('{sanitizedAuthor}'), 'i'))
                    FILTER(LANG(?label) = 'en')
                    
                    OPTIONAL {{ ?poem dbo:abstract ?abstract . FILTER(LANG(?abstract) = 'en') }}
                    OPTIONAL {{ ?poem dbp:published ?date }}
                    OPTIONAL {{ ?poem dbo:publicationDate ?date }}
                  }}
                  UNION
                  {{
                    # Try to match any work with title and author mention
                    ?poem rdfs:label ?label ;
                          dbo:abstract ?abstract .
                    
                    FILTER(REGEX(LCASE(?label), LCASE('{sanitizedTitle}'), 'i'))
                    FILTER(REGEX(LCASE(?abstract), LCASE('{sanitizedAuthor}'), 'i'))
                    FILTER(LANG(?abstract) = 'en')
                    
                    OPTIONAL {{ ?poem dbp:published ?date }}
                    OPTIONAL {{ ?poem dbo:publicationDate ?date }}
                  }}
                }}
                LIMIT 1
            ";

            return await ExecuteQuery(query);
        }

        private async Task<(string description, string publicationDate)> TryFindPoemByTitleOnly(string title)
        {
            string sanitizedTitle = SanitizeForSparql(title);
            Console.WriteLine($"Searching DBpedia for poem with title: \"{sanitizedTitle}\"");

            string query = $@"
                PREFIX dbo: <http://dbpedia.org/ontology/>
                PREFIX dbp: <http://dbpedia.org/property/>
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                
                SELECT DISTINCT ?abstract ?date WHERE {{
                  {{
                    # Try to match by exact title of any creative work
                    ?poem a dbo:Work ;
                          rdfs:label ?label .
                    
                    FILTER(LCASE(?label) = LCASE('{sanitizedTitle}'))
                    FILTER(LANG(?label) = 'en')
                    
                    OPTIONAL {{ ?poem dbo:abstract ?abstract . FILTER(LANG(?abstract) = 'en') }}
                    OPTIONAL {{ ?poem dbp:published ?date }}
                    OPTIONAL {{ ?poem dbo:publicationDate ?date }}
                  }}
                  UNION
                  {{
                    # Try to match by partial title match
                    ?poem rdfs:label ?label .
                    
                    FILTER(REGEX(LCASE(?label), CONCAT('^', LCASE('{sanitizedTitle}'), '( |$)'), 'i'))
                    FILTER(LANG(?label) = 'en')
                    
                    OPTIONAL {{ ?poem dbo:abstract ?abstract . FILTER(LANG(?abstract) = 'en') }}
                    OPTIONAL {{ ?poem dbp:published ?date }}
                    OPTIONAL {{ ?poem dbo:publicationDate ?date }}
                  }}
                }}
                LIMIT 1
            ";

            return await ExecuteQuery(query);
        }

        private async Task<(string description, string publicationDate)> TryFindAuthorInformation(string author)
        {
            string sanitizedAuthor = SanitizeForSparql(author);
            Console.WriteLine($"Searching DBpedia for author: \"{sanitizedAuthor}\"");

            string query = $@"
                PREFIX dbo: <http://dbpedia.org/ontology/>
                PREFIX dbp: <http://dbpedia.org/property/>
                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                
                SELECT DISTINCT ?abstract ?birthDate WHERE {{
                  ?author a dbo:Person ;
                          rdfs:label ?name ;
                          dbo:abstract ?abstract .
                  
                  FILTER(REGEX(LCASE(?name), LCASE('{sanitizedAuthor}'), 'i'))
                  FILTER(LANG(?abstract) = 'en')
                  
                  OPTIONAL {{ ?author dbo:birthDate ?birthDate }}
                }}
                LIMIT 1
            ";

            var (authorBio, authorBirthDate) = await ExecuteQuery(query);

            // If we find author info, format it as poem metadata
            if (!string.IsNullOrEmpty(authorBio))
            {
                return (
                    $"No specific poem information found. About the author: {authorBio}",
                    !string.IsNullOrEmpty(authorBirthDate) ? $"Author born: {authorBirthDate}" : null
                );
            }

            return (null, null);
        }

        private async Task<(string description, string publicationDate)> ExecuteQuery(string query)
        {
            string description = null;
            string publicationDate = null;

            try
            {
                var encodedQuery = Uri.EscapeDataString(query);
                var requestUrl = $"{DBPEDIA_ENDPOINT}?query={encodedQuery}&format={DEFAULT_FORMAT}";

                var response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Received response from DBpedia");

                    using (JsonDocument document = JsonDocument.Parse(json))
                    {
                        var root = document.RootElement;
                        var bindings = root.GetProperty("results").GetProperty("bindings");

                        if (bindings.GetArrayLength() > 0)
                        {
                            var binding = bindings[0];

                            // Extract description (abstract)
                            if (binding.TryGetProperty("abstract", out var abstractElem))
                            {
                                description = abstractElem.GetProperty("value").GetString();
                                Console.WriteLine("Found description in DBpedia");
                            }

                            // Extract publication date
                            if (binding.TryGetProperty("date", out var dateElem) ||
                                binding.TryGetProperty("birthDate", out dateElem))
                            {
                                publicationDate = dateElem.GetProperty("value").GetString();
                                Console.WriteLine("Found date in DBpedia");

                                // Parse date if needed
                                if (publicationDate != null && DateTime.TryParse(publicationDate, out DateTime date))
                                {
                                    publicationDate = date.ToString("yyyy-MM-dd");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No results found in DBpedia response");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"DBpedia query failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying DBpedia: {ex.Message}");
            }

            return (description, publicationDate);
        }

        private string SanitizeForSparql(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Replace quotes and escape special characters
            string sanitized = input
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                .Replace("\\", "\\\\");

            // Further cleanup for better matching
            sanitized = sanitized
                .Replace(".", " ")  // Replace periods with spaces
                .Replace(",", " ")  // Replace commas with spaces
                .Replace(";", " ")  // Replace semicolons with spaces
                .Replace(":", " ")  // Replace colons with spaces
                .Trim();            // Remove leading/trailing whitespace

            return sanitized;
        }
    }
}