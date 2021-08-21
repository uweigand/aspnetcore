// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.AspNetCore.Analyzers.DelegateEndpoints;

public partial class DelegateEndpointAnalyzer : DiagnosticAnalyzer
{
    private static void DetectMisplacedLambdaAttribute(
        in OperationAnalysisContext context,
        WellKnownTypes wellKnownTypes,
        IInvocationOperation invocation,
        IMethodSymbol method)
    {
        var lambdaInvocation = invocation.GetAncestor<IAnonymousFunctionOperation>(OperationKind.AnonymousFunction);

        if (lambdaInvocation is null)
        {
            return;
        }

        var invocationParent = lambdaInvocation.GetAncestor<IInvocationOperation>(OperationKind.Invocation);

        if (invocationParent is null)
        {
            return;
        }

        if (IsNotDelegateHandlerInvocation(wellKnownTypes, invocationParent, invocationParent.TargetMethod))
        {
            return;
        }

        var attributes = method.GetAttributes();
        var location = lambdaInvocation.Syntax.GetLocation();

        foreach (var attribute in attributes)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.DetectMisplacedLambdaAttribute,
                location,
                attribute.AttributeClass?.Name,
                method.Name));
        }
    }
}