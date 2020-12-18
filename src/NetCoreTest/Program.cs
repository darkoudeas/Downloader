using Downloader;
using Konsole;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NetCoreTest
{
    public class Program
    {
        static ProgressBar pb = new ProgressBar(100);
        static ProgressBar pbchuck = new ProgressBar(100);
        static async Task Main(string[] args)
        {

            var downloadOpt = new DownloadConfiguration
            {
                AllowedHeadRequest = true, // Can fetch file size by HEAD request or must be used GET method to support host
                ParallelDownload = false, // download parts of file as parallel or not
                BufferBlockSize = 10000, // usually, hosts support max to 8000 bytes
                ChunkCount = 0, // 0 for AutoChunk
                MaxTryAgainOnFailover = 5, // the maximum number of times to fail.
                OnTheFlyDownload = false, // caching in-memory or not?
                Timeout = 10000, // timeout (millisecond) per stream block reader
                MaximumBytesPerSecond =0, // speed limited to 1MB/s
                TempDirectory = "C:\\temp", // Set the temp path for buffering chunk files, the default path is Path.GetTempPath().
                ClearPackageAfterDownloadCompleted = true,
                RequestConfiguration = // config and customize request headers
                {
                    Accept = "*/*",
                    UserAgent = $"Downloader",
                    ProtocolVersion = HttpVersion.Version11,
                    KeepAlive = true,
                    UseDefaultCredentials = false
                    
                }
            };

            

            var ds = new DownloadService(downloadOpt);
            ds.ChunkDownloadProgressChanged += OnChunkDownloadProgressChanged;
            ds.DownloadProgressChanged += OnDownloadProgressChanged;
            ds.DownloadFileCompleted += OnDownloadFileCompleted;

            try
            {
                Stopwatch stopwatch = new Stopwatch();                
                stopwatch.Start();
                await ds.DownloadFileAsync("http://mirrors.standaloneinstaller.com/video-sample/metaxas-keller-Bell.mpeg", new DirectoryInfo("c:\\temp")).ConfigureAwait(false);
                stopwatch.Stop();                
                Console.WriteLine("Time elapsed: {0} seconds", stopwatch.Elapsed.TotalSeconds);
                
            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {                                
                Console.WriteLine("404: File not found");
            }
            catch (WebException ew)
            {
                Console.WriteLine($"Web Exception {ew.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"General Exception {e.Message}");
            }
            Console.ReadLine();
        }

        public static void OnChunkDownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            Program.pbchuck.Refresh(System.Convert.ToInt32(e.ProgressPercentage), $"Downloading chuck {e.ProgressId}");
            //throw new NotImplementedException();
        }

        public static void OnDownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            Program.pb.Refresh(System.Convert.ToInt32(e.ProgressPercentage), "Downloading");            
        }

        private static void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine("Donwload Completed");
        }
    }
}
