using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Test.Helpers
{
    public class TestWebBrowser
    {
        public readonly string LoginPath = "/chat/Account/Login";

        private readonly CookieContainer _cookies;

        private readonly TestServer _testServer;

        private Stack<HttpResponseMessage> _responses = new Stack<HttpResponseMessage>();

        public HttpResponseMessage Response => _responses.Peek();

        public TestWebBrowser(TestServer testServer)
        {
            _testServer = testServer;
            _cookies = new CookieContainer();
        }

        public async Task<HttpResponseMessage> PostAsync(string requestPath, Action<TestRequestBuilder> setup = null)
        {
            var builder = new TestRequestBuilder(requestPath, _testServer, _cookies);

            if (setup != null)
            {
                setup(builder);
            }

            var response = await builder.PostAsync();
            _responses.Push(response);
            return response;
        }

        public async Task<HttpResponseMessage> PostJsonAsync(string requestPath, object postObject, Action<TestRequestBuilder> setup = null)
        {
            var response = await PostAsync(requestPath, b => {
                b.SetJsonContent(postObject);
                if (setup != null)
                {
                    setup(b);
                }
            });

            return response;
        }

        public async Task<T> PostJsonDeserializeResultAsync<T>(string requestPath, object postObject, Action<TestRequestBuilder> setup = null)
        {
            await PostJsonAsync(requestPath, postObject, setup);
            return await DeserializeJsonResultAsync<T>();
        }

        public async Task<T> GetJsonDeserializeResultAsync<T>(string requestPath, Action<TestRequestBuilder> setup = null)
        {
            await GetAsync(requestPath, setup);
            return await DeserializeJsonResultAsync<T>();
        }

        public async Task<HttpResponseMessage> GetAsync(string requestPath, Action<TestRequestBuilder> setup = null)
        {
            var builder = new TestRequestBuilder(requestPath, _testServer, _cookies);
            if (setup != null)
            {
                setup(builder);
            }

            var response = await builder.GetAsync();
            _responses.Push(response);
            return response;
        }

        public async Task<HttpResponseMessage> FollowRedirect()
        {
            var response = Response;
            if (response.StatusCode != HttpStatusCode.Moved && response.StatusCode != HttpStatusCode.Redirect)
            {
                return response;
            }
            var redirectUrl = new Uri(response.Headers.Location.ToString(), UriKind.RelativeOrAbsolute);
            if (redirectUrl.IsAbsoluteUri)
            {
                redirectUrl = new Uri(redirectUrl.PathAndQuery, UriKind.Relative);
            }
            return await GetAsync(redirectUrl.ToString());
        }

        public async Task<HttpResponseMessage> GetLoginAsync()
        {
            return await GetAsync(LoginPath);
        }

        public async Task<bool> TryLoginAsync(ApplicationUser user, string password = null)
        {
            await PostAsync(LoginPath, b =>
            {
                b.Form(form =>
                {
                    form.Add("Email", user.Email);
                    form.Add("Password", password);
                });
            });

            return Response.StatusCode == HttpStatusCode.Redirect
                && Response.Headers.Location.OriginalString == "/chat";
        }

        public async Task<T> DeserializeJsonResultAsync<T>()
        {
            var obj = JsonConvert.DeserializeObject<T>(await Response.Content.ReadAsStringAsync(), new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            return obj;
        }

        public async Task<IDictionary<string, IEnumerable<ValidationErrorViewModel>>> DeserializeApiErrorJsonResultAsync()
        {
            return await DeserializeJsonResultAsync<IDictionary<string, IEnumerable<ValidationErrorViewModel>>>();
        }
    }
}