// using System.Linq.Expressions;
// using System.Text;


// //printer class to print out Lox Sexpressions in correct format+order
// class ASTPrinter : Sexpr.ISexprVisitor<string>{
//     public void Print(List<Sexpr> SexprList){
//         foreach (Sexpr sexpr in SexprList){
//             Console.WriteLine(sexpr.Accept(this));
//         }
//     }

//     public string VisitAtomSexpr(Sexpr.Atom sexpr){
//         if (sexpr.value!=null){
//             return sexpr.value.ToString();
//         }
//         return "nil";
//     }

//     public string VisitBinarySexpr(Sexpr.Binary Sexpr){
//         return Parenthesize(Sexpr.op.lexeme, Sexpr.left, Sexpr.right);
//     }  

//     public string VisitCondSexpr(Sexpr.Cond sexpr){
//         return Parenthesize("cond", sexpr.conditions.ToArray());
//     }
    
//     //create a new define atom which takes in the function args and body conds
//     public string VisitDefineSexpr(Sexpr.Define sexpr){
//         Sexpr.Atom defineAtom = new Sexpr.Atom(sexpr.name.lexeme);
//         return Parenthesize("define", defineAtom, sexpr.args, sexpr.body);
//     }

//     //create array of each value within the given list
//     public string VisitListSexpr(Sexpr.List sexpr){
//         return Parenthesize("list", sexpr.values.ToArray());
//     }

//     //when we see "set", we create a new atom to be equal to the value in the set statement
//     public string VisitSetSexpr(Sexpr.Set sexpr){
//         Sexpr.Atom setAtom = new Sexpr.Atom(sexpr.name.lexeme);
//         return Parenthesize("set", setAtom, sexpr.value);
//     }


//     public string VisitUnarySexpr(Sexpr.Unary Sexpr){
//         return Parenthesize(Sexpr.op.lexeme, Sexpr.right);
//     }


//     private string Parenthesize(string name, params Sexpr[] Sexprs){
//         StringBuilder builder = new StringBuilder();
//         builder.Append("(").Append(name);
//         foreach (Sexpr Sexpr in Sexprs){
//             builder.Append(" ");
//             builder.Append(Sexpr.Accept(this));
//         }

//         builder.Append(")");
//         return builder.ToString();
//     }

// }