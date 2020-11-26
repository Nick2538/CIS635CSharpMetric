using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeMetrics
{
    public class CSharpCodeAnalyzer
    {
        private CSharpSyntaxTree SyntaxTree { get; }
        private List<CSharpClass> SubClasses = new List<CSharpClass>();

        public CSharpCodeAnalyzer(string filepath)
        {
            SyntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(File.ReadAllText(filepath));
            IEnumerable<ClassDeclarationSyntax> classes = SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>().ToList();
            List<ClassDeclarationSyntax> classSyntax = new List<ClassDeclarationSyntax>(classes);
            foreach (var cls in classSyntax)
            {
                SubClasses.Add(new CSharpClass((CSharpSyntaxTree)cls.SyntaxTree));
            }
        }

        public IEnumerable<CSharpClass> IterSubClasses()
        {
            foreach (var cls in SubClasses)
            {
                yield return cls;
            }
        }
    }
}
