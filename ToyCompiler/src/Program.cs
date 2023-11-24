using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ToyCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Env.RegisterBuildinFunctions();

            Lexer lexer = new Lexer();

            string filename = args[0];
            string script = File.ReadAllText(filename);

            if (!lexer.ParseToken(script))
            {
                Console.WriteLine("lexer parse failed!");
                Console.WriteLine(lexer.ShowTokens());
                return;
            }
            Console.WriteLine("lexer parse success!");
            TokenReader tokenReader = new TokenReader(lexer.mTokenList);

            StatList tree = new StatList();
            if (!tree.Parse(tokenReader))
            {
                Console.WriteLine("stat parse failed!");
                return;
            }
            Console.WriteLine("stat parse success!");
            //直接解释执行
            tree.Exec(Env.GlobalScope);
            //编译成指令执行
            VM vm = new VM();
            vm.Parse(tree);
            Console.WriteLine("press any key to exit...");
            Console.Read();
        }
    }



}
