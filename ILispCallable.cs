using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

//mirrors the Sexpr : visitor structure
//when the interpreter evaluates a function call as the first element in the list
    //the VisitListExpr will check if the element is evaluated to be a callable function 
    //it will then check which type of callable (define, car, plus, etc) and call the corresponding Call() method
    //this Call() then handles the actual interpretation for each function


namespace Lisp{

public class ILispCallableFunction{
    //the expected number of args for each func
    public int Arity(){
        return 0;
    }
    public object Call(Interpreter interpreter, List<Sexpr> args){
        return null;
    }

    public interface ILispCallable{
        public int Arity();
        public object Call(Interpreter interpreter, List<Sexpr> arguments);
    }

    //create new function on define call
    //pass the args and body/sexprs of the function to the LispFunc class
    //the Lisp Function class evaluates the functions itself, handles environment switching/enclosing
    public class DEFINE : ILispCallableFunction{
        public int Arity()
        {
            return 3;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            // make sure the func name is a symbol in the right place in our args list
            if (args[0] is Sexpr.Atom atom && atom.value is Token token && token.type == TokenType.SYMBOL)
            
                    //if we have no parameters
                    //create new instance of a function in current environ with new empty list and the function body
                    if (args[1] == null){
                        interpreter.environment.Define(token.lexeme, new LispFunction(new List<Token>(), args[2]));

                        // Return to eval loop
                        throw new Return(null);
                    }
                    //if we have some params for the function as a non-empty list
                    else if (args[1] is Sexpr.List list){
                        List<Token> funcArgs = new();
                        foreach (Sexpr sexpr in list.values)
                        {
                            // grab each argument and add to our list of args to init a new Lisp Function
                            //functions args have to be symbols/identifiers
                            if (sexpr is Sexpr.Atom atoms && atoms.value is Token tok && tok.type == TokenType.SYMBOL)
                            {
                                funcArgs.Add(tok);
                            }
                            else
                            {
                                throw new RuntimeError("Name of function argument must be a symbol.");
                            }
                        }
                        //creating instance of a new function in the current environment
                        //pass the args and the third item in list, which will be the body of the functions
                        interpreter.environment.Define(token.lexeme, new LispFunction(funcArgs, args[2]));

                        // use the return exception to return back to the main environment
                        throw new Return(null);
                    }
                    else
                    {
                        throw new RuntimeError("Expected list of function args.");
                    }
            
                else
                {
                    throw new RuntimeError("Name of function must be a symbol.");
                }
            }
    }

    //check for diff values, return first one
    //not value-indep, search diff types of lists & types
    public class CAR: ILispCallableFunction{
        public int Arity(){
            return 1;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args){
            object right = interpreter.Evaluate(args[0]);

            if (right is List<object> list){
                return list[0];
            }
            // Unevaluated s-expression
            else if (right is Sexpr.List sexprlist && sexprlist.values.Count != 0){
                object ob = sexprlist.values[0];
                // Nil is self-evaluating
                if (ob is Sexpr.List lis2 && lis2.values.Count == 0){
                    return null;
                }
                // Basic forms are self-evaluating
                else if (ob is Sexpr.Atom atom){
                    if (atom.value is double){
                        return (double)atom.value;
                    }
                    else if (atom.value is string){
                        return (string)atom.value;
                    }
                    else{
                        return ob;
                    }
                }
                else{
                    return ob;
                }
            }
            else{
                throw new RuntimeError($"Operand must be a list.");

            }
        }
    }

    //check for diff values, return tail of list
    //not value-indep, search diff types of lists & types
    public class CDR: ILispCallableFunction{
        public int Arity(){
            return 1;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args){
            //evaluate the right handside expression/arg to cdr, make sure it is a list of some kind
            //can either be list<ob> or list<sexpr>
            object right = interpreter.Evaluate(args[0]);

            if (right is List<object> list){
                if (list.Count()==2){
                    //should be nil signifying empty list
                    //prevents returning (())
                    return list[1];
                }
                //if more than just (2 ()), return everything except the first value
                else{
                    return list.Skip(1).ToList();
                }
            }
            //if not just a list of values, and is a list of sexprs
            //return a new sexpr list w all tail elements
            else if (right is Sexpr.List sexprList && sexprList.values.Count()>0){
                List<Sexpr> tmp = sexprList.values.Skip(1).ToList();
                //if the new tail list has no elements it is empty list ()
                if (tmp.Count()==0){
                    return null;
                }
                else{
                    return new Sexpr.List(tmp);
                }
            }
            else{
                throw new RuntimeError("Arguments to cdr can only be lists");
            }

            }
    }

    public class CONS: ILispCallableFunction{
        public int Arity(){
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args){
            object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[1]);
            //cons (8 (9 4))
            if (right is List<object> list){
                return list.Prepend(left).ToList();
            }
            else{
                //cons (9 ())
                return new List<object>(){left, right};
            }

            }
        
    }

    public class COND: ILispCallableFunction{
        public int Arity()
        {
            //arbitrary amount of args
            //try -1?
            return -1;

        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            if (!interpreter.IsEven(args.Count)){
                throw new RuntimeError("Need even number of expressions");
            }
            else{
                for (int i=0; i<args.Count; i++){
                    object ob = interpreter.Evaluate(args[i]);
                    //if the conditional statement evaluates to true
                        //evaluate the next statement of the pair 
                    if (interpreter.IsTruthy(ob)){
                        return interpreter.Evaluate(args[i+1]);
                    }
                    //iterate twice per cond sexpr since we're dealing in pairs
                    i++;
                }
            }
            throw new RuntimeError("incorrect cond sexpr syntax");
            
            }
    }

    public class NUMBERQ: ILispCallableFunction{
        public int Arity()
        {
            return 1;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object right = interpreter.Evaluate(args[0]);
            if (right is double){
                return true;
            }
            else{
                return false;
            }

            }
        
    }

    public class SYMBOLQ: ILispCallableFunction{
        public int Arity()
        {
            return 1;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object right = interpreter.Evaluate(args[0]);
            if (interpreter.IsSymbol(right)){
                return true;
            }
            else{
                return false;
            }
          
            }
        
    }

    public class ATOMQ: ILispCallableFunction{
        public int Arity()
        {
            return 1;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object right = interpreter.Evaluate(args[0]);
            //might need to evaluate and check for tokentype/symbol??
            //could be too inclusive, check with testing
            if (args[0] is Sexpr.Atom || right is double || right is string || interpreter.IsSymbol(right)){
                return true;
            }
            else{
                return null;
            }
           

            }
        
    }

    public class NILQ: ILispCallableFunction{
        public int Arity()
        {
            return 1;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object right = interpreter.Evaluate(args[0]);
            if (right==null){
                return true;
            }
            else{
                return null;
            }

            }
        
    }

     public class EQQ: ILispCallableFunction{
        public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[1]);
            if (interpreter.IsEqual(left, right)){
                return true;
            }
            else{
                return false;
            }

            }
        
    }

    public class ANDQ: ILispCallableFunction{
        public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[1]);
            if (interpreter.IsTruthy(left) && interpreter.IsTruthy(right)){
                return true;
            }
            else{
                return null;
            }

            }
    }

     public class ORQ: ILispCallableFunction{
        public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[1]);
            if (interpreter.IsTruthy(left) || interpreter.IsTruthy(right)){
                return true;
            }
            else{
                return null;
            }
        }
    }

     public class NOTQ: ILispCallableFunction{
        public int Arity()
        {
            return 1;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            //object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[0]);
            if (!interpreter.IsTruthy(right)){
                return true;
            }
            else{
                return null;
            }

            }
    }

     public class PLUS: ILispCallableFunction{
        public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[1]);
            if (left is double l && right is double r){
                return l+r;
            }
            else{
                throw new RuntimeError("Expecting only numbers for addition");
            }
            }
    }

     public class SUB: ILispCallableFunction{
        
         public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[1]);
            if (left is double l && right is double r){
                return (l-r);
            }
            else{
                throw new RuntimeError("Expecting only numbers for subtraction");
            }
    }}

     public class STAR: ILispCallableFunction{
         public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[1]);
            if (left is double l && right is double r){
                return (l*r);
            }
            else{
                throw new RuntimeError("Expecting only numbers for multiplication");
            }
    }}
    
     public class SLASH: ILispCallableFunction{
         public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[1]);
            if (left is double l && right is double r){
                if (r==0){
                    throw new RuntimeError("Cannot divide by zero");
                }
                else{
                return (l/r);
                }
            }
            else{
                throw new RuntimeError("Expecting only numbers for division");
            }
    }}

    public class GREATER: ILispCallableFunction{
         public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            // object left = interpreter.Evaluate(args[0]);
            // //Console.WriteLine(args[0]);
            // Console.WriteLine(left);
            // object right = interpreter.Evaluate(args[1]);
            // //Console.WriteLine(args[1]);
            // Console.WriteLine(right);
            // left=(double)1;
            // if (left is double l && right is double r){
            //     if (l > r){
            //         return true;
            //     }
            //     else{
            //         return null;
            //     }
            // }
            // else{
            //     throw new RuntimeError("Expecting only numbers for >");
            // }
            double[] d = new double[2];

            for (int i = 0; i < 2; i++)
            {
                object arg = interpreter.Evaluate(args[i]);
                if (arg is not double)
                {
                    throw new RuntimeError($"Operand '{interpreter.Stringify(arg)}' is not a number.");
                }
                d[i] = (double)arg;
            }

            return d[0] > d[1] ? true : null;
        }
    }
    

    public class LESS: ILispCallableFunction{
        public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object left = interpreter.Evaluate(args[0]);
            object right = interpreter.Evaluate(args[1]);
            if (left is double l && right is double r){
                if (l < r){
                    return true;
                }
                else{
                    return null;
                }
            }
            else{
                throw new RuntimeError("Expecting only numbers for <");
            }
    }
    }

    public class SET: ILispCallableFunction{
        public int Arity()
        {
            return 2;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            //get var name within the set function
            //object symbol = interpreter.Evaluate(args[0]);
            //evaluate the expression we are setting our variable to
                //can eithe be an atom  or expression within a list
            object value = interpreter.Evaluate(args[1]);

            if (args[0] is Sexpr.Atom atom){
                if (atom.value is Token token){
                    if (token.type==TokenType.SYMBOL){
                        interpreter.environment.Define(token.lexeme, value);
                        throw new EmptyReturn();
                    }
                    else{
                        throw new RuntimeError("Variable names for set must be symbols");
                    }
                }
            }
            throw new RuntimeError("Variables in set must be atoms");


    }
        
    }

    //check if a variable is a list
     public class LISTQ: ILispCallableFunction{
        public int Arity()
        {
            return 1;
        }

        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            object value = interpreter.Evaluate(args[0]);
            //Console.WriteLine(value);
            //lists can be lists of sexprs, objects/atoms, or an empty list ()
            if (value is Sexpr.List || value==null || value is List<object>){
                return true;
            }
            else{
                return null;
            }
            //throw new RuntimeError("Variables in set must be atoms");

    }
    }


    public class LIST: ILispCallableFunction{
        public int Arity()
        {
            return 1;
        }
        //create a new list for a variable
        public object Call(Interpreter interpreter, List<Sexpr> args)
        {
            List<object> list = new List<object>();
            foreach (Sexpr sexpr in args){
                list.Add(interpreter.Evaluate(sexpr));
            }
            //nil, empty list
            if (list.Count()==0){
                return null;
            }
            //not empty, add the () character at the end as the terminator
            else{
                list.Add(null);
                return list;
            }
            //throw new RuntimeError("Variables in set must be atoms");

    }
    }


}
}