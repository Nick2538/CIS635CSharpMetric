using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Analysis
{
    public class CodeAnalyzer
    {
        public CSharpSyntaxTree SyntaxTree { get; }
        private List<ConstructorDeclarationSyntax> Constructors { get; }
        private List<MethodDeclarationSyntax> Methods { get; }

        public CodeAnalyzer(string filepath)
        {
            SyntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(File.ReadAllText(filepath));
            IEnumerable<ConstructorDeclarationSyntax> constructors = SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>().ToList();
            Constructors = new List<ConstructorDeclarationSyntax>(constructors);
            IEnumerable<MethodDeclarationSyntax> methods = SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>().ToList();
            Methods = new List<MethodDeclarationSyntax>(methods);
        }

        public int RecurseBlockCount(BlockSyntax block)
        {
            int statements = block.Statements.Count;
            foreach (var childBlock in block.DescendantNodes().OfType<BlockSyntax>().ToList())
            {
                statements += RecurseBlockCount(childBlock);
            }
            return statements;
        }

        public double AverageMethodSize()
        {
            int sum = 0;
            foreach (var method in Methods)
            {
                BlockSyntax body = method.Body;
                sum += RecurseBlockCount(body);
            }
            if (Methods.Count == 0)
            {
                return 0;
            }
            else
            {
                return ((double)sum) / Methods.Count;
            }
        }

        public Dictionary<string, int> MethodUsage()
        {
            Dictionary<string, int> usages = new Dictionary<string, int>();
            foreach (var method in Methods) 
            {
                SyntaxToken methodID = method.Identifier;
                string methodName = methodID.ValueText;
                if (!usages.ContainsKey(methodName))
                {
                    usages.Add(methodName, 0);
                }
                BlockSyntax body = method.Body;
                foreach (var statement in body.Statements)
                {
                    foreach(var token in statement.ChildTokens())
                    {
                        if (token.Equals(methodID))
                        {
                            usages.Add(methodName, usages[methodName] + 1);
                        }
                    }
                }
            }
            return usages;
        }
    }
}
