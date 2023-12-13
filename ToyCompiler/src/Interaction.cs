using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{
    //js调用cs的函数
    delegate int APIDelegate(Context ctx);

    //C#和js交互
    static class Interaction
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
            print.mInnerAction = () =>
            {
                foreach (Token t in print.mParams)
                {
                    var v = Executor.LocalScope.GetVariant(t.desc);
                    if (v != null)
                    {
                        Console.Write($"{v} ");
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine();
            };
            print.mInnerVisit = (List<Instruction> code) =>
            {
                code.Add(new Instruction(OpCode.Print));
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
            len.mInnerAction = () =>
            {
                var v = Executor.LocalScope.GetVariant("arg1");
                VArray arr = v.arr;
                Variant r = new Variant();
                r.variantType = VariantType.Number;
                r.num = arr.Length();
                Executor.RunStack.Add(r);
            };
            len.mInnerVisit = (List<Instruction> code) =>
            {
                code.Add(new Instruction(OpCode.Len));
            };
            return len;
        }

        public static int fib(int n)
        {
            if (n < 0) return 0;
            if (n < 3) return n;
            return fib(n - 1) + fib(n - 2);
        }

        public static int JsCallCs_Fibonacci(Context ctx)
        {
            //从栈上获取参数
            int n = (int)InteractAPI.API_ArgToNumber(ctx,0);
            int r = fib(n);
            InteractAPI.API_PushNumber(ctx,r);
            return 1;//返回值数量
        }

        public static void CsCallJs_Fibonacci(Context ctx)
        {
            Variant v = 10;
            InteractAPI.API_Call(ctx, "fib", v);
            int ret = (int)InteractAPI.API_PeekNumber(ctx,0);
            Console.WriteLine(ret);
        }

    }

    static class InteractAPI
    {

        public static void RegFunc(Context ctx,APIDelegate api, string name)
        {
            Variant f = new Variant();
            f.variantType = VariantType.Function;
            f.id = name;
            f.api = api;
            ctx.GlobalScope.SetVariant(f);
        }

        //idx从0开始
        public static double API_ArgToNumber(Context ctx, int idx)
        {
            int argNum = (int)ctx.Stack.Peek(2);
            Variant v = ctx.Stack.Peek(2 + argNum - idx);
            return (double)v;
        }

        public static string API_ArgToString(Context ctx, int idx)
        {
            int argNum = (int)ctx.Stack.Peek(2);
            Variant v = ctx.Stack.Peek(2 + argNum - idx);
            return (string)v;
        }

        public static bool API_ArgToBoolean(Context ctx, int idx)
        {
            int argNum = (int)ctx.Stack.Peek(2);
            Variant v = ctx.Stack.Peek(2 + argNum - idx);
            return (bool)v;
        }

        public static double API_PeekNumber(Context ctx, int idx)
        {
            Variant v = ctx.Stack.Peek(idx + 1);
            return (double)v;
        }

        public static string API_PeekString(Context ctx, int idx)
        {
            Variant v = ctx.Stack.Peek(idx + 1);
            return (string)v;
        }

        public static bool API_PeekBoolean(Context ctx, int idx)
        {
            Variant v = ctx.Stack.Peek(idx + 1);
            return (bool)v;
        }

        public static void API_PushNumber(Context ctx, double d)
        {
            ctx.Stack.Push(d);
            ctx.SP++;
        }

        public static void API_PushString(Context ctx, string s)
        {
            ctx.Stack.Push(s);
            ctx.SP++;
        }

        public static void API_PushBoolean(Context ctx, bool b)
        {
            ctx.Stack.Push(b);
            ctx.SP++;
        }

        public static int API_GetArgCount(Context ctx)
        {
            int argNum = (int)ctx.Stack.Peek(4);
            return argNum;
        }

        //模拟了call指令
        public static int API_Call(Context ctx, string name, params object[] args)
        {
            Variant f = ctx.LocalScope.GetVariant(name);
            if (f.variantType == VariantType.Function)
            {
                ctx.Stack.Push(f);
                foreach (Variant v2 in args)
                {
                    ctx.Stack.Push(v2);
                }
                ctx.Stack.Push(args.Length);
                ctx.Stack.Push(ctx.Code.Count);//返回地址
                ctx.Stack.Push(ctx.LocalScope);
                ctx.Stack.Push(ctx.BP);
                ctx.SP += 5 + args.Length;
                ctx.BP = ctx.SP;
                Scope scope = new Scope();
                scope.SetUpScope(ctx.GlobalScope);
                ctx.LocalScope = scope;
                for (int i = 0; i < args.Length; i++)
                {
                    Variant v = new Variant();
                    v.id = f.fun.mParams[i].desc;
                    v.Assign(args[i] as Variant);
                    scope.SetVariant(v);
                }
                ctx.IP = f.label;
                ctx.Co.Run(f.label);
                //此时栈上应该只有返回值了
                return ctx.Stack.Count;
            }
            return -1;
        }


    }
}
