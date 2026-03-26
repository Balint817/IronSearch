using PythonExpressionManager;

namespace DependentConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var tag1 = new Script(
            //    "def run(arg, tagDict):\n" +
            //    "\treturn arg.Foo");

            //var tag2 = new Script(
            //    "def run(arg, tagDict):\n" +
            //    "\treturn arg.Bar");

            //var tag3 = new Script(
            //    "def run(arg, tagDict):\n" +
            //    "\treturn tagDict['foo']() and tagDict['bar']()");


            //WrappedCLRDelegate del = (dynamic input, PythonDictionary tagDict, PythonTuple args, PythonDictionary kwargs) =>
            //{
            //    FooBar foobar = input;
            //    return foobar.Bar && foobar.Foo;
            //};

            //var tag4 = ScriptExecutor.FromDelegate(del);

            var executor = new ScriptExecutor();

            var userScriptManager = new UserScriptManager("scripts", executor, (int)Priorities.UserScript);

            //executor.RegisterScript("foobar", tag4);


            var fooBars = new List<FooBar>() { new() { Foo = false, Bar = false }, new() { Foo = true, Bar = false }, new() { Foo = false, Bar = true }, new() { Foo = true, Bar = true } };

            var compiled = executor.Compile("[i for i in 1]");

            foreach (var fooBar in fooBars)
            {
                try
                {
                    Console.WriteLine(fooBar);
                    Console.WriteLine(executor.Evaluate(fooBar, compiled));
                    Console.WriteLine("-------------------");
                }
                catch (Exception ex)
                {
                    try
                    {
                        CompiledScript.ConvertException(ex);
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(ex2.GetType());
                        Console.WriteLine(ex2.Message);
                    }
                }
                break;
            }

            //Console.WriteLine("Press enter to continue...");
            //Console.ReadLine();
            //Console.WriteLine("-----------------------------------");
            //Console.WriteLine();
            //Console.WriteLine();
            //compiled = executor.Compile("foobar()");
            //foreach (var fooBar in fooBars)
            //{
            //    Console.WriteLine(fooBar);
            //    Console.WriteLine(executor.Evaluate(fooBar, compiled));
            //    Console.WriteLine("-------------------");
            //}
        }
    }

    public class FooBar
    {
        public bool Foo { get; set; }
        public bool Bar { get; set; }
        public override string ToString()
        {
            return $"FooBar(Foo={Foo}, Bar={Bar})";
        }
    }
}
