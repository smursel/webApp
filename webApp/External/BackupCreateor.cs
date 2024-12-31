using System.Diagnostics;

public class PostgreSqlBackup
{
    public static void BackupDatabase(string host, string port, string username, string password, string databaseName, string backupFilePath)
    {
        try
        {
            // Set the environment variable for PostgreSQL password
            Environment.SetEnvironmentVariable("PGPASSWORD", password);

            // Build the pg_dump command
            string arguments = $"-h {host} -p {port} -U {username} -F c -b -v -f \"{backupFilePath}\" {databaseName}";

            // Create a new process for pg_dump
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "C:\\Program Files\\PostgreSQL\\15\\bin\\pg_dump.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = processInfo })
            {
                process.Start();

                // Read and log output and errors (optional)
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Backup completed successfully!");
                }
                else
                {
                    Console.WriteLine($"Backup failed. Error: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while backing up the database: {ex.Message}");
        }
        finally
        {
            // Clear the environment variable for security
            Environment.SetEnvironmentVariable("PGPASSWORD", null);
        }
    }
}
