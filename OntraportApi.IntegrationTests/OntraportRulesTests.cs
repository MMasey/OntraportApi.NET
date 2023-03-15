﻿using HanumanInstitute.OntraportApi.Models;
using Xunit.Abstractions;

namespace HanumanInstitute.OntraportApi.IntegrationTests;

public class OntraportRulesTests : OntraportBaseDeleteTests<OntraportRules, ApiRule>
{
    public OntraportRulesTests(ITestOutputHelper output) :
        base(output, 1, "rule1")
    {
    }


}
