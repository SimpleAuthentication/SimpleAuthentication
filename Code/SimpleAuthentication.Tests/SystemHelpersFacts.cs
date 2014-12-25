using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using Xunit;

namespace SimpleAuthentication.Tests
{
    public class SystemHelpersFacts
    {
        public class AddOrUpdateQueryFacts
        {
            [Fact]
            public void GivenAUriWithNoQuerystringParameters_AddOrUpdateQuery_AddsTheQuerystringParameters()
            {
                // Arrange.
                var uri = new Uri("http://www.mysite.com/a/b/c");

                // Act.
                var result = SystemHelpers.CreateUri(uri, null);

                // Assert.
                result.Query.ShouldBeNullOrEmpty();
            }

            [Fact]
            public void GivenAUriWithSomeQuerystringParameters_AddOrUpdateQuery_AddsTheQuerystringParameters()
            {
                // Arrange.
                var uri = new Uri("http://www.mysite.com/a/b/c");

                var querystringParameters = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("a", "1"),
                    new KeyValuePair<string, string>("b^*&/234", "&%7as ad7 6* SA ")
                };

                // Act.
                var result = SystemHelpers.CreateUri(uri, 
                    querystringParameters.ToDictionary(key => key.Key, value => value.Value));

                // Assert.
                var query = string.Format("?{0}", querystringParameters.ToEncodedString());
                result.Query.ShouldBe(query);
            }

            [Fact]
            public void GivenAUriWithSomeQuerystringParametersAndAnExistingStateValue_AddOrUpdateQuery_AddsTheQuerystringParametersAndUpdatesTheStateKeyValue()
            {
                // Arrange.
                var uri = new Uri("http://www.mysite.com/a/b/c?pewpew=woot&foo=bar&x=y&state=hithere");
                var querystringParameters = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("a", "1"),
                    new KeyValuePair<string, string>("b^*&/234", "&%7as ad7 6* SA "),
                    new KeyValuePair<string, string>("state", "0aa44508-991a-47db-b6b4-d8edd4c0bf40")
                };

                // Act.
                var result = SystemHelpers.CreateUri(uri,
                    querystringParameters.ToDictionary(key => key.Key, value => value.Value));

                // Assert.
                var queryStringSegments = new List<KeyValuePair<string, string>>
                {
                    {new KeyValuePair<string, string>("pewpew", "woot")},
                    {new KeyValuePair<string, string>("foo", "bar")},
                    {new KeyValuePair<string, string>("x", "y")}
                };
                queryStringSegments.Add(querystringParameters[2]);
                queryStringSegments.Add(querystringParameters[0]);
                queryStringSegments.Add(querystringParameters[1]);
                var encodedQueryString = queryStringSegments.ToEncodedString();
                var url = string.Format("?{0}", encodedQueryString);
                result.Query.ShouldBe(url);
            }
        }

        public class ConvertKeyValueContentToDictionaryFacts
        {
            [Fact]
            public void GivenSomeKeyValueContent_ConvertKeyValueContentToDictionary_ReturnsADictionaryWithKeyValues()
            {
                // Arrange.
                const string content = "a=1&b=2&c=3";

                // Act.
                var result = SystemHelpers.ConvertKeyValueContentToDictionary(content);

                // Assert.
                result.Count.ShouldBe(3);
                result.ContainsKey("a").ShouldBe(true);
                result["a"].ShouldBe("1");
                result.ContainsKey("b").ShouldBe(true);
                result["b"].ShouldBe("2");
                result.ContainsKey("c").ShouldBe(true);
                result["c"].ShouldBe("3");
                result.ContainsKey("xx").ShouldBe(false);
            }

            [Fact]
            public void GivenSomeBadContent_ConvertKeyValueContentToDictionary_ReturnsAnEmptyDictionary()
            {
                // Arrange.
                const string content = "i am some bad content";

                // Act.
                var result = SystemHelpers.ConvertKeyValueContentToDictionary(content);

                // Assert.
                result.Count.ShouldBe(0);
            }

            [Fact]
            public void GivenSomeMixedKeyValueContentAndSomeBadContent_ConvertKeyValueContentToDictionary_ReturnsADictionary()
            {
                // Arrange.
                const string mixedContent = "a=1&i am some bad content";

                // Act.
                var result = SystemHelpers.ConvertKeyValueContentToDictionary(mixedContent);

                // Assert.
                result.Count.ShouldBe(1);
                result.ContainsKey("a").ShouldBe(true);
                result["a"].ShouldBe("1");
            }
        }

        public class CrossSiteRequestForgeryCheckFacts
        {
            [Fact]
            public void GivenAQuerystringAndMatchingStateValues_CrossSiteRequestForgeryCheck_ThrowsNoException()
            {
                // Arrange.
                const string stateKey = "state";
                const string state = "675E6D63-B292-4D0E-B0FA-0DC39FDD8C89";
                var querystring = new Dictionary<string, string>
                {
                    {stateKey, state},
                    {"pewpew", "han solo"}
                };

                // Act & Assert.
                SystemHelpers.CrossSiteRequestForgeryCheck(querystring, state, stateKey);
            }

            [Fact]
            public void GivenAQuerystringWhichIsMissingAStateKeyValue_CrossSiteRequestForgeryCheck_ThrowsAnException()
            {
                // Arrange.
                const string stateKey = "state";
                const string state = "675E6D63-B292-4D0E-B0FA-0DC39FDD8C89";
                var querystring = new Dictionary<string, string>
                {
                    {"pewpew", "han solo"}
                };

                // Act.
                var exception = Should.Throw<AuthenticationException>(() => SystemHelpers.CrossSiteRequestForgeryCheck(querystring, state, stateKey));

                // Assert.
                exception.Message.ShouldBe(
                    "The callback querystring doesn't include a state key/value parameter. We need one of these so we can do a CSRF check. Please check why the request url from the provider is missing the parameter: 'state'. eg. &state=something...");
            }

            [Fact]
            public void GivenAQuerystringButTheStateValuesMismatch_CrossSiteRequestForgeryCheck_ThrowsAnException()
            {
                // Arrange.
                const string stateKey = "state";
                const string state = "675E6D63-B292-4D0E-B0FA-0DC39FDD8C89";
                var querystring = new Dictionary<string, string>
                {
                    {stateKey, "asdasd"},
                    {"pewpew", "han solo"}
                };

                // Act.
                var exception = Should.Throw<AuthenticationException>(() => SystemHelpers.CrossSiteRequestForgeryCheck(querystring, state, stateKey));

                // Assert.
                exception.Message.ShouldBe(
                    "CSRF check fails: The callback 'state' value 'asdasd' doesn't match the server's *remembered* state value '************************************'.");
            }
        }
    }
}