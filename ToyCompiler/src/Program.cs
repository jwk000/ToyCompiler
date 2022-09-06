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
                return;
            }

            //Console.WriteLine(lexer.ShowTokens());
            TokenReader tokenReader = new TokenReader(lexer.mTokenList);

            StatList root = new StatList();
            if (!root.Parse(tokenReader))
            {
                return;
            }

            root.Exec(Env.GlobalScope);

            Console.Read();
        }
    }



}
