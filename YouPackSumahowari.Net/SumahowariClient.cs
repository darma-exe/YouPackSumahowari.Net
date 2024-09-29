using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PuppeteerSharp;
using YouPackSumahowari.Net.Extensions;
using YouPackSumahowari.Net.Helpers;
using YouPackSumahowari.Net.Models;

namespace YouPackSumahowari.Net
{
    public class SumahowariClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private string? _loginToken;
        private bool _disposed;
        
        public int ApiVersion { get; set; } = 35;

        public SumahowariClient()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            };
            var baseUrl = "https://smawari.post.japanpost.jp/epsilon_jp_web_api/v1/";
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl)
            };

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ios");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("ja"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
            _httpClient.DefaultRequestHeaders.Add("accept-charset", "UTF-8");
            _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        }

        /// <summary>
        /// Logs in with the specified email address and password to retrieve an authentication token.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public async Task LoginAsync(string email, string password)
        {
            var hashedPassword = Hash(password);

            var requestUri = $"LoginAuth?version={ApiVersion}";

            var payload = new
            {
                mailAddress = email,
                password = hashedPassword
            };

            var jsonBody = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            try
            {
                var response = await _httpClient.SendAsync(request);

                var responseContent = await response.Content.ReadAsStringAsync();

                var jsonElement = JsonDocument.Parse(responseContent).RootElement;

                if (jsonElement.TryGetProperty("status", out var statusElement) &&
                    statusElement.GetString() == "SUCCESS" &&
                    jsonElement.TryGetProperty("result", out var resultElement) &&
                    resultElement.TryGetProperty("token", out var tokenElement))
                {
                    _loginToken = tokenElement.GetString();
                    return;
                }

                throw new Exception("Login failed. Unable to retrieve token.");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Searches for a post office by the specified post office name.
        /// </summary>
        /// <param name="name">The name of the post office.</param>
        /// <returns></returns>
        public async Task<PostOffice> FindPostOfficeAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            await PuppeteerHelper.DownloadIfNeededAsync();

            await using var browser = await Puppeteer.LaunchAsync(new() { Headless = true });
            await using var page = await browser.NewPageAsync();

            var postOfficeListUrl = await FindPostOfficeListUrlAsync(page, name);

            var postOfficeUrl = await FindPostOfficeUrlAsync(page, postOfficeListUrl);

            await page.GoToAsync(postOfficeUrl);
            return await ExtractPostOfficeInfoAsync(page);

            // Retrieve the URL of the post office list page from the search results page
            async Task<string> FindPostOfficeListUrlAsync(IPage p, string postOfficeName)
            {
                var url = $"https://www.e-map.ne.jp/smt/jppost17/search_fw.htm?enc=UTF8&cond5=NOT+COL_07%3A1++AND+COL_41%3A0&p_s1=&p_s2=jp_016&p_s3=https%3A%2F%2Fsmawari.post.japanpost.jp%2Fepsilon_jp_web_api%2Fv1%2FCallbackASP&p_f3=1&keyword={postOfficeName}&cond1=COL_02%3A1001&cond6=COL_02%3A1001&cond10=COL_02%3A1100&cond11=COL_02%3A1100";
                await p.GoToAsync(url);
                
                var postOfficeListUrlSelector = "#ZdcEmapSearchFWList3 a";
                await p.WaitForSelectorAsync(postOfficeListUrlSelector);
                var postOfficeListUrlPath =
                    await (await p.QuerySelectorAsync(postOfficeListUrlSelector))?.GetAttributeAsync("href");
                if (string.IsNullOrWhiteSpace(postOfficeListUrlPath))
                    throw new Exception("Failed to retrieve post office list URL.");
                
                return "https://www.e-map.ne.jp/smt/jppost17/" + postOfficeListUrlPath;
            }

            // Retrieve the URL of the post office from list page
            async Task<string> FindPostOfficeUrlAsync(IPage p, string postOfficeListUrl)
            {
                await p.GoToAsync(postOfficeListUrl);
                
                var postOfficeListSelector = "div.z_litem";
                await p.WaitForSelectorAsync(postOfficeListSelector);
                var postOfficeListItems = await p.QuerySelectorAllAsync(postOfficeListSelector);
                foreach (var item in postOfficeListItems)
                {
                    var nameElement = await item.QuerySelectorAsync("a > span.z_litem_name");
                    var postOfficeName = nameElement != null ? await nameElement.GetTextContentAsync() : string.Empty;
                    if (postOfficeName.Contains(name))
                    {
                        var postOfficeLink = await item.QuerySelectorAsync("a");
                        var postOfficeUrlPath =
                            postOfficeLink != null ? await postOfficeLink.GetAttributeAsync("href") : null;
                        if (!string.IsNullOrWhiteSpace(postOfficeUrlPath))
                        {
                            return postOfficeUrlPath;
                        }
                    }
                }
                
                throw new Exception($"Failed to find post office with name '{name}'.");
            }

            async Task<PostOffice> ExtractPostOfficeInfoAsync(IPage p)
            {
                var baseID = await ((await p.QuerySelectorAsync("input[name='name13']"))?.GetAttributeAsync("value")
                                    ?? Task.FromResult(string.Empty));
                var postalCode = await ((await p.QuerySelectorAsync("input[name='name6']"))?.GetAttributeAsync("value")
                                        ?? Task.FromResult(string.Empty));
                var address = await ((await p.QuerySelectorAsync("input[name='name7']"))?.GetAttributeAsync("value")
                                     ?? Task.FromResult(string.Empty));
                var phoneNumber = await ((await p.QuerySelectorAsync("input[name='name8']"))?.GetAttributeAsync("value")
                                         ?? Task.FromResult(string.Empty));
                var postOfficeName = await ((await p.QuerySelectorAsync("input[name='name3']"))?.GetAttributeAsync("value")
                                            ?? Task.FromResult(string.Empty));
                var divCode = await ((await p.QuerySelectorAsync("input[name='name2']"))?.GetAttributeAsync("value")
                                     ?? Task.FromResult(string.Empty)); 

                return new PostOffice
                {
                    BaseID = baseID,
                    PostalCode = postalCode,
                    Address = address,
                    PhoneNumber = phoneNumber,
                    Name = postOfficeName,
                    DivCode = divCode,
                    DivName = string.Empty,
                    StoreCode = string.Empty
                };
            }
        }

        /// <summary>
        /// Registers the specified label information.
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public async Task<(string result, string? labelHeaderID, string? errorCode, string? errorMessage)> RegisterLabelAsync(Label label)
        {
            if (string.IsNullOrWhiteSpace(_loginToken))
                throw new InvalidOperationException("Not logged in. Please call LoginAsync first.");

            var endpoint = "RegisterLabel";
            var query = $"version={ApiVersion}&token={_loginToken}";
            var url = $"{endpoint}?{query}";

            var jsonBody = JsonSerializer.Serialize(label, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            try
            {
                var response = await _httpClient.SendAsync(request);

                var responseContent = await response.Content.ReadAsStringAsync();
                // {"status":"SUCCESS","result":{"labelHeaderId":"12345678"}}
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var jsonObject = JsonSerializer.Deserialize<JsonElement>(responseContent, options);
                
                var status = jsonObject.GetProperty("status").GetString() ?? string.Empty;
                if (status == "SUCCESS")
                {
                    var labelHeaderID = jsonObject.GetProperty("result").GetProperty("labelHeaderId").GetString() ?? string.Empty;
                    return (status, labelHeaderID, null, null);
                }
                else
                {
                    var errorCode = jsonObject.GetProperty("errorCode").GetString() ?? string.Empty;
                    var errorMessage = jsonObject.GetProperty("errorMessage").GetProperty("defaultMessage").GetString() ?? string.Empty;
                    
                    return (status, null, errorCode, errorMessage);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Processes the payment for a registered label.
        /// </summary>
        /// <param name="labelHeaderID"></param>
        /// <param name="pinCode">The PIN code set for payment in the mobile app.</param>
        /// <returns></returns>
        public async Task<(string status, string? errorCode, string? errorMessage)> RegisterLabelPaymentAsync(string labelHeaderID, string pinCode)
        {
            if (string.IsNullOrWhiteSpace(_loginToken))
                throw new InvalidOperationException("Not logged in. Please call LoginAsync first.");

            var endpoint = "RequestLabel";
            var query = $"version={ApiVersion}&token={_loginToken}";
            var url = $"{endpoint}?{query}";

            var body = new
            {
                labelHeaderId = labelHeaderID,
                pin = pinCode,
                payFlag = 0
            };
            var jsonBody = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            try
            {
                var response = await _httpClient.SendAsync(request);

                var responseContent = await response.Content.ReadAsStringAsync();
                // {"status":"SUCCESS"}
                // {"status":"FAIL","errorCode":"W101928","errorMessage":{"messageCode":"MW0073","defaultMessage":"入力されたご依頼主の郵便番号と住所が一致しません。入力内容を見直してください。"}}

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var jsonObject = JsonSerializer.Deserialize<JsonElement>(responseContent, options);

                var status = jsonObject.GetProperty("status").GetString() ?? string.Empty;
                if (status == "SUCCESS")
                {
                    return (status, null, null);
                }
                else
                {
                    var errorCode = jsonObject.GetProperty("errorCode").GetString() ?? string.Empty;
                    var errorMessage = jsonObject.GetProperty("errorMessage").GetProperty("defaultMessage").GetString() ?? string.Empty;
                    
                    return (status, errorCode, errorMessage);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Retrieves the QR code for the specified shipping label.
        /// </summary>
        /// <param name="labelHeaderID"></param>
        /// <returns>A Base64-encoded QR code image.</returns>
        public async Task<(QRCode? qr, string? errorCode, string? errorMessage)> GetLabelQRAsync(string labelHeaderID)
        {
            if (string.IsNullOrWhiteSpace(_loginToken))
                throw new InvalidOperationException("Not logged in. Please call LoginAsync first.");
            
            var endpoint = "PrintLabel";
            var query = $"version={ApiVersion}&labelHeaderId={labelHeaderID}&div=POST&token={_loginToken}";
            var url = $"{endpoint}?{query}";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Content = new StringContent(string.Empty),
            };
            
            try
            {
                var response = await _httpClient.SendAsync(request);

                var responseContent = await response.Content.ReadAsStringAsync();
                // {"status":"FAIL","errorCode":"W100003","errorMessage":{"messageCode":"ME0001","defaultMessage":"サーバーと通信ができませんでした。時間をおいて再度お試しください。"}}
                // {"status":"SUCCESS","result":{"jpTrackingNo":"123456789012","authenticationKey":"1234","data":"data:image/png;base64,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","limit":"2024/09/28 11:59"}}
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var jsonObject = JsonSerializer.Deserialize<JsonElement>(responseContent, options);
                
                var status = jsonObject.GetProperty("status").GetString() ?? string.Empty;
                if (status == "SUCCESS")
                {
                    var resultElement = jsonObject.GetProperty("result");
                    var trackingNumber = resultElement.GetProperty("jpTrackingNo").GetString() ?? string.Empty;
                    var authCode = resultElement.GetProperty("authenticationKey").GetString() ?? string.Empty;
                    var base64QR = resultElement.GetProperty("data").GetString() ?? string.Empty;
                    var limitString = resultElement.GetProperty("limit").GetString();
                    var limit = DateTime.ParseExact(limitString, "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
                    
                    return (new QRCode
                    {
                        TrackingNumber = trackingNumber,
                        AuthCode = authCode,
                        QRCodeBase64 = base64QR,
                        Limit = limit
                    }, null, null);
                }
                else
                {
                    var errorCode = jsonObject.GetProperty("errorCode").GetString() ?? string.Empty;
                    var errorMessage = jsonObject.GetProperty("errorMessage").GetProperty("defaultMessage").GetString() ?? string.Empty;
                    
                    return (null, errorCode, errorMessage);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Hashes the specified password using the SHA-256 algorithm.
        /// </summary>
        /// <param name="password">The password to be hashed.</param>
        /// <param name="iterations">The number of hash iterations.</param>
        /// <returns>The Base64-encoded SHA-256 hash of the password.</returns>
        static string Hash(string password, int iterations = 5000)
        {
            // Replace the yen symbol
            if (password.Contains("¥"))
                password = password.Replace("¥", "\\");

            // Encode in UTF-8
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password using SHA-256
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(passwordBytes);

            for (var i = 0; i < iterations; i++)
            {
                hashedBytes = sha256.ComputeHash(hashedBytes);
            }

            var base64PasswordHash = Convert.ToBase64String(hashedBytes);
            return base64PasswordHash;
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}