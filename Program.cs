using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace Lisp
{
    public class Lisp{
        //private static readonly Interpreter interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;

        public static void Main(string[] args){
            //if we are given a file to execute
            if (args.Length == 1){
                ExecuteFile(args[0]);
            }
            else if (args.Length > 1){
                Console.WriteLine("Usage: Lisp [script]");
                Environment.Exit(64);   //exit with code 64 signifying invalid input
            }
            //if we are given no file, execute line-by-line
            else{
                ExecutePrompt();
             
            }
        }

        //function to interpret/execute a given file of Lisp code
        //reads Lisp code into array of type bytes, and converts the bytes to strings, passing the strings to the Run() function
        private static void ExecuteFile(String path){
            byte[] bytes = File.ReadAllBytes(path);
            ExecuteLoxCode(Encoding.Default.GetString(bytes));
            if (hadError){
                Environment.Exit(65);
            }
            if (hadRuntimeError){
                Environment.Exit(70);
            }
        }

        //function to run Lisp code line-by-line within while loop
        //provides a basic terminal-like interface
        private static void ExecutePrompt(){
            StreamReader reader = new StreamReader(Console.OpenStandardInput());
                while (true){
                    Console.Write("Lisp> ");
                    string input = reader.ReadLine();
                    if (input == null) break;
                    ExecuteLoxCode(input);
                    hadError = false;
                }
            }


        //main driver function that executes the Lisp code by scanning the input for each meaningful token
        //for now, just prints the tokens we are given in our input to see if we are processing input correctly
        private static void ExecuteLoxCode(string input){
            Scanner scanner = new Scanner(input);
            List<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);
            List<Sexpr> sexprs = parser.Parse();
            //Expr expression = parser.Parse();
            if (hadError) return;
            //resolve after error check, will not run code if there are syntax errors
            //Resolver resolver = new Resolver(interpreter);
            //resolver.Resolve(statements);

            //check again for any resolving errors between scopes
            //if (hadError) return;
            Interpreter interpreter = new Interpreter();
            interpreter.Interpret(sexprs);
            //Console.WriteLine(new ASTPrinter().Print(expression));

            // foreach (Token token in tokens){
            //     Console.WriteLine("token text: " + token.lexeme + " token type: " + token.type);
            // }
        }

        //function to notify user of invalid code/errors in code
        //returns the line number of the error and a message explaining there was an error
        public static void Error(int line, string message){
            Report(line, "", message);
        }

        private static void Report(int line, string location, string message){
            Console.Error.WriteLine("[line " + line + "] Error" + location + ": " + message);
            hadError = true;
        }

        public static void TokenError(Token token, string msg){
            if (token.type == TokenType.EOF){
                Report(token.line, " at end", msg);
            }
            else {
                Report(token.line, " at '" + token.lexeme + "'", msg);
            }
        }

        public static void RuntimeError(RuntimeError error){
            Console.WriteLine(error.Message + "Line number: " + error.tokenT.line);
            hadRuntimeError = true;
        }

        }
}  
