using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{
    //内置函数
    static class Buildin
    {
        public static FunStat Print()
        {
            FunStat print = new FunStat();
            print.mFunID = new Token() { tokenType = TokenType.TTID, desc = "print" };
            print.mParams = new List<Token>
            {
                new Token(){ tokenType = TokenType.TTID, desc = "arg1"},
                new Token(){ tokenType = TokenType.TTID, desc = "arg2"},
                new Token(){ tokenType = TokenType.TTID, desc = "arg3"},
                new Token(){ tokenType = TokenType.TTID, desc = "arg4"},
                new Token(){ tokenType = TokenType.TTID, desc = "arg5"},
                new Token(){ tokenType = TokenType.TTID, desc = "arg6"},
                new Token(){ tokenType = TokenType.TTID, desc = "arg7"},
                new Token(){ tokenType = TokenType.TTID, desc = "arg8"},
                new Token(){ tokenType = TokenType.TTID, desc = "arg9"},
            };
            print.mInnerBuild = true;
            print.mInnerAction = (Scope scope) =>
            {
                foreach (Token t in print.mParams)
                {
                    var v = scope.GetVariant(t.desc);
                    if (v != null)
                    {
                        Console.Write($"{v}\t");
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine();
            };

            return print;
        }

        public static FunStat Len()
        {
            FunStat len = new FunStat();
            len.mFunID = new Token() { tokenType = TokenType.TTID, desc = "len" };
            len.mParams = new List<Token>
            {
                new Token(){tokenType = TokenType.TTID, desc = "arg1"}
            };
            len.mInnerBuild = true;
            len.mInnerAction = (Scope scope) =>
            {
                var v = scope.GetVariant("arg1");
                VArray arr = v.arr;
                Variant r = new Variant();
                r.variantType = VariantType.Number;
                r.num = arr.Length();
                Env.RunStack.Add(r);
            };

            return len;
        }

    }
}
