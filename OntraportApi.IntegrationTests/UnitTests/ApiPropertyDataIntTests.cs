﻿using System.Collections.Generic;
using HanumanInstitute.OntraportApi.Converters;
using HanumanInstitute.OntraportApi.Models;
using Xunit;

namespace HanumanInstitute.OntraportApi.UnitTests;

public class ApiPropertyDataIntTests
{
    private readonly string _key = "key1";
    private readonly ApiObject _host = new ApiObject();

    private ApiProperty<int> SetupProperty() => new ApiProperty<int>(_host, _key);

    private void Set(string value) => _host.Data[_key] = value;

    public static IEnumerable<object[]> GetValues() => new[] {
        new object[] { "0", 0 },
        new object[] { "5", 5 },
        new object[] { "-5", -5 }
    };

    [Theory]
    [MemberData(nameof(GetValues))]
    public void Value_SetRawValue_ReturnsExpectedValue(string rawValue, int typedValue)
    {
        var prop = SetupProperty();
        Set(rawValue);

        var result = prop.Value;

        Assert.Equal(typedValue, result);
    }

    [Theory]
    [MemberData(nameof(GetValues))]
    public void Value_SetValue_StoresExpectedRawValue(string rawValue, int typedValue)
    {
        var prop = SetupProperty();

        prop.Value = typedValue;

        Assert.Equal(rawValue, prop.RawValue);
    }

    [Theory]
    [MemberData(nameof(GetValues))]
#pragma warning disable xUnit1026 // typeValue not used
#pragma warning disable IDE0060   // typeValue not used
    public void HasValue_Set_ReturnsTrue(string rawValue, int _)
    {
        var prop = SetupProperty();
        Set(rawValue);

        var result = prop.HasValue;

        Assert.True(result);
    }
}
