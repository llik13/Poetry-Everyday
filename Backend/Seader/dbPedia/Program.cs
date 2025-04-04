using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FilleDataBase
{
    class DBpediaEnricher
    {
        private static readonly DBpediaService dbpediaService = new DBpediaService();

        public static async Task EnrichExistingDatabase()
        {
            Console.WriteLine("Starting DBpedia enrichment of existing poetry database...");

            // MySQL connection string - update this with your credentials
            string connectionString = "server=localhost;port=3306;database=content;user=root;password=0000;";

            try
            {
                // Create options with MySQL connection
                var optionsBuilder = new DbContextOptionsBuilder<PoetryDbContext>()
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

                // Create context with the options
                using var context = new PoetryDbContext(optionsBuilder.Options);

                // Verify the database connection
                try
                {
                    // Check if we can access the Poems table
                    int totalPoems = await context.Poems.CountAsync();
                    Console.WriteLine($"Found {totalPoems} poems in database to enrich");

                    int enrichedCount = 0;
                    int processedCount = 0;
                    int batchSize = 50;

                    // Process poems in batches to avoid loading everything into memory
                    for (int skip = 0; skip < totalPoems; skip += batchSize)
                    {
                        // Get a batch of poems that don't have description or publication date
                        var poemBatch = await context.Poems
                            .Where(p => string.IsNullOrEmpty(p.Description) || string.IsNullOrEmpty(p.PublicationDate))
                            .Skip(skip)
                            .Take(batchSize)
                            .ToListAsync();

                        if (!poemBatch.Any())
                            break;

                        foreach (var poem in poemBatch)
                        {
                            try
                            {
                                Console.WriteLine($"\nProcessing poem: \"{poem.Title}\" by {poem.Author}");
                                var (description, publicationDate) = await dbpediaService.GetPoemMetadata(poem.Title, poem.Author);

                                bool hasChanges = false;

                                // Only update if we got data and it's not already set
                                if (!string.IsNullOrEmpty(description) && string.IsNullOrEmpty(poem.Description))
                                {
                                    Console.WriteLine($"Adding description: {description.Substring(0, Math.Min(50, description.Length))}...");
                                    poem.Description = description;
                                    hasChanges = true;
                                }

                                if (!string.IsNullOrEmpty(publicationDate) && string.IsNullOrEmpty(poem.PublicationDate))
                                {
                                    Console.WriteLine($"Adding publication date: {publicationDate}");
                                    poem.PublicationDate = publicationDate;
                                    hasChanges = true;
                                }

                                if (hasChanges)
                                {
                                    enrichedCount++;
                                    Console.WriteLine($"Successfully enriched poem \"{poem.Title}\"");
                                }
                                else
                                {
                                    Console.WriteLine($"No metadata found for poem \"{poem.Title}\"");
                                }

                                // Add a small delay to avoid overwhelming the DBpedia endpoint
                                await Task.Delay(500);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error enriching poem \"{poem.Title}\" by {poem.Author}: {ex.Message}");
                            }

                            processedCount++;

                            // Show progress every 10 poems
                            if (processedCount % 10 == 0 || processedCount == totalPoems)
                            {
                                Console.WriteLine($"Progress: {processedCount}/{totalPoems} poems processed, {enrichedCount} enriched with new data");
                            }
                        }

                        // Save changes for this batch
                        await context.SaveChangesAsync();
                        Console.WriteLine($"Saved batch of {poemBatch.Count} poems");
                    }

                    Console.WriteLine($"Enrichment complete! Added metadata to {enrichedCount} poems out of {totalPoems} total.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database error: {ex.Message}");
                    Console.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner error: {ex.InnerException.Message}");
                }
            }
        }

        public static async Task Main()
        {
            try
            {
                await EnrichExistingDatabase();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during database enrichment:");
                Console.WriteLine(ex.ToString());

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