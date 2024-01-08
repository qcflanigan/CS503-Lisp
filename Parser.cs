using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using System.Xml;

//Parser is used to scan the tokens we have stored from Scanner and process each individually
//uses a List of tokens created from Scanner.cs

//Changing code to only parse lists and atoms, nothing else
//Scanner has just scanned in symbols, numbers, strings
//try parsing only lists (if we see left paren), everything else is an atom
    //then we can check if the car of list is a function/callable, cdr will be args and body evaluated separately

namespace Lisp{
class Parser{
    private readonly List<Token> tokens = new List<Token>();
    private int current = 0;
    public class ParseError : Exception{
        public ParseError() : base(){

        }
        public ParseError(string msg) : base(msg){}
    }

    public Parser(List<Token> tokens){
        this.tokens = tokens;
    }
        
    public List<Sexpr> Parse(){
        List<Sexpr> sepxrs = new List<Sexpr>();

        while (!IsAtEnd()){
            try{
                sepxrs.Add(Expression());
            }
            catch{
                Synchronize();
            }
        }
        return sepxrs;
    }

    //Sexpressions can either be lists (signaled by '(', or an atom, or nil)
    private Sexpr Expression(){
        if (Match(TokenType.LEFT_PAREN)){
            //Console.WriteLine(Peek());
            return HandleList();
        }
        // if (Match(TokenType.DEFINE)){
        //     return HandleDefine();
        // }
        //else, not a list so it will be an atom/ni;
        else{
        return HandleAtom();
        }
    }

    // private Stmt Statement(){
    //     if (Match(TokenType.LEFT_PAREN)){
    //         if (Match(TokenType.DEFINE)){
    //             return HandleDefine();
    //         }
    //         else if (Match(TokenType.SET)){
    //             return HandleSet();
    //         }
    //         else{
    //             return new Stmt.Sexpression(HandleList());
    //         }
    //     }
    //     //if we did not see a '(' character, we have an atom
    //     else{
    //         return new Stmt.Sexpression(HandleAtom());
    //     }
    //    }


    //now just parsing everything as a list OR atom
    //if we see a (, continue to add each following expression until we get to a ) terminating character
    private Sexpr HandleList(){

         List<Sexpr> defineVals = new List<Sexpr>();
        //int i=0;
        //Console.WriteLine(Peek());
        while(!Match(TokenType.RIGHT_PAREN)){
            //Console.WriteLine(Peek());
            defineVals.Add(Expression());
            //Console.WriteLine(defineVals[i]);
        }
        //Console.WriteLine(defineVals[0] + " " + defineVals[1]); //+ " " + defineVals[3]);
        return new Sexpr.List (defineVals);

        // if (IsBinaryFunction()){
        //     return HandleBinary();
        // }
        // else if (Match(TokenType.COND)){
        //     //return HandleCond();
        //     return null;
        // }
        // else if (Check(TokenType.DEFINE)){
        //     //throw Error(Previous(), "cannot define functions outside global environment");
        //     return HandleDefine();
        // }
        // else if (Match(TokenType.SET)){
        //     //throw Error(Previous(), "cannot use set to declare variables outside of global environment");
        //     return HandleSet();
        // }
        // else if (IsUnaryFunction()){
        //     return Unary();
        // }
        // else {
        //     return HandleValList();
        // }


    }

    // private Sexpr HandleList(){

    //     List<Sexpr> sexprs = new List<Sexpr>();
    //     //evaluate each token with the expr() func until we see ) signaling end of list
    //     while (!Match(TokenType.RIGHT_PAREN)){
    //         sexprs.Add(Expression());
    //     }
    //     //Consume(TokenType.RIGHT_PAREN, "Expecting ) after list");
    //     return new Sexpr.List(sexprs);
    // }

    private bool IsBinaryFunction(){
        if (Match(TokenType.ANDQ, TokenType.ORQ, TokenType.EQQ, TokenType.EQUAL, TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.LESS, TokenType.GREATER, TokenType.CONS)){
            return true;
        }
        return false;
    }
    private bool IsUnaryFunction(){
        if (Match(TokenType.CAR, TokenType.ATOMQ, TokenType.CDR, TokenType.SYMBOLQ, TokenType.NILQ, TokenType.LISTQ, TokenType.NUMQ, TokenType.NOTQ)){
            return true;
        }
        return false;
    }

    //if we see a num, symbol, or string, return an atom of that value
    //if not, it is not an atom so throw an error
    //everything is now a number, string or symbol/arithmetic op for atoms
    private Sexpr HandleAtom(){
        TokenType[] OptokenTypes = {TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.EQUAL, TokenType.GREATER, TokenType.LESS};

        if (Match(TokenType.NUMBER, TokenType.STRING)){
            return new Sexpr.Atom(Previous().literal);
        }
        //function names, set, cons, car, cdr, etc
        //these will be handled in interpreter now
        else if (Match(TokenType.SYMBOL)){
            return new Sexpr.Atom(Previous());
        }
        //+, -, /, *, >, <, =
        else if (Match(OptokenTypes)){
            return new Sexpr.Atom(Previous());
        }
        // else if (Match(tokenTypes)){
        //     return new Sexpr.Atom(Previous());
        // }
        // else if (Match(TokenType.NIL)){
        //     Console.WriteLine("nil");
        //     return new Sexpr.Atom(null);
        // }
        //'t, used as 'else' in cond statements
        else if (Match(TokenType.TRUE)){
            return new Sexpr.Atom(true);
        }
        // else if (Match(TokenType.DEFINE)){
        //     return HandleDefine();
        // }
        else{
            throw Error(Peek(), "Expected an atom");
        }
    }

    // private Sexpr HandleBinary(){
    //     Token op = Previous();
    //     Sexpr left = Expression();
    //     Sexpr right = Expression();
    //     Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression");
    //     return new Sexpr.Binary(left, op, right);
    // }

    // private Sexpr HandleCond(){
    //     Token op = Previous();

    //     List<Tuple<Sexpr, Sexpr>> conditions = new List<Tuple<Sexpr, Sexpr>>();
    
    //     //until we see the ')' character signaling an end to our current condition
    //         //continue creating pairs of conditions and add them to our tuples list
    //     while (!Match(TokenType.RIGHT_PAREN)){
    //         Tuple<Sexpr, Sexpr> condPair = new(Expression(), Expression());
    //         conditions.Add(condPair);
    //     }

    //     //Consume(TokenType.RIGHT_PAREN, "Expected ')' after condition");
    //     return new Sexpr.Cond(conditions, op);
    // }

    //handle function definitions/
    //(define funcName args body)
    // private Sexpr.Define HandleDefine(){
    //     // string kind = "function";
    //     // Token name = Consume(TokenType.SYMBOL, "Expect " + kind + " name.");
    //     // Consume(TokenType.LEFT_PAREN, "Expect ( after function name");
    //     // //use Consume() or HandleList() Recursion?
    //     // List<Token> args = new List<Token>();
    //     // if (!Check(TokenType.RIGHT_PAREN)){
    //     //     do{
    //     //         args.Add(Consume(TokenType.SYMBOL, "Expect parameter name"));
    //     //     } while (!Match(TokenType.RIGHT_PAREN));
    //     // }
    //     // //List<Sexpr>
    //     // //Consume(TokenType.RIGHT_PAREN, "Expecting a ) after function parameters");
    //     // Consume(TokenType.LEFT_PAREN, "Expecting ( before function body");
    //     // Sexpr body = HandleList();
    //     // // Console.WriteLine(args[0].lexeme);
    //     // // Console.WriteLine(body);
    //     // return new Sexpr.Define(name, args, body);   

    //     //will we need to grab name of function separately?
    //     //Token name = Consume(TokenType.SYMBOL, "Expect function name after 'define'");
    //     List<Sexpr> defineVals = new List<Sexpr>();
    //     //int i=0;
    //     //Console.WriteLine(Peek());
    //     while(!Match(TokenType.RIGHT_PAREN)){
    //         Console.WriteLine(Peek());
    //         defineVals.Add(Expression());
    //         //Console.WriteLine(defineVals[i]);
    //     }
    //     //Console.WriteLine(defineVals[0] + " " + defineVals[1]); //+ " " + defineVals[3]);
    //     return new Sexpr.Define(defineVals);
    // }

    // private Sexpr getBody(){
    //     List<Sexpr> funcBody = new List<Sexpr>();
    //     int i=0;
    //     while (!Match(TokenType.RIGHT_PAREN)){
    //         funcBody.Add(Expression());
    //         Console.WriteLine(funcBody[i]);
    //         i++;
    //     }
    //     return new Sexpr.List(funcBody);
    // }

    //create new variables with set
    // private Sexpr HandleSet(){
    //     //consume the variable name
    //     Token name = Consume(TokenType.SYMBOL, "Expected identifier in var declaration");
    //     //evaluate the rest of the set expression to get the variable value
    //     Sexpr value = Expression();
    //     Consume(TokenType.RIGHT_PAREN, "Expected ')' after set expression");
    //     return new Sexpr.Set(name, value);
    // }

      //handles the unary operations (!, -) as opposed to binary
    // private Sexpr Unary(){
    //     Token op = Previous();
    //     Sexpr right = Expression();
    //     Consume(TokenType.RIGHT_PAREN, "Expected ')' after unary expression");
    //     return new Sexpr.Unary(op, right);
    // }

    private Sexpr HandleValList(){
        List<Sexpr> listVals = new List<Sexpr>();
        //until we see a ')' character signaling the end of the list
        int i=0;
        while (!Match(TokenType.RIGHT_PAREN)){
            //item may be list of lists, recurse through Expression for both atoms and lists
            listVals.Add(Expression());

        }
        //if we have nil, empty list
        if (listVals.Count==0){
            return new Sexpr.Atom(null);
        }
        //can be nil/empty list
        return new Sexpr.List(listVals);
    }

    // private Sexpr HandleFunctionArgs(){
    //     List<Token> listVals = new List<Token>();
    //     //until we see a ')' character signaling the end of the list
    //     while (!Match(TokenType.RIGHT_PAREN)){
    //         //item may be list of lists, recurse through Expression for both atoms and lists
    //         listVals.Add(Expression());
    //     }

    //     if (listVals.Count!=0){
    //         //append nil () to end of list
    //         listVals.Add(new Sexpr.List(new List<Sexpr>()));
    //     }
    //     //can be nil/empty list
    //     return new Sexpr.List(listVals);
    // }



    // //function to handle equality tokens such as != and ==
    // private Sexpr Equality(){
    //     Sexpr Sexpr = Comparison();

    //     while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)){
    //         Token op = Previous();
    //         Sexpr right = Comparison();
    //         Sexpr = new Sexpr.Binary(Sexpr, op, right);
    //     }

    //     return Sexpr;
    // }

    //handles any tokens dealing with comparison logic (<, >, etc)
    //creates a newE to handle the comparators with the previous token (operator) and the 
    // private Sexpr Comparison(){
    //     Sexpr Sexpr = Term();
    //     while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)){
    //         Token op = Previous();
    //         Sexpr right = Term();
    //         Sexpr = new Sexpr.Binary(Sexpr, op, right);
    //     }

    //     return Sexpr;
    // }

    //handles simple +/- arithmeticEs
    // private Sexpr Term(){
    //     Sexpr Sexpr = Factor();

    //     while (Match(TokenType.MINUS, TokenType.PLUS)){
    //         Token op = Previous();
    //         Sexpr right = Factor();
    //         Sexpr = new Sexpr.Binary(Sexpr, op, right);
    //     }

    //     return Sexpr;
    // }

    // //handles arithmeticEs with division (/) and multiplication (*)
    // private Sexpr Factor(){
    //     Sexpr Sexpr = Unary();

    //     while (Match(TokenType.SLASH, TokenType.STAR)){
    //         Token op = Previous();
    //         Sexpr right = Unary();
    //         Sexpr = new Sexpr.Binary(Sexpr, op, right);
    //     }

    //     return Sexpr;
    // }

    //used to check the next token from our current index and see if it matches the expected token passed to it
    private bool Match(params TokenType[] types){
        foreach (TokenType token in types){
            if (Check(token)){
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Consume(TokenType type, string message){
        if (Check(type)) {
            return Advance();
        }
        else{
            //Console.WriteLine(Peek().type);
            throw Error(Peek(), message);
        }
    }

    //similar to match, but doesn't "consume"/advance the index
    //only looks at the next token to see if it matches the expected type
    private bool Check(TokenType type){
        if (IsAtEnd()) return false;
        return Peek().type == type;
    }


    //scans the current token and returns it after incrementing the index of our tokens list
    private Token Advance(){
        if (!IsAtEnd()) current++;
        return Previous();
    }

    //used to check if we see an EOF token, signals there is no more source code/tokens to parse
    private bool IsAtEnd(){
        return Peek().type == TokenType.EOF;
    }

    //finds the token at our current index
    private Token Peek(){
        return tokens[current];
    }

    // private char PeekNext(){
    //     if (current+1 >= source.Length) return '\0';
    //     return source[current+1];
    // }

    private Token PeekNext(){
        return tokens[current+1];
    }

    //finds the token before our current index
    private Token Previous(){
        return tokens[current-1];
    }

    private ParseError Error(Token token, string msg){
        Lisp.TokenError(token, msg);
        return new ParseError();
    }

    //find a ), means were done with a statement
    //when we see the beginning of a new statement, use a switch/case statement to handle the start of a new Lisp expr
    private void Synchronize(){
        Advance();

        //want to continue through Lox tokens until we find semicolon, meaning we are at end of statement
        while (!IsAtEnd()){
            if (Previous().type == TokenType.RIGHT_PAREN) return;

            //will complete once we have set up the statement handling logic
            switch (Peek().type){
                case TokenType.LEFT_PAREN:
                    return;
            }

            //Advance();
        }
    }

}
}