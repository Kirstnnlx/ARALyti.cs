using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ARALyti.cs.Services
{
    public class TopicResult
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public string Level { get; set; }
    }

    public class KeywordDetector
    {
        public List<TopicResult> DetectTopics(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return new List<TopicResult>();

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();
            var collector = new TopicCollector();
            collector.Visit(root);
            return collector.GetResults();
        }
    }

    internal class TopicCollector : CSharpSyntaxWalker
    {
        private readonly Dictionary<string, int> _scores = new();
        private readonly Stack<string> _methodStack = new();
        private readonly HashSet<string> _detectedCollections = new(); // avoid duplicate counting

        private void Add(string topic, int points, int cap = 100)
        {
            _scores.TryGetValue(topic, out int current);
            _scores[topic] = Math.Min(cap, current + points);
        }

        // ==================== CLASSES AND OBJECTS ====================
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            Add("Classes and Objects", 40);
            Add("Object-Oriented Programming", 25); // base OOP points
            base.VisitClassDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            Add("Classes and Objects", 30);
            base.VisitConstructorDeclaration(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            bool isThrown = node.Parent is ThrowStatementSyntax || node.Parent is ThrowExpressionSyntax;
            if (!isThrown)
                Add("Classes and Objects", 20);
            base.VisitObjectCreationExpression(node);
        }

        // ==================== OBJECT-ORIENTED PROGRAMMING ====================
        public override void VisitBaseList(BaseListSyntax node)
        {
            Add("Object-Oriented Programming", 35);
            Add("Inheritance", 100); // presence-based
            base.VisitBaseList(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            Add("Object-Oriented Programming", 40);
            Add("Inheritance", 100);
            base.VisitInterfaceDeclaration(node);
        }

        // ==================== METHODS AND FUNCTIONS ====================
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            Add("Methods and Functions", 25);

            if (node.Modifiers.Any(SyntaxKind.OverrideKeyword))
                Add("Object-Oriented Programming", 35);
            if (node.Modifiers.Any(SyntaxKind.VirtualKeyword))
                Add("Object-Oriented Programming", 25);
            if (node.Modifiers.Any(SyntaxKind.AbstractKeyword))
                Add("Object-Oriented Programming", 25);

            _methodStack.Push(node.Identifier.Text);
            base.VisitMethodDeclaration(node);
            _methodStack.Pop();
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            Add("Methods and Functions", 20);
            _methodStack.Push(node.Identifier.Text);
            base.VisitLocalFunctionStatement(node);
            _methodStack.Pop();
        }

        // ==================== ENCAPSULATION ====================
        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
                Add("Encapsulation", 30);
            else if (node.Modifiers.Any(SyntaxKind.ProtectedKeyword))
                Add("Encapsulation", 20);
            base.VisitFieldDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            Add("Encapsulation", 25);
            bool hasPrivateSetter = node.AccessorList?.Accessors
                .Any(a => a.Kind() == SyntaxKind.SetAccessorDeclaration && a.Modifiers.Any(SyntaxKind.PrivateKeyword)) ?? false;
            if (hasPrivateSetter)
                Add("Encapsulation", 20);
            base.VisitPropertyDeclaration(node);
        }

        // ==================== CONDITIONAL STATEMENTS ====================
        public override void VisitIfStatement(IfStatementSyntax node)
        {
            Add("Conditional Statements", 30);
            if (node.Else?.Statement is IfStatementSyntax)
                Add("Conditional Statements", 15);
            base.VisitIfStatement(node);
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            Add("Conditional Statements", 40);
            base.VisitSwitchStatement(node);
        }

        public override void VisitSwitchExpression(SwitchExpressionSyntax node)
        {
            Add("Conditional Statements", 35);
            base.VisitSwitchExpression(node);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Add("Conditional Statements", 15);
            base.VisitConditionalExpression(node);
        }

        // ==================== EXCEPTION HANDLING ====================
        public override void VisitTryStatement(TryStatementSyntax node)
        {
            Add("Exception Handling", 25);
            if (node.Finally != null)
                Add("Exception Handling", 30);
            base.VisitTryStatement(node);
        }

        public override void VisitCatchClause(CatchClauseSyntax node)
        {
            int points = node.Declaration != null ? 25 : 10;
            Add("Exception Handling", points);
            base.VisitCatchClause(node);
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            Add("Exception Handling", 20);
            base.VisitThrowStatement(node);
        }

        public override void VisitThrowExpression(ThrowExpressionSyntax node)
        {
            Add("Exception Handling", 20);
            base.VisitThrowExpression(node);
        }

        // ==================== LOOPS ====================
        public override void VisitForStatement(ForStatementSyntax node)
        {
            Add("Loops", 40);
            base.VisitForStatement(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Add("Loops", 40);
            base.VisitForEachStatement(node);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            Add("Loops", 40);
            base.VisitWhileStatement(node);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            Add("Loops", 35);
            base.VisitDoStatement(node);
        }

        // ==================== ARRAYS ====================
        public override void VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            Add("Arrays", 50);
            base.VisitArrayCreationExpression(node);
        }

        public override void VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
        {
            Add("Arrays", 40);
            base.VisitImplicitArrayCreationExpression(node);
        }

        public override void VisitArrayType(ArrayTypeSyntax node)
        {
            Add("Arrays", 30);
            base.VisitArrayType(node);
        }

        // ==================== COLLECTIONS (unique type only) ====================
        private static readonly HashSet<string> CollectionTypes = new()
        {
            "List", "Dictionary", "HashSet", "Queue", "Stack",
            "LinkedList", "SortedDictionary", "SortedSet",
            "IEnumerable", "ICollection", "IList", "IDictionary"
        };

        public override void VisitGenericName(GenericNameSyntax node)
        {
            string typeName = node.Identifier.Text;
            if (CollectionTypes.Contains(typeName) && !_detectedCollections.Contains(typeName))
            {
                _detectedCollections.Add(typeName);
                Add("Collections", 35); // per unique collection type
            }
            base.VisitGenericName(node);
        }

        // ==================== RECURSION ====================
        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (_methodStack.Count > 0)
            {
                string current = _methodStack.Peek();
                string called = node.Expression switch
                {
                    IdentifierNameSyntax id => id.Identifier.Text,
                    MemberAccessExpressionSyntax ma when ma.Expression is ThisExpressionSyntax => ma.Name.Identifier.Text,
                    _ => null
                };
                if (called != null && called == current)
                    Add("Recursion", 100);
            }
            base.VisitInvocationExpression(node);
        }

        // ==================== FILE HANDLING ====================
        private static readonly HashSet<string> FileTypes = new()
        {
            "File", "Directory", "Path", "FileStream",
            "StreamReader", "StreamWriter", "BinaryReader",
            "BinaryWriter", "FileInfo", "DirectoryInfo"
        };

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.Expression is IdentifierNameSyntax typeName && FileTypes.Contains(typeName.Identifier.Text))
                Add("File Handling", 40);
            base.VisitMemberAccessExpression(node);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            string ns = node.Name?.ToString() ?? "";
            if (ns == "System.IO" || ns.StartsWith("System.IO."))
                Add("File Handling", 30);
            base.VisitUsingDirective(node);
        }

        // ==================== OUTPUT ====================
        public List<TopicResult> GetResults()
        {
            return _scores
                .Where(kv => kv.Value > 0)
                .Select(kv => new TopicResult
                {
                    Name = kv.Key,
                    Score = kv.Value,
                    Level = kv.Value switch
                    {
                        >= 75 => "Strong",
                        >= 40 => "Developing",
                        _ => "Weak"
                    }
                })
                .OrderByDescending(r => r.Score)
                .ToList();
        }
    }
}