using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;


namespace AzSQL
{
    public class AzSQL
    {
        public static IEnumerable<DynamicTableEntity> RunQuery(string sqlQuery, StorageCredentials creds)
        {
            AzSQLParseTree parseTree = ParseAzSQL(sqlQuery);
            return AzTableQuery.RunQuery(parseTree, creds);
        }

        //overload
        public static IEnumerable<DynamicTableEntity> RunQuery(string sqlQuery, string acctName, string acctKey)
        {
            StorageCredentials creds = new StorageCredentials(acctName, acctKey);
            return RunQuery(sqlQuery, creds);
        }

        //overload
        public static IEnumerable<DynamicTableEntity> RunQuery(string sqlQuery, string SASToken)
        {
            StorageCredentials creds = new StorageCredentials(SASToken);
            return RunQuery(sqlQuery, creds);
        }

        internal static AzSQLParseTree ParseAzSQL(string input) 
        {   
            AntlrInputStream stream = new AntlrInputStream(input); 
            ITokenSource lexer = new AzSQLLexer(stream); 
            ITokenStream tokens = new CommonTokenStream(lexer); 
            AzSQLParser parser = new AzSQLParser(tokens); 
            parser.BuildParseTree = true;
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ParsingErrorListener());
            IParseTree tree = parser.stat();

            MyVisitor visitor = new MyVisitor();
            AzSQLParseTree parseTree = visitor.Visit(tree);
            return parseTree;
        }

    }


    class AzSQLParseTree
    {
        public List<string> columns = null;
        public bool allcolumns = false;

        public string tablename = null;

        public StringBuilder filterCondition = new StringBuilder(512);
        public bool hasFilterCondition = false;
    }


    class MyVisitor : AzSQLBaseVisitor<AzSQLParseTree>
    {
        AzSQLParseTree parseTree = new AzSQLParseTree();
        public override AzSQLParseTree VisitSelect_clause(AzSQLParser.Select_clauseContext context)
        {

            //Get List of Columns
            if (context.column_list().GetText() == "*") { 
                parseTree.allcolumns = true; 
                //Console.WriteLine("ColumnName is *");
            }
            else
            {
                //Console.WriteLine("Columns:");
                parseTree.columns = new List<string>(64);
                foreach (AzSQLParser.Column_nameContext columnName in context.column_list().column_name())
                {
                    string thisColumn = columnName.ID().GetText();
                    parseTree.columns.Add(thisColumn);
                    //Console.WriteLine("   {0}", thisColumn);
                }
            }

            // Get Table Name
            parseTree.tablename = context.table_name().GetText();
            //Console.WriteLine("Table Name: {0}", parseTree.tablename);

            if (context.where_clause() == null)
            {
                //Console.WriteLine("No WHERE clause");
                parseTree.hasFilterCondition = false;
            }
            else
            {
                parseTree.hasFilterCondition = true;
                //Console.WriteLine("Where Start: {0}", context.where_clause().expr().GetText()); 
                Visit(context.where_clause().expr());
                //Console.WriteLine("Where: {0}", parseTree.filterCondition.ToString());
            }

            return parseTree;

        }

        public override AzSQLParseTree VisitNotExpr(AzSQLParser.NotExprContext context)
        {
            //Console.WriteLine("   Entering NotExpr");
            parseTree.filterCondition.Append("(not ");
            Visit(context.expr()); 
            parseTree.filterCondition.Append(")");
            //Console.WriteLine("   NotExpr: {0}", parseTree.filterCondition.ToString());

            return parseTree;
        }

        public override AzSQLParseTree VisitBaseComparison(AzSQLParser.BaseComparisonContext context)
        {
            //Console.WriteLine("   Entering BaseComparison");
            parseTree.filterCondition.Append("(");
            parseTree.filterCondition.Append(context.ID().GetText());
            parseTree.filterCondition.Append(OpToString(context.COMPARISON_OP().GetText()));
            if (context.NEGATION() != null) { parseTree.filterCondition.Append(context.NEGATION().GetText()); }
            parseTree.filterCondition.Append(context.literal().GetText());
            parseTree.filterCondition.Append(")");
            //Console.WriteLine("   BaseComparison: {0}", parseTree.filterCondition.ToString()); 

            return parseTree;
        }

        public override AzSQLParseTree VisitAndExpr(AzSQLParser.AndExprContext context)
        {
            //Console.WriteLine("   Entering AndExpr");
            parseTree.filterCondition.Append("(");
            Visit(context.expr(0));
            parseTree.filterCondition.Append(" and ");
            Visit(context.expr(1));
            parseTree.filterCondition.Append(")");
            //Console.WriteLine("   AndExpr: {0}", parseTree.filterCondition.ToString());

            return parseTree;
        }

        public override AzSQLParseTree VisitOrExpr(AzSQLParser.OrExprContext context)
        {
            //Console.WriteLine("   Entering OrExpr");
            parseTree.filterCondition.Append("(");
            Visit(context.expr(0));
            parseTree.filterCondition.Append(" or ");
            Visit(context.expr(1));
            parseTree.filterCondition.Append(")");
            //Console.WriteLine("   OrExpr: {0}", parseTree.filterCondition.ToString());

            return parseTree;
        }

        public override AzSQLParseTree VisitParanthesizedExpr(AzSQLParser.ParanthesizedExprContext context)
        {
            //Console.WriteLine("   Entering ParanthesizedExpr");
            parseTree.filterCondition.Append("(");
            Visit(context.expr());
            parseTree.filterCondition.Append(")");
            //Console.WriteLine("   ParanthesizedExpr: {0}", parseTree.filterCondition.ToString());

            return parseTree;
        }

        public string OpToString(string op)
        {
            switch (op)
            {
                case "<" : return " lt ";
                case "<=": return " le ";
                case ">" : return " gt ";
                case ">=": return " ge ";
                case "=" : return " eq ";
                case "==": return " eq ";
                case "!=": return " ne ";
                case "<>": return " ne ";
                default: return " ";         // should really be exeption, but we should never get here since parser will fail beforehand on bad grammar
            }
        }
    }

    class ParsingErrorListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            // This is a variant of BailErrorStrategy, where parser throws the first time it enters SyntaxError method.
            // However, I prefer this because SytaxError lets us collect much more information for the exception, than 
            // just the RecognitionException in ErrorStategy classes. I want character position and message encapsulated
            // in the exception
            throw new AzSQLParseException(msg, line, charPositionInLine);
        }
    }

    class AzSQLParseException : Exception
    {
        public readonly int line, charPosition;
        public AzSQLParseException(string msg, int line, int charPosition) : base(msg)
        {
            this.line = line;
            this.charPosition = charPosition;
        }
    }

}
