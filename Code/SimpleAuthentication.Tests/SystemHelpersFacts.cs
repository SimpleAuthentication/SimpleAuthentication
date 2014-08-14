using System;
using System.Collections.Generic;
using Shouldly;
using SimpleAuthentication.Core;
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
                var querystringParameters = new Dictionary<string, string>
                {
                    {"a", "1"},
                    {"b^*&/234", "&%7as ad7 6* SA "}
                };

                // Act.
                var result = SystemHelpers.CreateUri(uri, querystringParameters);

                // Assert.
                result.Query.ShouldBe("?a=1&b%5E%2A%26%2F234=%26%257as%20ad7%206%2A%20SA%20");
            }

            [Fact]
            public void GivenAUriWithSomeQuerystringParameters_AddOrUpdateQuery_AddsTheQuerystringParameters()
            {
                // Arrange.
                var uri = new Uri("http://www.mysite.com/a/b/c?pewpew=woot&foo=bar&x=y");
                var querystringParameters = new Dictionary<string, string>
                {
                    {"a", "1"},
                    {"b^*&/234", "&%7as ad7 6* SA "}
                };

                // Act.
                var result = SystemHelpers.CreateUri(uri, querystringParameters);

                // Assert.
                result.Query.ShouldBe("?pewpew=woot&foo=bar&x=y&a=1&b%5E%2A%26%2F234=%26%257as%20ad7%206%2A%20SA%20");
            }

            [Fact]
            public void GivenAUriWithSomeQuerystringParametersAndAnExistingStateValue_AddOrUpdateQuery_AddsTheQuerystringParametersAndUpdatesTheStateKeyValue()
            {
                // Arrange.
                var uri = new Uri("http://www.mysite.com/a/b/c?pewpew=woot&foo=bar&x=y&state=hithere");
                var querystringParameters = new Dictionary<string, string>
                {
                    {"a", "1"},
                    {"b^*&/234", "&%7as ad7 6* SA "},
                    {"state", "0aa44508-991a-47db-b6b4-d8edd4c0bf40"}
                };

                // Act.
                var result = SystemHelpers.CreateUri(uri, querystringParameters);

                // Assert.
                result.Query.ShouldBe("?pewpew=woot&foo=bar&x=y&state=0aa44508-991a-47db-b6b4-d8edd4c0bf40&a=1&b%5E%2A%26%2F234=%26%257as%20ad7%206%2A%20SA%20");
            }
        }
    }
}