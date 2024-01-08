
namespace Lisp
{
    public class Environ
    {
        public readonly Environ enclosing;
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Environ()
        {
            enclosing=null;
        }

        public Environ(Environ enclosing)
        {
            this.enclosing = enclosing;
        }

        //add the passed value to our dict of values to define our environment
        public void Define(string name, object value)
        {
            values[name] = value;
        }

        //method to retrieve a certain token from our values dict
        //checks to see if their is an available enclosing environment
        public object Get(Token name)
        {
            
            if (values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }

            if (enclosing != null)
            {
                return enclosing.Get(name);
            }
            throw new RuntimeError(name + " Undefined variable " + name.lexeme);
        }

        // public void Assign(Token name, object value){
        //     if (values.ContainsKey(name.lexeme)){
        //         values[name.lexeme]=value;
        //         return;
        //     }
        //     if (enclosing!=null){
        //         enclosing.Assign(name, value);
        //         return;
        //     }
        //     throw new RuntimeError(name, "undefined variable '" + name.lexeme + "'. ");
        // }

        // public object GetAt(int distance, string name){
        //     try{
        //         return Ancestor(distance).values[name];
        //     }
        //     catch(Exception e){
        //         Console.WriteLine(e.Message);
        //         return null;
        //     }
        // }

        // public void AssignAt(int distance, Token name, object value){
        //     Ancestor(distance).values[name.lexeme]=value;
        // }

        // public Environ Ancestor(int distance){
        //     Environ environment = this;
        //     for (int i=0; i<distance; i++){
        //         environment = environment.enclosing;
        //     }
        //     return environment;
        // }
    }
}