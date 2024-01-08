//The main class to evaluate the Lisp Sexpressions
//Uses the Sexpr class to evaluate the Literal, Grouping, Binary and Unary Sexpressions
    //re-did this all to allow for more flexibility per function in the interpreter

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;


//        if (Match(TokenType.ANDQ, TokenType.ORQ, TokenType.EQQ, TokenType.EQUAL, TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.LESS, TokenType.GREATER, TokenType.CONS)){
//        if (Match(TokenType.CAR, TokenType.ATOMQ, TokenType.CDR, TokenType.SYMBOLQ, TokenType.NILQ, TokenType.LISTQ, TokenType.NUMQ, TokenType.NOTQ)){


namespace Lisp{
    
    public class Interpreter : Sexpr.ISexprVisitor<object>{

        public Environ globalEnviron = new Environ();        
        string[] LispCallableFunctions = {"define", "car", "cdr", "cons", "cond", "number?", "symbol?", "atom?", "nil?", "eq?", "and?", "or?", 
                                          "not?", "+", "-", "*", "/", ">", "<", "set", "list?", "list"};
        ILispCallableFunction[] callables = {new ILispCallableFunction.DEFINE(), new ILispCallableFunction.CAR(), new ILispCallableFunction.CDR(), 
                                            new ILispCallableFunction.CONS(), new ILispCallableFunction.COND(), new ILispCallableFunction.NUMBERQ(), 
                                            new ILispCallableFunction.SYMBOLQ(), new ILispCallableFunction.ATOMQ(), new ILispCallableFunction.NILQ(),
                                            new ILispCallableFunction.EQQ(), new ILispCallableFunction.ANDQ(), new ILispCallableFunction.ORQ(),
                                            new ILispCallableFunction.NOTQ(), new ILispCallableFunction.PLUS(), new ILispCallableFunction.SUB(),
                                            new ILispCallableFunction.STAR(), new ILispCallableFunction.SLASH(), new ILispCallableFunction.GREATER(),
                                            new ILispCallableFunction.LESS(), new ILispCallableFunction.SET(), new ILispCallableFunction.LISTQ(), new ILispCallableFunction.LIST()};
        public Environ environment;
        private readonly Dictionary<Sexpr, int> locals = new Dictionary<Sexpr, int>();
        private readonly List<Tuple<string, ILispCallableFunction>> functionCalls = new List<Tuple<string, ILispCallableFunction>>();


        //create the tuple of strings and corresponding callable functions
        //define each one globally
        public Interpreter(){
            environment=globalEnviron;  
            //create list of tuples for each function and its corresponding callable class
            for (int i=0; i<LispCallableFunctions.Count(); i++){
                var tmp = new Tuple<string, ILispCallableFunction>(LispCallableFunctions[i], callables[i]);
                functionCalls.Add(tmp);
            }
            //define each callable function on the global environment so the user can call them in any lisp program
            foreach (Tuple<string, ILispCallableFunction> tuple in functionCalls){
                globalEnviron.Define(tuple.Item1, tuple.Item2);
            }

        }

        
        public void Interpret(List<Sexpr> sexprs){
            try{
                foreach (Sexpr sexpr in sexprs){
                    try{
                        Console.WriteLine(Stringify(Evaluate(sexpr)));
                    }
                    catch (Exception e){
                        //Console.WriteLine(e.Message);
                    }
                }
            }
            catch (RuntimeError error){
                Lisp.RuntimeError(error);
            }
            }


        // public object VisitAssignSexpr(Sexpr.Assign Sexpr){
        //     object value = Evaluate(Sexpr.value);
        //     int distance = locals[Sexpr];
        //     return value;
        // }

        private bool IsOperation(Token token){
            TokenType[] operations = {TokenType.PLUS, TokenType.MINUS, TokenType.SLASH, TokenType.STAR, TokenType.EQUAL, TokenType.GREATER, TokenType.LESS};
            if (operations.Contains(token.type)){
                return true;
            }
            return false;
        }
            
        //called by Evaluate() if needed to eval an atom expression
        //now that everything is list or atom, we only need these two visit funcs
        //check if we have a symbol/arithmetic op as the atom in our current environ of vars, return its stored value
            //returning the value essentially evaluates the atom
        public object VisitAtomSexpr(Sexpr.Atom sexpr){
            if (sexpr.value is Token token && ((token.type == TokenType.SYMBOL) || IsOperation(token))){
                //Console.WriteLine("trying to get");
                try{
                    object ob = environment.Get(token);
                    // Console.WriteLine("back from get");
                    // Console.WriteLine(ob);
                    return ob;
                }
                catch (Exception e){
                    //return sexpr.value;
                    throw new RuntimeError(e.Message);
                    //environment.Define(token.lexeme, sexpr.value);
                    //Console.WriteLine("defined");
                    //object ob = environment.Get(token);
                    //Console.WriteLine("returning");
                    //return ob;
                }
                // if (ob==null) {
                //     Console.WriteLine("null ob");
                // }
                // else{
                //     if (ob is List<object> list){
                //         Console.WriteLine(list[0] + " 108 int");
                //     }
                    
                // }
                //return ob;
            }
            // return sexpr.value;
            //Console.WriteLine(sexpr.value);
            else{
                return sexpr.value;
            }
        }

        public object VisitListSexpr(Sexpr.List sexpr){
            //empty list, nil
            if (sexpr.values.Count==0){
                return null;
            }
            //non nil list
            else{
                //get the evaluated function of the list
                //the first element of the non nil list will be a LispCallable/LispFunction
                object func = Evaluate(sexpr.values[0]);
                //Console.WriteLine(func);
                //move this to each individual function case
                //can't generalize the objects per function
                // List<object> listVals = new List<object>();
                // foreach (Sexpr expr in sexpr.values){
                //     listVals.Add(Evaluate(expr));
                // }
                //if the first element in our list is a function, car, cdr, define, set, cons, etc
                //evaluating the first element will return a callable function that has been defined (need to define them globally)
                //we make sure it is a callable func, then call it's corresponding function Call() method to interpret each function

                //check for each different type of callable function
                if (func is ILispCallableFunction.PLUS plus){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return plus.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.DEFINE define){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return define.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.ANDQ andq){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return andq.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.ATOMQ atomq){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return atomq.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.CAR car){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return car.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.CDR cdr){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return cdr.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.COND cond){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return cond.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.CONS cons){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return cons.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.EQQ eq){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return eq.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.GREATER g){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return g.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.LESS less){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return less.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.LISTQ lis){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return lis.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.LIST l){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return l.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.NILQ n){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return n.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.NOTQ not){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return not.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.NUMBERQ num){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return num.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.ORQ o){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return o.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.SET set){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return set.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.SLASH slash){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return slash.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.STAR star){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return star.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.SUB sub){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return sub.Call(this, funcTail);
                }
                else if (func is ILispCallableFunction.SYMBOLQ symbol){
                    //check number of args?
                    List<Sexpr> funcTail = new List<Sexpr>();
                    //get the args and body of the list/function, as the first element is always the name which we already evaluated
                    funcTail = sexpr.values.Skip(1).ToList();
                    //call the function/first elem in the list with the rest of the list -> serving as func args
                    //also pass current interpreter to keep track of environments in function calls
                    return symbol.Call(this, funcTail);
                }
                else if (func is LispFunction fun){
                    List<Sexpr> funcTail = new List<Sexpr>();
                    funcTail = sexpr.values.Skip(1).ToList();
                    return fun.Call(this, funcTail);
                }
                else {
                    //Console.WriteLine("first element was not a lisp callable function");
                    //Console.WriteLine(func);
                    throw new RuntimeError("first elem was not a callable function");
                }
            }
        }

        
        //handles binary Sexpressions such as + (addition), - (subtraction), / (division) and * (multiplication), logical comparison (<, >)
        // public object VisitBinarySexpr(Sexpr.Binary Sexpr){

        //     object left = Evaluate(Sexpr.left);
        //     object right = Evaluate(Sexpr.right);

        //     switch (Sexpr.op.type){
        //         case TokenType.GREATER:
        //             CheckNumberOPs(Sexpr.op, left, right);
        //             if ((double)left > (double)right){
        //                 return true;
        //             }
        //             else{
        //                 return null;
        //             }
        //         case TokenType.LESS:
        //             CheckNumberOPs(Sexpr.op, left, right);
        //             if ((double)left < (double)right){
        //                 return true;
        //             }
        //             else{
        //                 return null;
        //             }
        //         //accounts for the = assignment, will return nil if objects are not equal
        //         case TokenType.EQUAL:
        //             if (IsEqual(left, right)){
        //                 return true;
        //             }
        //             else{
        //                 return null;
        //             }
        //         case TokenType.MINUS:
        //             CheckNumberOPs(Sexpr.op, left, right);
        //             return (double)left - (double)right;
        //         //special case to handle for string concatenation as well as arithmetic Sexpression
        //         case TokenType.PLUS:
        //             if (left is double && right is double){
        //                 return (double)left + (double)right;
        //             }
        //             else{
        //                 throw new RuntimeError(Sexpr.op, "Operands must be two integers. ");
        //             }
        //         case TokenType.SLASH:
        //             CheckNumberOPs(Sexpr.op, left, right);
        //             return (double)left / (double)right;
        //         case TokenType.STAR:
        //             CheckNumberOPs(Sexpr.op, left, right);
        //             return (double)left * (double)right;
        //         case TokenType.ANDQ:
        //             if (IsTruthy(left) && IsTruthy(right)){
        //                 return true;
        //             }
        //             else{
        //                 return null;
        //             }
        //         case TokenType.ORQ:
        //             if (IsTruthy(left) || IsTruthy(right)){
        //                 return true;
        //             }
        //             else{
        //                 return null;
        //             }
        //         //FIXME - check this code, might not be right for all symbols/cases
        //         case TokenType.EQQ:
        //             // if (SameSymbol(left, right)){
        //             //     return true;
        //             // }
        //             // else{
        //             //     return null;
        //             // }
        //             if (IsEqual(left, right)){
        //                 return true;
        //             }
        //             else{
        //                 return null;
        //             }
        //         //if the right object is already a list
        //             //convert the left obj to a list and add it to front of right list
        //         case TokenType.CONS:
        //             if (right is List<object> list){
        //                 return list.Prepend(left).ToList();
        //             }
        //             //if neither item is a list, create a new list with left and right objects
        //             else{
        //                 return new List<object>(){left, right};
        //             }
        //     }
        //     return null;
        // }
        public bool SameSymbol(object left, object right){
            if (left is Token token && right is Token tok){
                if (token.type == TokenType.SYMBOL && tok.type == TokenType.SYMBOL){
                    if (token.lexeme.Equals(tok.lexeme)){
                        return true;
                    }
                }
            }
            else{
                return false;
            }
            return false;
        }

        //use callable?
        // public object VisitCondSexpr(Sexpr.Cond sexpr){
        //     // foreach (Tuple<Sexpr, Sexpr> condpair in sexpr.conditions){
        //     //     object obj = Evaluate(condpair.Item1);
        //     //     if (IsTruthy(condpair)){
        //     //         return Evaluate(condpair.Item2);
        //     //     }
        //     // }
        //     // throw new RuntimeError(sexpr.op, "no condition in cond expression is truthy");
        //     return null;
        // }


        //use callable?
        // public object VisitDefineSexpr(Sexpr.Define sexpr){
        //     // LispFunction function = new LispFunction(sexpr.args, sexpr.body);
        //     // environment.Define(sexpr.name.lexeme, function);
        //     // return null;
        //     //null list, null func dec
        //     Console.WriteLine("in visitor 230 int");
        //     if (sexpr.args.Count==0){
        //         return null;
        //     }

        //     //if the first element in our define sexpr is the 'define' callable keyword
        //     //call the function w current interpreter/environ and the rest of the list holding the args and body
        //     if (sexpr.args[0] is Sexpr.Atom atom){
        //         //Console.WriteLine(atom.value);
        //     }
        //     //Console.WriteLine(sexpr.args[0]);
        //     //object ob = Evaluate(sexpr.args[0]);
        // //     if (ob==null){
        // //         Console.WriteLine("null ob");
        // // }
        //     ILispCallable define = new DEFINE();
        //     return define.Call(this, sexpr.args.ToList());

        //     // if (ob is ILispCallable func){
        //     //     if (func.Arity()==3){
        //     //         return define.Call(this, sexpr.args.Skip(1).ToList());
        //     //     }
        //     //     else{
        //     //         throw new RuntimeError("wrong number of args for defining a function");
        //     //     }
        //     // }
        //     // else{
        //     //     throw new RuntimeError("not a callable function in your function declaration");
        //     // }
            
        // }

        // public object VisitSetSexpr(Sexpr.Set sexpr){
        //     object value = Evaluate(sexpr.value);
        //     environment.Define(sexpr.name.lexeme, value);
        //     return value;
        // }

        //will now do the work for all of the old individual visit functions   
        //since we are passed a list of Sexpressions that are either more lists or atoms
            //we can simply evaluate each one simply, don't have to worry about case/function
        

    //     public object VisitSetStmt(Sexpr.Set stmt){
    //         object value = Evaluate(stmt.value);
    //         environment.Define(stmt.name.lexeme, value);
    //         return value;
    // }

        // public object VisitSexpressionStmt(Stmt.Sexpression stmt){
        //     object obj = Evaluate(stmt.expr);
        //     Console.WriteLine(Stringify(obj));
        //     return null;
        // }

        // public object VisitUnarySexpr(Sexpr.Unary sexpr){
        //     object right = Evaluate(sexpr.right);
        //     switch(sexpr.op.type){
        //         //return the first item in the right/only arg
        //         case TokenType.CAR:
        //             if (right is List<object> list){
        //                 return list[0];
        //             }
        //             else{
        //                 throw new RuntimeError(sexpr.op, "argument to car can only be a list");
        //             }
        //         case TokenType.CDR:
        //             if (right is List<object> lis){
        //                 return lis.Skip(1).ToList();
        //             }
        //             else{
        //                 throw new RuntimeError(sexpr.op, "argument to cdr can only be a list");
        //             }
        //         case TokenType.NILQ:
        //             if (right == null){
        //                 return true;
        //             }
        //             else{
        //                 return null;
        //             }
        //         //handles lists of elements and empty lists
        //         case TokenType.LISTQ:
        //             if (right is List<object> || right == null || sexpr.right is List<object>){
        //                 return true;
        //             }
        //             else{
        //                 return null;
        //             }
        //         case TokenType.NOTQ:
        //             if (!IsTruthy(right)){
        //                 return true;
        //             }
        //             else{
        //                 return null;
        //             }
        //         case TokenType.NUMQ:
        //             if (sexpr.right is double || right is double) return true;
        //             else{
        //                 return null;
        //             }
        //         case TokenType.SYMBOLQ:
        //             if (IsSymbol(sexpr.right)) return true;
        //             else{
        //                 return null;
        //             }
               
        //     }
        //     return null;
        // }

        public bool IsSymbol(object ob){
            if (ob is Sexpr.Atom atom && atom.value is Token token){
                if (token.type == TokenType.SYMBOL){
                    return true;
                }
                else{
                    return false;
                }
            }
            return false;
        }

        public object Evaluate(Sexpr sexpr){
            return sexpr.Accept(this);
        }

        // private void Execute(Stmt stmt){
        //     stmt.Accept(this);
        // }

        // public void ExecuteBlock(Sexpr sexpr, Environ environment){
        //     Environ previous = this.environment;
        //     try{
        //         this.environment = environment;

        //         Evaluate(sexpr);
        //     }
        //     finally{
        //         this.environment=previous;
        //     }
        // }

        // public void Resolve(Sexpr Sexpr, int depth){
        //     locals.Add(Sexpr, depth);
        // }

        // public void ExecuteBlock(List<Stmt> statements, Environ environment){
        //     Environ prev = this.environment;
        //     try{
        //         this.environment = environment;

        //         foreach (Stmt stmt in statements){
        //             Execute(stmt);
        //         }
        //     }
        //     //reset environment back to original after executing the block of statements 
        //     finally{
        //         this.environment=prev;
        //     }
        // }

        //private object LookUpVariable(Token name, Sexpr Sexpr){
            // if (locals.TryGetValue(Sexpr, out int distance)){
            //     return environment.GetAt(distance, name.lexeme);
            // }

            // else{
            //     return globals.Get(name);
            // }

            //return locals[Sexpr];

            // int distance = locals[Sexpr];
            // if (distance!=null){
            //     return environment.GetAt(distance, name.lexeme);
            // }
            // else{
            //     return globals.Get(name);
            // }
        //}

       

        //helper function to make sure the passed operator (-, /, *) is applied to a valid term in an Sexpression
        private void CheckNumberOP(Token op, object operand){
            if (operand is double) return;
            else{
                throw new RuntimeError(op, "Operand must be a number ");
            }
        }

        //similar to above function, checks validity for binary operations 
        private void CheckNumberOPs(Token op, object left, object right){
            if ((left is double && right is double)){
                return;
            }
            else{
                throw new RuntimeError(op, "Operands must be numbers ");
            }
        }

        public bool IsEven(int num){
            if ((num % 2)==0){
                return true;
            }
            else{
                return false;
            }
        }

        //false & null are "falsey", everything else is "truthy"
        public bool IsTruthy(object ob){
            if (ob==null) return false;
            if (ob is bool) return (bool)ob;
            return true;
        }

        //helper function to determine if two objects in an Sexpression are equal
        //used to help evaluate != and = Sexpressions
        public bool IsEqual(object a, object b){
            if (a==null && b==null) return true;
            else if (a==null) return false;
            else if (a is List<object> || b is List<object>){
                return false;
            }
            else{
                return a.Equals(b);
            }
        }

        //converts the result of our Sexpression to a string value/readable output
        //handles converting lists to dotted notation

        public string Stringify(object ob){
            if (ob==null) return "()";

            else if (ob is List<object> expr){
                string str = "(";
                for (int i=0; i<expr.Count; i++){
                    //Console.WriteLine(expr[i]);
                    // if (i==expr.Count-1){
                    //     str+=".";
                    // }
                    if (expr[i]==null){
                        str += "()";
                    }
                    // if we have list of sublists
                    //     recurse to flatten entire list into single elements
                    if (expr[i] is List<object>){
                        str+=Stringify(expr[i]);
                    }
                    //convert each list item to a string, add to our output str
                    else{
                        str += expr[i].ToString();
                    }

                    if (expr.Last()==null && i == expr.Count-2){
                        break;
                    }

                    else if (i!=expr.Count-1){
                        str+=" ";
                    }
                }
                str+=")";
                return str;
            }

            else if (ob is double){
                string text = ob.ToString();
                if (text.EndsWith(".0")){
                    text = text.Substring(0, text.Length-2);
                }
                return text;
            }
            //'t' for true, nil list for false
            else if (ob is bool){
                if ((bool)ob == true){
                    return "t";
                }
                else{
                    return "()";
                }
            }
            else{
                return ob.ToString();
            }
        }



    }

}