using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsGenerator
{
    public class Generator
    {
        private readonly string outputDirectory;
        private readonly int readerFilesCount;
        private readonly int writerFilesCount;
        private readonly int maxTasksCount;
        private readonly FileReader fileReader;
        private readonly FileWriter fileWriter;

        public Task Generate(List<string> paths)
        {
            ExecutionDataflowBlockOptions readerOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = readerFilesCount
            };
            ExecutionDataflowBlockOptions writerOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = writerFilesCount
            };
            ExecutionDataflowBlockOptions maxTasksOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxTasksCount
            };
            DataflowLinkOptions linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            var readerTransformBlock = new TransformBlock<string, Task<string>>(readPath => fileReader.ReadFileAsync(readPath), readerOptions);

            var writerTransformBlock = new ActionBlock<Task<List<GeneratedTest>>>(generatedTests => fileWriter.WriteFileAsync(generatedTests), writerOptions);

            var generatorTransformBlock = new TransformBlock<Task<string>, Task<List<GeneratedTest>>>(
                new Func<Task<string>, Task<List<GeneratedTest>>>(GenerateFileAsync), maxTasksOptions);

            readerTransformBlock.LinkTo(generatorTransformBlock, linkOptions);
            generatorTransformBlock.LinkTo(writerTransformBlock, linkOptions);

            foreach (var path in paths)
            {
                readerTransformBlock.Post(path);
            }

            readerTransformBlock.Complete();

            return writerTransformBlock.Completion;
        }

        private async Task<List<GeneratedTest>> GenerateFileAsync(Task<string> readSourseFile)
        {
            string sourceFile = await readSourseFile;
            var result = new List<GeneratedTest>();

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceFile);
            var compilationUnitRoot = syntaxTree.GetCompilationUnitRoot();

            var classes = compilationUnitRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classInfo in classes)
            {
                var publicMethods = classInfo.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .Where(x => x.Modifiers.Any(y => y.ValueText == "public"));

                var namespaceInfo = (classInfo.Parent as NamespaceDeclarationSyntax)?.Name.ToString();
                var className = classInfo.Identifier.ValueText;
                var methodsNames = new List<string>();

                foreach (var publicMethod in publicMethods)
                {
                    var tempMethodName = GetMethodName(methodsNames, publicMethod.Identifier.ToString(), 0);
                    methodsNames.Add(tempMethodName);
                }

                NamespaceDeclarationSyntax namespaceDeclarationSyntax = NamespaceDeclaration(QualifiedName(
                    IdentifierName(namespaceInfo), IdentifierName("Test")));

                CompilationUnitSyntax compilationUnit = CompilationUnit()
                    .WithUsings(GetUsings())
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclarationSyntax
                        .WithMembers(SingletonList<MemberDeclarationSyntax>(ClassDeclaration(className + "Test")
                            .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("TestClass"))))))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithMembers(GetMethods(methodsNames))))));

                var name = Path.Combine(outputDirectory, className + "Test.cs");

                result.Add(new GeneratedTest(name, compilationUnit.NormalizeWhitespace().ToFullString()));
            }

            return result;
        }

        private string GetMethodName(List<string> methods, string method, int count)
        {
            var res = method + (count == 0 ? "" : count.ToString());
            if (methods.Contains(res)) return GetMethodName(methods, method, count + 1);

            return res;
        }

        private SyntaxList<UsingDirectiveSyntax> GetUsings()
        {
            List<UsingDirectiveSyntax> usingDirectiveSyntax = new List<UsingDirectiveSyntax>()
            {
                UsingDirective(
                    QualifiedName(
                        QualifiedName(
                            QualifiedName(
                                IdentifierName("Microsoft"),
                                IdentifierName("VisualStudio")),
                            IdentifierName("TestTools")),
                        IdentifierName("UnitTesting")))
            };

            return List(usingDirectiveSyntax);
        }

        private SyntaxList<MemberDeclarationSyntax> GetMethods(List<string> methods)
        {
            var result = new List<MemberDeclarationSyntax>();

            foreach (var method in methods)
                result.Add(GetMethod(method));

            return List(result);
        }

        private MethodDeclarationSyntax GetMethod(string name)
        {
            return MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier(name + "Test"))
                .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("TestMethod"))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithBody(Block(ExpressionStatement(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Assert"), IdentifierName("Fail")))
                            .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    Literal("autogenerated")))))))));
        }

        public Generator(string outputDirectory, int readerFilesCount, int writerFilesCount, int maxTasksCount)
        {
            this.outputDirectory = outputDirectory;
            this.readerFilesCount = readerFilesCount;
            this.writerFilesCount = writerFilesCount;
            this.maxTasksCount = maxTasksCount;

            fileReader = new FileReader();
            fileWriter = new FileWriter();
        }
    }
}
