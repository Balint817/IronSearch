//namespace PythonExpressionManager
//{
//    public static class ScriptTemplate<T>
//    {
//        static ScriptTemplate()
//        {
//            var t = typeof(T);

//            TemplateString =
//                $"def {Script.OutputFunctionName}(arg, tagDict):\n" +
//                $"\timport clr\n" +
//                $"\tclr.AddReference('{t.Namespace}')\n" +
//                $"\tfrom {t.Namespace} import {t.Name}\n" +
//                $"\targ: {t.Name} = arg\n"
//                ;
//        }

//        public static readonly string TemplateString;
//    }
//}
