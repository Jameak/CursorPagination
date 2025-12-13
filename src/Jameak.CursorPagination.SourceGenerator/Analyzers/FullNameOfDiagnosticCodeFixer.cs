using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Jameak.CursorPagination.SourceGenerator.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FullNameOfDiagnosticCodeFixer)), Shared]
internal sealed class FullNameOfDiagnosticCodeFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticHelper.s_suspiciousNameOfRule.Id);

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        const string actionName = "Add fullnameof prefix";
        var diagnosticToFix = context.Diagnostics.First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: actionName,
                createChangedDocument: cancellationToken => ApplyFix(context.Document, diagnosticToFix, cancellationToken),
                equivalenceKey: actionName),
            diagnosticToFix);

        return Task.CompletedTask;
    }

    private static async Task<Document> ApplyFix(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);

        var exprToFix = GetExpressionToFix(root, diagnostic);

        if (exprToFix == null)
        {
            return document;
        }

        var firstSimpleNameNode = HelperMethods.GetFirstSimpleNameSyntaxSyntax(exprToFix);

        if (firstSimpleNameNode == null)
        {
            return document;
        }

        var tokenToFix = firstSimpleNameNode.Identifier;
        if (tokenToFix.IsVerbatimIdentifier())
        {
            return document;
        }

        var newToken = SyntaxFactory.Identifier(
            tokenToFix.LeadingTrivia,
            "@" + tokenToFix.ValueText,
            tokenToFix.TrailingTrivia);

        var fixedSimpleName = firstSimpleNameNode.WithIdentifier(newToken);
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        editor.ReplaceNode(firstSimpleNameNode, fixedSimpleName);

        return editor.GetChangedDocument();
    }

    private static ExpressionSyntax? GetExpressionToFix(SyntaxNode? root, Diagnostic diagnostic)
    {
        if (root is null)
        {
            return null;
        }

        var nodeToFix = root.FindNode(diagnostic.Location.SourceSpan);
        if (nodeToFix is AttributeArgumentSyntax attrArgSyntax)
        {
            nodeToFix = attrArgSyntax.Expression;
        }

        if (nodeToFix is InvocationExpressionSyntax invocExprSyntax)
        {
            nodeToFix = invocExprSyntax.ArgumentList;
        }

        if (nodeToFix is ArgumentListSyntax argumentListSyntax)
        {
            nodeToFix = argumentListSyntax.Arguments.FirstOrDefault();
            if (nodeToFix == null)
            {
                return null;
            }
        }

        if (nodeToFix is not ArgumentSyntax argumentToFix)
        {
            return null;
        }

        return argumentToFix.Expression;
    }
}
