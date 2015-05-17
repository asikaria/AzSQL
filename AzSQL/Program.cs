using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzSQL
{
    class Program
    {
        static string acctName, acctKey;
        static void Main(string[] args)
        {

            if (args.Length == 2)
            {
                acctName = args[0];
                acctKey = args[1];
                repl();
            }
            else if (args.Length == 1 && args[0].Trim().ToLower() == "/test")
            {
                RunParserTests();
            }
            else
            {
                Console.WriteLine("Usage: AzTableQuery <StorageAcctName> <StorageAcctKey>");
            }
        }


        private static void repl()    // R-E-P-Loop
        {
            StorageCredentials creds = new StorageCredentials(acctName, acctKey);
            string input;
            string prompt = "> ";

            Console.Write("{0}", prompt); input = Console.ReadLine();
            while (input.ToLower() != "quit" && input.ToLower() != "exit")
            {
                if (input.Trim() != "")
                {
                    try
                    {
                        IEnumerable<DynamicTableEntity> results = AzSQL.RunQuery(input, creds);
                        AzTableQuery.RenderResults(results);
                    }
                    catch (AzSQLParseException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Syntax Eror at line {0}, position {1}: {2}", ex.line, ex.charPosition, ex.Message);
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Exception {0}: {1}", ex.GetType().FullName, ex.Message);
                        Console.ResetColor();
                    }
                    finally
                    {
                        Console.WriteLine();
                    }
                }
                Console.Write("{0}", prompt); input = Console.ReadLine();
            }
        }


        private static void RunParserTests()
        {
            //TODO: These tests just print the parsed expression - it would be good to actually compare automatically to expected values.
            parserTester(@"select a,b,c from tabfoo where a='fft' and not b==-3 or g=true or m='hggf'");
            parserTester(@"select * from tabfoo");
            parserTester(@"select a,b,c from tabfoo where not b==-3");

            //error cases - should have parse errors
            parserTester(@"select *,a,b,c from tabfoo where not b==-3");
            parserTester(@"select a,b,c where not b==-3");
            parserTester(@"select from tabfoo where not b==-3");
            parserTester(@"select a,b,c from tabfoo where b eq -3");

            //interestting case
            // statement technically terminates at 'tabfoo', since that is the longest match possible.
            // TODO: it is better to modify grammar to actually fail at superfluous input: in this 
            //       case the 'not b==-3' is not superfluous, but ignored because 'where' is missing
            parserTester(@"select a,b,c from tabfoo not b==-3");  

        }

        private static void parserTester(string input)
        {
            Console.WriteLine("{0}", input);
            try
            {
                AzSQLParseTree parseTree = AzSQL.ParseAzSQL(input);
                Console.WriteLine("{0}", parseTree.filterCondition.ToString());
            }
            catch (AzSQLParseException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Syntax Eror at line {0}, position {1}: {2}", ex.line, ex.charPosition, ex.Message);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception {0}: {1}", ex.GetType().FullName, ex.Message);
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("----------------------");
                Console.WriteLine();
            }
        }

        private static void E2ETester(string input, StorageCredentials creds)
        {
            Console.WriteLine("{0}", input);
            IEnumerable<DynamicTableEntity> results = AzSQL.RunQuery(input, creds);
            AzTableQuery.RenderResults(results);
            Console.WriteLine("----------------------");
            Console.WriteLine();
        }


    }
}
