using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ChatApp.Test.Helper
{
    public class TestRequestBuilder
    {
        private readonly TestServer _testServer;

        private readonly CookieContainer _cookies;

        private Uri _absoluteUrl;

        private HttpContent _content;

        private IDictionary<string, string> _queryStrings = new Dictionary<string, string>();

        private bool _withoutCookie = false;

        private bool _withoutXsrfToken = false;

        private string _xsrfCookieName = "XSRF-TOKEN";

        private string _xsrfHeaderName = "X-XSRF-TOKEN";

        public TestRequestBuilder(string requestPath, TestServer testServer, CookieContainer cookies)
        {
            _testServer = testServer;
            _cookies = cookies;
            
            var requestUrl = new Uri(requestPath, UriKind.RelativeOrAbsolute);
            _absoluteUrl = requestUrl.IsAbsoluteUri ? requestUrl : new Uri(_testServer.BaseAddress, requestUrl);
        }

        public TestRequestBuilder SetJsonContent(object postObject)
        {
            var json = JsonConvert.SerializeObject(postObject, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            _content = new StringContent(json, Encoding.UTF8, "applicaiton/json");
            return this;
        }

        public TestRequestBuilder Form(Action<IDictionary<string, string>> formSetup)
        {
            var formValues = new Dictionary<string, string>();
            formSetup(formValues);
            _content = new FormUrlEncodedContent(formValues);
            return this;
        }

        public TestRequestBuilder Query(Action<IDictionary<string, string>> querySetup)
        {
            querySetup(_queryStrings);
            return this;
        }

        public TestRequestBuilder WithoutCookie()
        {
            _withoutCookie = true;
            return this;
        }

        public TestRequestBuilder WithoutXsrfToken()
        {
            _withoutXsrfToken = true;
            return this;
        }

        public TestRequestBuilder WithXsrfHeaderName(string headerName)
        {
            _xsrfHeaderName = headerName;
            return this;
        }

        public TestRequestBuilder WithXsrfCookieName(string cookieName)
        {
            _xsrfCookieName = cookieName;
            return this;
        }

        private RequestBuilder Build()
        {
            var requestUrl = _absoluteUrl.ToString();

            if (_queryStrings.Any())
            {
                requestUrl = QueryHelpers.AddQueryString(requestUrl, _queryStrings);
            }

            var builder = _testServer.CreateRequest(requestUrl);

            if (!_withoutCookie)
            {
                AddCookies(builder, _absoluteUrl);
            }

            if (!_withoutXsrfToken)
            {
                SetXsrfHeader(builder, _absoluteUrl);
            }

            if (_content != null)
            {
                builder.And(m =>
                {
                    m.Content = _content;
                });
            }

            return builder;
        }

        public async Task<HttpResponseMessage> GetAsync()
        {
            var builder = Build();
            var response = await builder.GetAsync();
            if (!_withoutCookie)
            {
                UpdateCookies(response, _absoluteUrl);
            }
            return response;
        }

        public async Task<HttpResponseMessage> PostAsync()
        {
            var builder = Build();
            var response = await builder.PostAsync();
            if (!_withoutCookie)
            {
                UpdateCookies(response, _absoluteUrl);
            }
            return response;
        }

        private void AddCookies(RequestBuilder requestBuilder, Uri absoluteUrl)
        {
            var cookieHeader = _cookies.GetCookieHeader(absoluteUrl);
            if (!string.IsNullOrWhiteSpace(cookieHeader))
            {
                requestBuilder.AddHeader(HeaderNames.Cookie, cookieHeader);
            }
        }

        private void UpdateCookies(HttpResponseMessage response, Uri absoluteUrl)
        {
            if (response.Headers.Contains(HeaderNames.SetCookie))
            {
                var cookieHeaders = SetCookieHeaderValue.ParseList(response.Headers.GetValues(HeaderNames.SetCookie).ToList());
                foreach (var header in cookieHeaders)
                {
                    var uri = new Uri(_testServer.BaseAddress, header.Path.ToString());
                    _cookies.Add(uri, new Cookie(header.Name.ToString(), header.Value.ToString()));
                }
            }
        }

        private void SetXsrfHeader(RequestBuilder requestBuilder, Uri absoluteUrl)
        {
            var cookies = _cookies.GetCookies(absoluteUrl);
            var cookie = cookies[_xsrfCookieName];
            if (cookie != null)
            {
                requestBuilder.AddHeader(_xsrfHeaderName, cookie.Value);
            }
        }
    }
}