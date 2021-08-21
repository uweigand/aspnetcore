// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.AspNetCore.Analyzer.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Analyzers.DelegateEndpoints;

public partial class DetectMisplacedLambdaAttributeTest
{
    private TestDiagnosticAnalyzerRunner Runner { get; } = new(new DelegateEndpointAnalyzer());

    [Fact]
    public async Task MinimalAction_WithCorrectlyPlacedAttribute_Works()
    {
        // Arrange
        var source = @"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/"", [Authorize] () => Hello());
void Hello() { }
";
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source);

        // Assert
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task MinimalAction_WithMisplacedAttribute_ProducesDiagnostics()
    {
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
var app = WebApplication.Create();
app.MapGet(""/"", /*MM*/() => Hello());
[Authorize]
void Hello() { }
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        var diagnostic = Assert.Single(diagnostics);
        Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
        AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
        Assert.Equal("AuthorizeAttribute should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task MinimalAction_WithMultipleMisplacedAttributes_ProducesDiagnostics()
    {
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
var app = WebApplication.Create();
app.MapGet(""/"", /*MM*/() => Hello());
[Authorize]
[Produces(""application/xml"")]
void Hello() { }
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        Assert.Collection(diagnostics,
            diagnostic => {
                Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
                AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
                Assert.Equal("AuthorizeAttribute should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
            },
            diagnostic => {
                Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
                AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
                Assert.Equal("ProducesAttribute should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
            }
        );
    }

    [Fact]
    public async Task MinimalAction_WithSingleMisplacedAttribute_ProducesDiagnostics()
    {
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
var app = WebApplication.Create();
app.MapGet(""/"", /*MM*/[Authorize]() => Hello());
[Produces(""application/xml"")]
void Hello() { }
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        Assert.Collection(diagnostics,
            diagnostic => {
                Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
                AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
                Assert.Equal("ProducesAttribute should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
            }
        );
    }

    [Fact]
    public async Task MinimalAction_WithMisplacedAttributesOnMultipleMethods_ProducesDiagnostics()
    {
        // Arrange
        var source = TestSource.Read(@"
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
var app = WebApplication.Create();
app.MapGet(""/"", /*MM*/() => {
    if (true)
    {
        Hello();
    }
    Bye();
});
[Authorize]
void Hello() { }
[Produces(""application/xml"")]
void Bye() { }
");
        // Act
        var diagnostics = await Runner.GetDiagnosticsAsync(source.Source);

        // Assert
        Assert.Collection(diagnostics,
            diagnostic => {
                Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
                AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
                Assert.Equal("AuthorizeAttribute should be placed on the delegate instead of Hello", diagnostic.GetMessage(CultureInfo.InvariantCulture));
            },
            diagnostic => {
                Assert.Same(DiagnosticDescriptors.DetectMisplacedLambdaAttribute, diagnostic.Descriptor);
                AnalyzerAssert.DiagnosticLocation(source.DefaultMarkerLocation, diagnostic.Location);
                Assert.Equal("ProducesAttribute should be placed on the delegate instead of Bye", diagnostic.GetMessage(CultureInfo.InvariantCulture));
            }
        );
    }
}

