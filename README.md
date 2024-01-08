## Project Description:

    This project reflects a complete interpreter for the Lisp programming language. The interpreter is written in the C# programming language and closely follows Robert Nystrom's Java implementation of the Lox programming language in his textbook 'Crafting Interpreters'. The interpreter allows for full implementation of any Lisp program, including support for variable declaration, function declarations, and various common Lisp functions

## Usage:

    The program runs using the .NET (dotnet) framework with the C# language. To construct the necessary dependencies for using the .NET framwork, follow the following link and setup instructions to install .NET. 
            https://dotnet.microsoft.com/en-us/download/dotnet/6.0

    Once installed, open a new folder within your IDE (this project was developed using Visual Studio Code) and paste all of the included files from the project into this folder. 
    
    Once you have all of the necessary files within your chosen IDE, run the command "dotnet build" within your IDE terminal. This will build a compiled binary of the C# files needed to execute any Lisp program. 

    Once compiled, run the command "dotnet run" to execute individual Lisp expressions in a REPL prompt, or "dotnet run <file_name>" to read in and execute an entire Lisp program within the provided file name. The user/tester can copy&paste any of the testing files in the 'TestResults/TestingFiles' directory into the working directory and run the interpreter to produce the expected output of any individual Lisp program.

## Testing:

    This project uses various small Lisp programs that we have completed throughout the semester, including blackjack, bowling, fibonacci, a simpler from of merging lists, and other various functions to illustrate the Lisp functions that have been implemented. 
    To test any file, simply refer to the same methods of usage, by going to the TestResults/TestingFiles directory and running that file with the command "dotnet run <path_to_file>" once the "dotnet build" has been run to generate the compiled binary. 
    **
    A few of the tests were skipped during unit testing as some of the files did not have "expect" comments to describe the expected output, or took too long computationally. However, these skipped tests were run separately and their correct output was verified.

## Implemented Functionality
    
    The Lisp language comes with several provided functions that had to be included in order to ensure the language is still Turing-Complete, including 'CAR', 'CDR', 'DEFINE', 'CONS', 'SET', 'COND', 'SYMBOL?' 'LIST?', 'ATOM?', 'NIL?', and other logical and arithmetic operators. This interpreter has implemented each of the above functions, and allowed for users to create their own functions using the 'define' keyword.  
    
    The intepreter treats everything as either a list or an atom during the parsing phase. The scanned list of tokens is parsed to create Atom-Sexpressions or List-Sexpressions, which allowed a much simpler implementation in both the interpreter files and parsing files. The parsed code is checked for these various lisp functions, identified as Lisp-Callable-Functions, and then treats everything in the corresponding list after the callable function as the arguments and body to that function. 

    COND 
    (cond t1 r1 t2 r2 t3 r3)
    if t1 is true returns r1...if t2 is true return r2...
    Requires even number of statements
    Requires 't be used for last statement if last statement makes an odd number of conditionals
    Handles de-facto if syntax. Currently no support for if, but conditional statements are handled by the cond. 

    SET (should only used globally)
        (set name exp)
        The symbol name is associated with the value of exp
        can return the name, or the value, or nil even

    +
    (+ expr1 epr2)
    Returns the sum of expressions. The expressions must be numbers

    -
    (- expr1 epr2)
    Returns the difference of expressions. The expressions must be numbers

    *
    (* expr1 epr2)
    Returns the product of expressions. The expressions must be numbers

    /
    (/ expr1 epr2)
    Returns the quotient. The expressions must be numbers

    eq?
    (eq? expr1 expr2)
    Compares the values of two atoms or (). Returns () when either expression is a larger list.

    < 
    (< expr1 expr2)
    Return t when expr1 is less than expr2. Expr1 and expr2 must be numbers.

    > 
    (> expr1 expr2)
    Return t when expr1 is greater  expr2. Expr1 and expr2 must be numbers.

    CONS
    (cons expr1 expr2)
    Create a cons cell with expr1 as car and expr2 and cdr: ie: (exp1 . expr2)

    CAR
    (car expr)
    Expr should be a non empty list. Car returns the car cell of the first cons cell

    CDR
    (cdr expr)
    Expr should be a non empty list. Cdr returns the cdr cell of the first cons cell

    NUMBER?
    (number? Expr)
    Returns t if the expr is numeric, () otherwise

    SYMBOL?
    (symbol? Expr)
    Returns t if the expr is a name, () otherwise

    LIST?
    (list? Expr)
    Returns t if Expr is not an atom

    NIL?
    (nil? Expr)
    Return t if Expr is ()

    ATOM?
    (atom? expr)
    Returns t if expr is an atom (not list or nil), or () if expr is a list or nil

    AND?
                (AND? exp1 exp2)
                Return nil if either expression is nil

    OR?
                (OR? exp1 exp2)
                Return nil if both expressions are nil
    
    Define
    (define name (arg1 .. argN) expr)
    Defines a function, name. Args is a list of formal parameters. When called the expression will be evalluated with the actual parameters replacing the formal parameters.


## Existing Errors and Nuances:
    Through testing more complex programs like mergesort, there were a few circumstances where the instances where the interpreter would not process certain declared functions that the user would define. For example, the interpreter would not process the 'mergesort' and 'ms' functions within the mergesort program, but was successful in the 'prep' and 'merge' functions. This could be an issue with the syntax of the test file since the interpreter is able to recognize and evaluate most functions in general cases, but I am not sure. 

    Also, the 'cond' function that allows for conditional statements within functions requires and even number of conditions at all times and that they be in pairs, meaning the "'t" truth symbol is required with the last statement in every cond expression.

    The cond function currently handles the same conditional logic accomplished by any If statements, so there is currently no support for if statements. That would be the primary focus of a second iteration of this project. 
