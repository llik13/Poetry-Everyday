using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilleDataBase
{
    class PoemDataExporter
    {
        public static async Task ExportToCSV(string outputPath)
        {
            Console.WriteLine("Starting poem data export to CSV...");

            // MySQL connection string - update with your credentials
            string connectionString = "server=localhost;port=3306;database=content;user=root;password=0000;";

            try
            {
                // Create options builder and context
                var optionsBuilder = new DbContextOptionsBuilder<PoetryDbContext>();
                optionsBuilder.UseMySql(connectionString, MySqlServerVersion.LatestSupportedServerVersion);

                using var context = new PoetryDbContext(optionsBuilder.Options);

                // Check if we can connect to the database
                if (!await context.Database.CanConnectAsync())
                {
                    Console.WriteLine("Error: Cannot connect to the database. Please check your connection string.");
                    return;
                }

                // Check if there are any poems
                int totalPoems = await context.Poems.CountAsync();
                if (totalPoems == 0)
                {
                    Console.WriteLine("No poems found in the database.");
                    return;
                }

                Console.WriteLine($"Found {totalPoems} poems in the database. Beginning export...");

                // Create CSV file
                using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
                {
                    // Write header
                    writer.WriteLine("Id,Author,Title,LinesCount,Description,PublicationDate");

                    // Process poems in batches to avoid memory issues
                    int batchSize = 100;
                    int processedCount = 0;

                    for (int skip = 0; skip < totalPoems; skip += batchSize)
                    {
                        var poems = await context.Poems
                            .OrderBy(p => p.Id)
                            .Skip(skip)
                            .Take(batchSize)
                            .ToListAsync();

                        foreach (var poem in poems)
                        {
                            // Escape and sanitize CSV values
                            string escapedTitle = EscapeCsvField(poem.Title);
                            string escapedAuthor = EscapeCsvField(poem.Author);
                            string escapedDescription = EscapeCsvField(poem.Description ?? "");
                            string escapedPublicationDate = EscapeCsvField(poem.PublicationDate ?? "");

                            // Write poem data to CSV
                            writer.WriteLine($"{poem.Id},{escapedAuthor},{escapedTitle},{poem.LinesCount},{escapedDescription},{escapedPublicationDate}");

                            processedCount++;
                        }

                        Console.WriteLine($"Exported {processedCount}/{totalPoems} poems...");
                    }
                }

                Console.WriteLine($"Export completed successfully. CSV file saved to: {Path.GetFullPath(outputPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting data: {ex.Message}");
                Console.WriteLine(ex.ToString());

                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception:");
                    Console.WriteLine(ex.InnerException.ToString());
                }
            }
        }

        // Helper method to properly escape CSV fields
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // Check if the field contains special characters
            bool needsEscaping = field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r');

            if (needsEscaping)
            {
                // Replace double quotes with two double quotes and wrap in quotes
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        public static async Task ExportFullPoems(string outputPath)
        {
            Console.WriteLine("Starting full poem content export to CSV...");

            // MySQL connection string - update with your credentials
            string connectionString = "server=localhost;port=3306;database=content;user=root;password=0000;";

            try
            {
                // Create options builder and context
                var optionsBuilder = new DbContextOptionsBuilder<PoetryDbContext>();
                optionsBuilder.UseMySql(connectionString, MySqlServerVersion.LatestSupportedServerVersion);

                using var context = new PoetryDbContext(optionsBuilder.Options);

                // Check if there are any poems
                int totalPoems = await context.Poems.CountAsync();
                if (totalPoems == 0)
                {
                    Console.WriteLine("No poems found in the database.");
                    return;
                }

                // Create CSV file
                using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
                {
                    // Write header
                    writer.WriteLine("Id,Author,Title,Content,Description,PublicationDate");

                    // Process poems in batches to avoid memory issues
                    int batchSize = 50;
                    int processedCount = 0;

                    for (int skip = 0; skip < totalPoems; skip += batchSize)
                    {
                        var poems = await context.Poems
                            .OrderBy(p => p.Id)
                            .Skip(skip)
                            .Take(batchSize)
                            .ToListAsync();

                        foreach (var poem in poems)
                        {
                            // Get poem content by joining all lines
                            string content = string.Join("\n", poem.Lines);

                            // Escape and sanitize CSV values
                            string escapedTitle = EscapeCsvField(poem.Title);
                            string escapedAuthor = EscapeCsvField(poem.Author);
                            string escapedContent = EscapeCsvField(content);
                            string escapedDescription = EscapeCsvField(poem.Description ?? "");
                            string escapedPublicationDate = EscapeCsvField(poem.PublicationDate ?? "");

                            // Write poem data to CSV
                            writer.WriteLine($"{poem.Id},{escapedAuthor},{escapedTitle},{escapedContent},{escapedDescription},{escapedPublicationDate}");

                            processedCount++;
                        }

                        Console.WriteLine($"Exported {processedCount}/{totalPoems} poems with full content...");
                    }
                }

                Console.WriteLine($"Full poem export completed successfully. CSV file saved to: {Path.GetFullPath(outputPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting data: {ex.Message}");
                Console.WriteLine(ex.ToString());
            }
        }

        public static async Task Main()
        {
            try
            {
                Console.WriteLine("Poetry Database to CSV Exporter");
                Console.WriteLine("-------------------------------");
                Console.WriteLine("1. Export basic poem information (no poem content)");
                Console.WriteLine("2. Export full poems with content");
                Console.Write("Enter your choice (1 or 2): ");

                string choice = Console.ReadLine();

                Console.Write("Enter output CSV file path (default is 'poems.csv'): ");
                string outputPath = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    outputPath = "poems.csv";
                }

                if (choice == "1")
                {
                    await ExportToCSV(outputPath);
                }
                else if (choice == "2")
                {
                    await ExportFullPoems(outputPath);
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please enter 1 or 2.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}