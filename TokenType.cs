public enum TokenType{

    //will only appear once
    LEFT_PAREN, RIGHT_PAREN, MINUS, PLUS, SLASH, STAR, QUOTE,

    //can appear once or twice 
    GREATER, GREATER_EQUAL, LESS, LESS_EQUAL, EQUAL,

    //literals
    SYMBOL, STRING, NUMBER, NIL, TRUE,

    //keywords
    IF,

    //Lisp Functions
    CAR, CDR, NILQ, LISTQ, EQQ, DEFINE, ATOMQ, SET, COND, CONS, NUMQ, SYMBOLQ,

    //Booleans
    ANDQ, ORQ, NOTQ,
    
    EOF
}