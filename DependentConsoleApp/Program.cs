using IronPython.Runtime;
using PythonExpressionManager;

namespace DependentConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            //var tag2 = new Script(
            //    "def run(arg, tagDict):\n" +
            //    "\treturn arg.Bar");

            //var tag3 = new Script(
            //    "def run(arg, tagDict):\n" +
            //    "\treturn tagDict['foo']() and tagDict['bar']()");


            WrappedCLRDelegate del = (dynamic input, PythonDictionary tagDict, PythonTuple args, PythonDictionary kwargs) =>
            {
                throw new Exception("foobar");
                //FooBar foobar = input;
                //return foobar.Bar && foobar.Foo;
            };

            var executor = new ScriptExecutor();

            var userScriptManager = new UserScriptManager("scripts", executor, (int)Priorities.UserScript);

            var tag1 = new Script(executor.Engine,
                "def run(arg, tagDict):\n"
                + "\timport clr\n"
                + "\timport System\n"
                + "\traise System.Exception('asd')\n"
                );

            var tag4 = executor.FromDelegate(del);

            executor.RegisterScript("foo", tag1);
            executor.RegisterScript("foobar", tag4);


            var fooBars = new List<FooBar>() { new() { Foo = false, Bar = false }, new() { Foo = true, Bar = false }, new() { Foo = false, Bar = true }, new() { Foo = true, Bar = true } };

            CompiledScript compiled;
            try
            {
                compiled = executor.Compile("foo)");
            }
            catch (Exception ex)
            {

                try
                {
                    if (!CompiledScript.TryConvertException(ex, executor.Engine))
                    {
                        throw;
                    }
                    //CompiledScript.TryConvertException(ex, executor.Engine);
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(1);
                    Console.WriteLine(ex2.Message);
                    return;
                }
                Console.WriteLine(2);
                Console.WriteLine(ex.GetType());
                Console.WriteLine(ex);

                Console.WriteLine();
                return;
            }

            foreach (var fooBar in fooBars)
            {
                try
                {
                    //Console.WriteLine(fooBar);
                    Console.WriteLine(executor.Evaluate(fooBar, compiled));
                    Console.WriteLine("-------------------");
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (!CompiledScript.TryConvertException(ex, executor.Engine))
                        {
                            throw;
                        }
                        //CompiledScript.TryConvertException(ex, executor.Engine);
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(1);
                        Console.WriteLine(ex2.Message);
                        break;
                    }
                    Console.WriteLine(2);
                    Console.WriteLine(ex.GetType());
                    Console.WriteLine(ex);

                    Console.WriteLine();
                    break;
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
