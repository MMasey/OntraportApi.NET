﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EmergenceGuardian.OntraportApi.Converters;
using EmergenceGuardian.OntraportApi.Models;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.OntraportApi.IntegrationTests
{
    public abstract class OntraportBaseReadTests<T, U>
        where T : OntraportBaseRead<U>
        where U : ApiObject
    {
        protected readonly ITestOutputHelper Output;
        protected readonly int ValidId;

        public OntraportBaseReadTests(ITestOutputHelper output, int validId)
        {
            Output = output;
            ValidId = validId;
        }

        protected T SetupApi()
        {
            var config = new ConfigHelper().GetConfig();
            var requestHelper = new OntraportRequestHelper(config, new WebRequestService());
            return (T)Activator.CreateInstance(typeof(T), new[] { requestHelper });
        }

        protected OntraportObjects SetupObjectsApi()
        {
            var config = new ConfigHelper().GetConfig();
            var requestHelper = new OntraportRequestHelper(config, new WebRequestService());
            return new OntraportObjects(requestHelper);
        }

        [Fact]
        public async Task SelectAsync_ValidId_ReturnsData()
        {
            var api = SetupApi();

            var result = await api.SelectAsync(ValidId);

            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task SelectMultipleAsync_ValidId_ReturnsData()
        {
            var api = SetupApi();

            var result = await api.SelectMultipleAsync(new ApiSearchOptions(ValidId));

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task SelectMultipleAsync_NoArgs_ReturnsAll()
        {
            var api = SetupApi();

            var result = await api.SelectMultipleAsync();

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task SelectMetadataAsync_NoArg_ReturnsData()
        {
            var api = SetupApi();

            var result = await api.GetMetadataAsync();

            Assert.NotEmpty(result.Fields);
        }

        [Fact]
        public async Task SelectCollectionInfoAsync_NoArg_ReturnsData()
        {
            var api = SetupApi();

            var result = await api.GetCollectionInfoAsync();

            Assert.NotEmpty(result.ListFields);
        }

        [Fact]
        public async Task SelectAsync_ValidId_AllPropertiesHaveKey()
        {
            var api = SetupApi();
            var hasError = false;

            var result = await api.SelectAsync(ValidId);

            foreach (var propInfo in result.GetType().GetProperties())
            {
                if (IsGenericTypeOf(typeof(ApiPropertyBase<,>), propInfo.PropertyType))
                {
                    var prop = propInfo.GetValue(result);
                    var hasKeyInfo = prop.GetType().GetProperty("HasKey");
                    if (hasKeyInfo != null)
                    {
                        var hasKey = (bool)hasKeyInfo.GetValue(prop);
                        if (!hasKey)
                        {
                            hasError = true;
                            var keyInfo = prop.GetType().GetProperty("Key");
                            var key = keyInfo.GetValue(prop);
                            Output.WriteLine(key.ToString());
                        }
                    }
                }
            }
            Assert.False(hasError, "Some keys are not present in the dictionary and have been listed in output.");
        }

        [Fact]
        public async Task SelectAsync_ValidId_AllPropertiesHaveValueProperty()
        {
            var api = SetupApi();
            var hasError = false;

            var result = await api.SelectAsync(ValidId);

            foreach (var propInfo in result.GetType().GetProperties())
            {
                if (IsGenericTypeOf(typeof(ApiPropertyBase<,>), propInfo.PropertyType))
                {
                    var valuePropName = propInfo.Name + "Value";
                    var valueProp = result.GetType().GetProperty(valuePropName);

                    if (valueProp == null)
                    {
                        hasError = true;
                        Output.WriteLine(propInfo.Name);
                    }
                }
            }
            Assert.False(hasError, "Some properties don't have a matching Value property.");
        }

        private bool IsGenericTypeOf(Type genericType, Type someType)
        {
            if (someType.IsGenericType
                    && genericType == someType.GetGenericTypeDefinition()) return true;

            return someType.BaseType != null
                    && IsGenericTypeOf(genericType, someType.BaseType);
        }

        [Fact]
        public async Task SelectAsync_ValidId_AllKeysHaveProperties()
        {
            var api = SetupApi();
            var hasError = false;

            var result = await api.SelectAsync(ValidId);

            var propList = GetAllFieldProperties(result);
            var customFieldRegex = new Regex("^f[0-9]{4}$");
            foreach (var key in result.Data.Keys)
            {
                if (!customFieldRegex.Match(key).Success)
                {
                    if (!propList.Contains(key))
                    {
                        hasError = true;
                        Output.WriteLine($"{key} :      {result.Data[key]}");
                    }
                }
            }
            Assert.False(hasError, "Some dictionary keys don't have properties and have been listed in output.");
        }

        private List<string> GetAllFieldProperties(U obj)
        {
            var result = new List<string>();
            foreach (var propInfo in obj.GetType().GetProperties())
            {
                if (IsGenericTypeOf(typeof(ApiPropertyBase<,>), propInfo.PropertyType))
                {
                    var prop = propInfo.GetValue(obj);
                    var keyInfo = prop.GetType().GetProperty("Key");
                    if (keyInfo != null)
                    {
                        var key = keyInfo.GetValue(prop);
                        result.Add(key.ToString());
                    }
                }
            }
            return result;
        }
    }
}
