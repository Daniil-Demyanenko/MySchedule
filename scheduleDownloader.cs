using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;

namespace job_checker
{
    public static class ScheduleDownloader
    {
        private static readonly HttpClient _client = new HttpClient();
        private static string _cdir = AppDomain.CurrentDomain.BaseDirectory + "/Cache"; //Путь к папке Cache в директории программы
        public static void CheckUpdate()
        {
            if (!CacheIsRelevant()) Download();
        }

        /// <summary>
        /// Проверяем релевантность кэша, путём проверки существования папки Cache, в которой он должен храниться, 
        /// наличия файлов в этой папке, 
        /// и последнего времени записи файлов расписания
        /// </summary>
    
        private static bool CacheIsRelevant()
        {

            if (!Directory.Exists(_cdir))
            {
                Directory.CreateDirectory(_cdir);
                return false;
            }

            string[] dirFiles = Directory.GetFileSystemEntries(_cdir);

            if (dirFiles.Length == 0)
            {
                return false;
            }

            // var fileWriteDate = File.GetLastWriteTime(_cdir + "/ras.xls");
            // if ((DateTime.Now - fileWriteDate).TotalHours > 4) return false;

            return true;

        }
        private static void Download()
        {
            (string name, string link) = GetDirectLinkAsync(ParseLinkFromPage()).Result;
            Console.WriteLine($"{name}\n{link}");
            string extension = name.Split('.')[^1]; // получаем расширение файла (xls или xlsx)
            string filePath = _cdir + "/ИФМОИОТ_ОФО_БАК." + extension; // генерируем имя скачанного файла
            DownloadFromDirectLinkAsync(link, filePath).Wait(); // ждём окончания загрузки
            Console.WriteLine($"Файл успешно скачан по пути {filePath}");

            Console.ReadKey();
        }
        private static string ParseLinkFromPage()
        {
            var link = LoadPage(@"https://lgpu.org/elektronnyy-resurs-distancionnogo-obucheniya-ifmit.html");
            var document = new HtmlDocument();
            document.LoadHtml(link);

            HtmlNodeCollection xpathLink = document.DocumentNode.SelectNodes("//tr[3]/td[2]/p/a");
            var diskLink = "";
            diskLink = xpathLink[0].GetAttributeValue("href", "");

            return diskLink;
        }

        private static string LoadPage(string url) //Тут происходит какая то чёрная магия, в которой я не разобрался. Оно работает - и хуй с ним
        {
            var result = "";
            var request = (HttpWebRequest)WebRequest.Create(url);
            using var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var receiveStream = response.GetResponseStream();
                if (receiveStream != null)
                {
                    StreamReader readStream;
                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    result = readStream.ReadToEnd();
                    readStream.Close();
                }
                response.Close();
            }
            return result;
        }

        static async Task<(string, string)> GetDirectLinkAsync(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://cloud-api.yandex.net/v1/disk/public/resources?public_key={url}");
            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return (result.name, result.file);
        }
        private static async Task DownloadFromDirectLinkAsync(string url, string path)
        {
            using WebClient webClient = new WebClient();
            await webClient.DownloadFileTaskAsync(new Uri(url), path); // если директория не существует то ее надо создать заранее
        }
    }
}