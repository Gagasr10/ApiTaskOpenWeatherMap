using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Tests.Helpers
{
    public static class TestHelpers
    {
        public static async Task LogResponse(HttpStatusCode status, string content, string testName)
        {
            string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            string logDir = Path.Combine(root, "Logs");
            Directory.CreateDirectory(logDir);
            string logPath = Path.Combine(logDir, "log.txt");

            await File.AppendAllTextAsync(logPath,
                $"[{DateTime.Now}] Test: {testName}\nStatus: {(int)status} {status}\nResponse:\n{content}\n\n");
        }
    }
}

