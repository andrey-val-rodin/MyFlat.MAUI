﻿using MyFlat.Maui.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;

namespace MyFlat.Maui.Services
{
    public class GlobusService
    {
        private readonly IMessenger _messenger;
        private readonly HttpClient _httpClient = new HttpClient();
        private string _phpSessionId;
        private string _bitrixSessionId;
        private decimal? _balance;

        public decimal Balance
        {
            get { return _balance ?? 0; }
        }

        public bool IsAuthorized
        {
            get
            {
                return !string.IsNullOrEmpty(_phpSessionId);
            }
        }

        public GlobusService(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public async Task<bool> AuthorizeAsync(string login, string password)
        {
            if (IsAuthorized)
                return true;

            _phpSessionId = await RetrieveSessionIdAsync();
            if (string.IsNullOrEmpty(_phpSessionId))
            {
                Reset();
                return false;
            }

            var request = CreateRequest(
                new Uri("https://lk.globusenergo.ru/ajax/auth.php"),
                $"tourl=%2Fpersonal%2Finfo%2F&AUTH_FORM=Y&TYPE=AUTH&USER_LOGIN={HttpUtility.UrlEncode(login)}&USER_PASSWORD={HttpUtility.UrlEncode(password)}",
                "https://lk.globusenergo.ru/");

            var response = await SendAsync(request);
            if (response?.StatusCode != HttpStatusCode.OK)
            {
                await ShowErrorAsync(
                    $"Глобус вернул код ошибки {response?.StatusCode.ToString()}");
                Reset();
                return false;
            }

            var html = await response?.Content?.ReadAsStringAsync();
            if (html == null || html.Contains("ERROR|Неверный логин или пароль"))
            {
                await ShowErrorAsync("Глобус: Неверный логин или пароль");
                Reset();
                return false;
            }

            if (!HtmlParser.TryGetSessionId(html, out _bitrixSessionId))
            {
                await ShowErrorAsync("Глобус: ошибка при получении Bitrix SessionId");
                Reset();
                return false;
            }

            if (!HtmlParser.TryGetBalance(html, out decimal result))
            {
                await ShowErrorAsync("Ошибка при получении баланса из Глобуса");
                Reset();
                return false;
            }
            else
                _balance = result;

            return IsAuthorized;
        }

        private async Task ShowErrorAsync(string errorMessage)
        {
            if (_messenger != null)
                await _messenger.ShowErrorAsync(errorMessage);
        }

        private async Task<string> RetrieveSessionIdAsync()
        {
            try
            {
                var uri = new Uri("https://lk.globusenergo.ru/");
                using var httpClient = new HttpClientWithCookies();
                await httpClient.GetAsync(uri);
                var cookies = httpClient.CookieContainer?.GetCookies(uri)?.Cast<Cookie>()?.ToList();
                var result = cookies?.FirstOrDefault(c => c.Name == "PHPSESSID")?.Value;
                if (string.IsNullOrEmpty(result))
                    await ShowErrorAsync("Глобус: ошибка при получении SessionId");

                return result;
            }
            catch (Exception e)
            {
                await ShowErrorAsync("Глобус: ошибка при получении SessionId: " + e.Message);
                return null;
            }
        }

        private void Reset()
        {
            _phpSessionId = null;
            _bitrixSessionId = null;
            _balance = null;
        }

        public async Task<bool> LogoffAsync()
        {
            if (!IsAuthorized)
                throw new InvalidOperationException();

            var request = CreateRequest(
                new Uri("https://lk.globusenergo.ru/personal/info/?logout=yes"),
                "",
                "https://lk.globusenergo.ru/personal/info/");

            var response = await SendAsync(request);
            if (response?.StatusCode != HttpStatusCode.OK)
            {
                await ShowErrorAsync(
                    $"Глобус вернул код ошибки {response?.StatusCode.ToString()}");
                return false;
            }

            _phpSessionId = null;
            return !IsAuthorized;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException e)
            {
                await ShowErrorAsync(e.Message);
            }

            return response;
        }

        private HttpRequestMessage CreateRequest(Uri uri, string content, string referrer)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            if (referrer == null)
                throw new ArgumentNullException(nameof(referrer));

            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = new StringContent(content)
            };

            request.Headers.Connection.Add("keep-alive");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            request.Headers.Add("Origin", "https://lk.globusenergo.ru");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.Add("Referer", referrer);
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Cookie", $"PHPSESSID={_phpSessionId}; BITRIX_SM_GUEST_ID=115872; BITRIX_SM_LAST_VISIT=02.08.2021+12%3A11%3A09; BITRIX_CONVERSION_CONTEXT_s1=%7B%22ID%22%3A8%2C%22EXPIRE%22%3A1627937940%2C%22UNIQUE%22%3A%5B%22conversion_visit_day%22%5D%7D; BX_USER_ID=02c28ee84de81190c66c50093df21db8");
            request.Headers.AcceptEncoding.Clear();

            return request;
        }

        public async Task<decimal?> GetBalanceAsync()
        {
            if (!IsAuthorized)
                throw new InvalidOperationException();

            var request = CreateRequest(
               new Uri("https://lk.globusenergo.ru/personal/info/"),
               "",
               "https://lk.globusenergo.ru/");

            var response = await SendAsync(request);
            if (response?.StatusCode != HttpStatusCode.OK)
            {
                await ShowErrorAsync(
                    $"Глобус вернул код ошибки {response?.StatusCode.ToString()}");
                return null;
            }

            var html = await response.Content?.ReadAsStringAsync();
            if (html == null || !HtmlParser.TryGetBalance(html, out decimal result))
            {
                await ShowErrorAsync("Ошибка при получении баланса из Глобуса");
                return null;
            }

            return result;
        }

        public async Task<bool> SendMetersAsync(int kitchenHotWater, int bathroomHotWater)
        {
            if (!IsAuthorized)
                throw new InvalidOperationException();

            var request = CreateRequest(
               new Uri("https://lk.globusenergo.ru/personal/meters/"),
               $"action=set_meters&sessid={_bitrixSessionId}&indiccur1%5B91649%5D={kitchenHotWater}&indiccur1%5B91650%5D={bathroomHotWater}",
               "https://lk.globusenergo.ru/personal/meters/");

            var response = await SendAsync(request);
            if (response?.StatusCode != HttpStatusCode.OK)
            {
                await ShowErrorAsync(
                    $"Глобус вернул код ошибки {response?.StatusCode.ToString()}");
                return false;
            }

            var html = await response.Content?.ReadAsStringAsync();
            if (html == null || html.Contains("errortext") ||
                Regex.Matches(html, "Показания переданы").Count == 0)
            {
                await ShowErrorAsync(
                    $"Ошибка во время передачи показаний в Глобус");
                return false;
            }

            return true;
        }
    }
}
