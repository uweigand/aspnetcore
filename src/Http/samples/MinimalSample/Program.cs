// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

// Console.WriteLine($"Waiting for debugger to attach on {System.Environment.ProcessId}");
// while (!System.Diagnostics.Debugger.IsAttached)
// {
//     Thread.Sleep(100);
// }
// Console.WriteLine("Debugger attached");

var dump = static (string s, Delegate d) =>
{
    var mi = d.Method;
    var context = new NullabilityInfoContext();
    var info = context.Create(mi.GetParameters()[0]);
    return new
    {
        ReadState = info.ReadState.ToString(),
        WriteState = info.WriteState.ToString(),
        Type = info.Type.ToString(),
        ElementType = info.ElementType?.ToString()
    };
};

var app = WebApplication.Create(args);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.MapGet("/foo", (string? name) => $"Inline lambda {name}");
// app.MapGet("/", (string? name) => { });

Console.WriteLine(dump("/", (string? name) => $"Inline lambda {name}"));
Console.WriteLine(dump("/o", (string? name) => { }));

app.Run();
