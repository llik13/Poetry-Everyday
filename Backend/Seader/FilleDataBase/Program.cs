using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FilleDataBase
{
    class ResilientPoetryImporter
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2) // Increased timeout for large responses
        };

        private const int MAX_RETRIES = 3;
        private const int RETRY_DELAY_MS = 2000; // 2 seconds

        public static async Task<List<string>> FetchAuthors()
        {
            Console.WriteLine("Fetching authors...");

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    var response = await client.GetAsync("https://poetrydb.org/author");

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        if (attempt < MAX_RETRIES)
                        {
                            Console.WriteLine($"Retrying ({attempt}/{MAX_RETRIES})...");
                            await Task.Delay(RETRY_DELAY_MS);
                            continue;
                        }
                        return new List<string>();
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    var authorsResponse = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

                    if (authorsResponse != null && authorsResponse.ContainsKey("authors"))
                    {
                        return authorsResponse["authors"];
                    }

                    return new List<string>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching authors (attempt {attempt}/{MAX_RETRIES}): {ex.Message}");
                    if (attempt < MAX_RETRIES)
                    {
                        Console.WriteLine("Retrying...");
                        await Task.Delay(RETRY_DELAY_MS);
                    }
                    else
                    {
                        Console.WriteLine("Maximum retry attempts reached. Aborting.");
                    }
                }
            }

            return new List<string>();
        }

        public static async Task<List<Poem>> FetchPoemsByAuthor(string author)
        {
            Console.WriteLine($"Fetching poems for author: {author}");

            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    var response = await client.GetAsync($"https://poetrydb.org/author/{Uri.EscapeDataString(author)}/author,title,linecount,lines");

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error fetching poems for {author}: {response.StatusCode}");
                        if (attempt < MAX_RETRIES)
                        {
                            Console.WriteLine($"Retrying ({attempt}/{MAX_RETRIES})...");
                            await Task.Delay(RETRY_DELAY_MS);
                            continue;
                        }
                        return new List<Poem>();
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    var token = JToken.Parse(json);
                    List<Poem> poems = new List<Poem>();

                    if (token.Type == JTokenType.Array)
                    {
                        // Deserialize manually to ensure we handle null values properly
                        var poemsArray = (JArray)token;
                        foreach (var poemToken in poemsArray)
                        {
                            var poem = new Poem
                            {
                                Author = poemToken["author"]?.ToString() ?? author,
                                Title = poemToken["title"]?.ToString() ?? "Untitled",
                                LinesCount = poemToken["linecount"]?.Value<int>() ?? 0
                            };

                            // Handle lines specifically to avoid null values
                            if (poemToken["lines"] != null && poemToken["lines"].Type == JTokenType.Array)
                            {
                                poem.Lines = poemToken["lines"].ToObject<List<string>>() ?? new List<string>();
                            }
                            else
                            {
                                poem.Lines = new List<string>();
                            }

                            poems.Add(poem);
                        }
                        return poems;
                    }

                    Console.WriteLine($"Unexpected JSON format: {json}");
                    return new List<Poem>();
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Network error fetching poems for {author} (attempt {attempt}/{MAX_RETRIES}): {ex.Message}");
                    if (attempt < MAX_RETRIES)
                    {
                        Console.WriteLine($"Waiting {RETRY_DELAY_MS / 1000} seconds before retrying...");
                        await Task.Delay(RETRY_DELAY_MS * attempt); // Increasing backoff
                    }
                    else
                    {
                        Console.WriteLine("Maximum retry attempts reached. Skipping this author.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching poems for {author} (attempt {attempt}/{MAX_RETRIES}): {ex.Message}");
                    if (attempt < MAX_RETRIES)
                    {
                        Console.WriteLine("Retrying...");
                        await Task.Delay(RETRY_DELAY_MS);
                    }
                    else
                    {
                        Console.WriteLine("Maximum retry attempts reached. Skipping this author.");
                    }
                }
            }

            return new List<Poem>();
        }

        public static async Task SavePoemsToMySqlDb()
        {
            // MySQL connection string - update with your credentials
            string connectionString = "server=localhost;port=3306;database=content;user=root;password=0000;";

            try
            {
                // Create a new DbContextOptionsBuilder without any previously registered services
                var optionsBuilder = new DbContextOptionsBuilder<PoetryDbContext>();

                // Only register the Pomelo provider
                optionsBuilder.UseMySql(connectionString,
                    MySqlServerVersion.LatestSupportedServerVersion);

                // Create context with the options
                using var context = new PoetryDbContext(optionsBuilder.Options);

                // Ensure database exists with schema
                bool created = await context.Database.EnsureCreatedAsync();
                if (created)
                {
                    Console.WriteLine("Database was created successfully.");
                }
                else
                {
                    Console.WriteLine("Database already exists.");
                }

                // Get all authors
                var authors = await FetchAuthors();
                int authorCount = 0;
                int totalPoems = 0;
                int skipCount = 0; // Track number of skipped authors due to errors

                // You can limit the number of authors for testing
                // authors = authors.Take(10).ToList();

                foreach (var author in authors)
                {
                    try
                    {
                        var poems = await FetchPoemsByAuthor(author);

                        // If no poems were fetched (likely due to error), skip this author
                        if (poems.Count == 0)
                        {
                            skipCount++;
                            Console.WriteLine($"Skipping author: {author} (no poems fetched)");
                            continue;
                        }

                        // Validate poems before adding to DB
                        foreach (var poem in poems)
                        {
                            // Ensure Lines is never null
                            if (poem.Lines == null)
                            {
                                poem.Lines = new List<string>();
                            }

                            // Double-check LinesCount
                            poem.LinesCount = poem.Lines.Count;
                        }

                        // Check if any poems by this author already exist
                        var existingCount = await context.Poems.Where(p => p.Author == author).CountAsync();
                        if (existingCount > 0)
                        {
                            Console.WriteLine($"Found {existingCount} existing poems by {author}, skipping...");
                            skipCount++;
                            continue;
                        }

                        context.Poems.AddRange(poems);
                        totalPoems += poems.Count;
                        authorCount++;

                        Console.WriteLine($"Added {poems.Count} poems by {author}. Total: {totalPoems} poems from {authorCount} authors (skipped {skipCount})");

                        // Save in batches to avoid memory issues
                        if (authorCount % 5 == 0 || totalPoems > 500)
                        {
                            Console.WriteLine("Saving batch to database...");
                            await context.SaveChangesAsync();
                            totalPoems = 0;
                        }

                        // Add a delay between authors to avoid overloading the API
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing author {author}: {ex.Message}");
                        skipCount++;
                    }
                }

                // Save any remaining poems
                if (totalPoems > 0)
                {
                    Console.WriteLine("Saving final batch to database...");
                    await context.SaveChangesAsync();
                }

                Console.WriteLine($"Data import completed. Added poems from {authorCount} authors (skipped {skipCount}).");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during database operation:");
                Console.WriteLine(ex.ToString());

                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception:");
                    Console.WriteLine(ex.InnerException.ToString());
                }
            }
        }

        public static async Task Main()
        {
            try
            {
                Console.WriteLine("Starting poetry import to MySQL database...");
                await SavePoemsToMySqlDb();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Critical error during import:");
                Console.WriteLine(ex.ToString());

                // If there's an inner exception, print that too
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception:");
                    Console.WriteLine(ex.InnerException.ToString());
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}