using System;
using System.Collections.Generic;

public abstract class Sexpr
{
public interface ISexprVisitor<R>
{
   R VisitAtomSexpr(Atom sexpr);
   R VisitListSexpr(List sexpr);
}
  public class Atom  : Sexpr 
  {
      public Atom(object value)
      {
             this.value = value;
      }

  public override R Accept<R>(ISexprVisitor<R> visitor)
  {
     return visitor.VisitAtomSexpr(this);
      }
      public readonly object value;
      }

  public class List  : Sexpr 
  {
      public List(List<Sexpr> values)
      {
             this.values = values;
      }

  public override R Accept<R>(ISexprVisitor<R> visitor)
  {
     return visitor.VisitListSexpr(this);
      }
      public readonly List<Sexpr> values;
      }


     public abstract R Accept<R>(ISexprVisitor<R> visitor);
}
