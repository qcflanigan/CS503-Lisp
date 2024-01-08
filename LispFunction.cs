using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

//use to handle the define declarations, creating new functions for lisp lang

namespace Lisp{

    public class LispFunction : ILispCallableFunction{
        //private readonly Stmt.Function declaration;
        // private readonly Environ closure;
        // private readonly bool isInit;
        private readonly List<Token> args;
        private readonly Sexpr funcBody;

        public LispFunction(List<Token> args, Sexpr funcBody){
            this.args = args;
            this.funcBody = funcBody;
            
        }

    //     public LispFunction Bind(LispInstance instance){
    //         Environ environment = new Environ(closure);
    //         environment.Define("this", instance);
    //         return new LispFunction(declaration, environment, isInit);
    // }


        // public override string ToString()
        // {
        //     return "<fn " + declaration.name.lexeme + ">";
        // }

        public int Arity(){
            return args.Count;
        }

        //handle the call to each declared function
        //will take in a given list of arguments and use the interpreter to evaluate each one, defining it in the current environment
        public object Call(Interpreter interpreter, List<Sexpr> arguments){
            Environ environ = new(interpreter.globalEnviron);
            
            for (int i=0; i<args.Count; i++){
                //Console.WriteLine("46, " + args[i].lexeme);
                environ.Define(args[i].lexeme, interpreter.Evaluate(arguments[i]));
            }

            //interpreter.ExecuteBlock(declaration.body, environment);
            //try to execute the func block, then catch the return exceptions for return statements
            //set new environment to that of the function, evaluate the body of the function using passed interpreter
            Environ currEnviron = interpreter.environment;
            object returnVal = null;
            try{
                interpreter.environment = environ;
                returnVal = interpreter.Evaluate(funcBody);
            }
            finally{
                //reset the environment back to current interpreter environ after setting the temp environment for the func evaluation
                interpreter.environment = currEnviron;
            }
            return returnVal;
            // catch (Return returnVal){
            //     if (isInit){
            //         return closure.GetAt(0, "this");
            //     }
            //     return returnVal.value;
            // }
            // if (isInit) return closure.GetAt(0, "this");
            // return null;
        }


    }
}