using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{

    class Context
    {
        public int ID = 0;
        public int BP = -1;
        public int SP = -1;
        public int IP = 0;
        public List<Variant> Stack = new List<Variant>();
        public List<Instruction> Code = new List<Instruction>();
        public Scope LocalScope;//当前作用域
        public Scope GlobalScope = new Scope();//全局作用域
        public Coroutine Co;
    }

    static class ListUtils
    {
        public static T Pop<T>(this List<T> list)
        {
            T t = list.Last();
            list.RemoveAt(list.Count - 1);
            return t;
        }

        public static T Peek<T>(this List<T> list, int n)
        {
            int idx = list.Count - n;
            return list[idx];
        }

        public static void Push<T>(this List<T> list, T value)
        {
            list.Add(value);
        }

    }

    class Debugger
    {
        Context mCtx;
        List<int> mBreakPoints = new List<int>();
        bool mStepMode = false;
        string mLastCmd = "";
        public void BindContext(Context ctx)
        {
            mCtx = ctx;
        }


        public void BeforeStep()
        {
            if (mStepMode || mBreakPoints.Contains(mCtx.IP))
            {
                WaitRun();
            }
        }

        public void AfterStep(Instruction ins)
        {
            if (mStepMode)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var v in mCtx.Stack)
                {
                    sb.Append(v.ToString() + " ");
                }

                Console.WriteLine($"co-{mCtx.ID}: {ins,-28} [IP:{mCtx.IP,4}][BP:{mCtx.BP,4}][SP:{mCtx.SP,4}]|Stack:{sb}");
            }
        }

        public void WaitRun()
        {
            Console.Write(">");
            string cmd = Console.ReadLine();
            if (cmd.Length == 0)
            {
                if (mLastCmd.Length > 0)
                {
                    cmd = mLastCmd;
                }
            }
            if (cmd.Length == 0)
            {
                WaitRun();
                return;
            }
            mLastCmd = cmd;
            string[] args = cmd.Split(' ');
            switch (args[0])
            {
                case "r":
                    {
                        mStepMode = false;
                        return;
                    }
                case "s":
                    {
                        mStepMode = true;
                        return;
                    }
                case "b":
                    {
                        AddBreakPoint(int.Parse(args[1])); break;
                    }
                case "p":
                    {
                        PrintVariant(args[1]); break;
                    }
                case "l":
                    {
                        PrintCode(); break;
                    }
                case "sc":
                    {
                        PrintScope(); break;
                    }
                case "bt":
                    {
                        PrintCallStack(); break;
                    }
                default:
                    break;
            }
            WaitRun();
        }

        public void AddBreakPoint(int lineno)
        {
            if (!mBreakPoints.Contains(lineno))
            {
                mBreakPoints.Add(lineno);
            }

            Console.WriteLine($"BreakPoint at {lineno} added.");
        }
        public void DelBreakPoint(int lineno)
        {
            mBreakPoints.Remove(lineno);
            Console.WriteLine($"BreakPoint at {lineno} removed.");
        }

        public void PrintVariant(string varname)
        {
            Variant v = mCtx.LocalScope.GetVariant(varname);
            if (v != null)
            {
                Console.WriteLine($"{varname}:{v}");
            }
        }

        public void PrintCode(int n = 10)
        {
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(mCtx.Code[mCtx.IP + i]);
            }
        }

        public void PrintScope()
        {
            Console.WriteLine(mCtx.LocalScope);
        }

        public void PrintCallStack()
        {
            //逆序遍历栈，寻找bp和函数变量
            int bp = mCtx.BP;
            while (bp > 0)
            {
                bp = (int)mCtx.Stack[bp].num;
                Variant f = mCtx.Stack[bp + 1];
                Console.WriteLine(f);//todo 发起调用的行
            }
        }
    }

    class VM
    {
        public static int CtxID = 0;
        StatTree tree = new StatTree();
        Coroutine main = new Coroutine();
        Context ctx = null;     
        Debugger dbg = null;
        Coroutine curco = null;

        public VM()
        {
            main.vm = this;
            ctx = main.ctx;
            ctx.ID = CtxID++;
            ctx.Co = main;
            ctx.LocalScope = ctx.GlobalScope;
            curco = main;
        }

        public void AttachDebugger()
        {
            dbg = new Debugger();
            main.AttachDebugger(dbg);
        }

        public void SetCurCo(Coroutine co)
        {
            curco = co;
        }

        public bool Parse(string script)
        {
            //词法分析
            Lexer lexer = new Lexer();
            if (!lexer.ParseToken(script))
            {
                Console.WriteLine("lexer parse failed!");
                Console.WriteLine(lexer.ShowTokens());
                return false;
            }
            Console.WriteLine("lexer parse success!");

            //语法分析
            TokenReader tokenReader = new TokenReader(lexer.mTokenList);

            if (!tree.Parse(tokenReader))
            {
                Console.WriteLine("stat parse failed!");
                return false;
            }
            Console.WriteLine("stat parse success!");
            return true;
        }

        public void Exec(string script)
        {
            if (Parse(script))
            {
                Executor.RegisterBuildinFunctions();
                tree.Exec(Executor.GlobalScope);
            }
        }

        public void Compile(string script)
        {
            if (Parse(script))
            {
                Visit(tree);
            }
        }

        void Visit(StatTree tree = null)
        {
            Interaction.Print().Visit(ctx.Code);
            Interaction.Len().Visit(ctx.Code);
            tree?.Visit(ctx.Code);

            for (int i = 0; i < ctx.Code.Count; i++)
            {
                Instruction ins = ctx.Code[i];
                ins.CodeLine = i;
            }
        }

        public void Dump()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Instruction ins in ctx.Code)
            {
                sb.AppendLine(ins.ToString());
            }
            Console.WriteLine(sb.ToString());
        }

        public void Run(int label = 0)
        {
            dbg?.WaitRun();

            while (!curco.Terminated)
            {
                curco.Run();
            }
            //main.Run(label);
        }

        //read-eval-print-loop
        public void REPL()
        {
            //可以直接输出表达式的值
            //可以定义变量和函数
            //可以换行输入，检测语法完整结束输入
            //可以用上下箭头查看历史记录
            //按esc退出

            List<string> cmdHistory = new List<string>();
            int historyIndex = -1;
            string cmd = "";
            bool nextline = true;
            while (true)
            {
                if (nextline) { Console.Write(">"); nextline = false; }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    break;
                }

                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (historyIndex >= 0)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(new string(' ', Console.WindowWidth)); // 清空当前行
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write('>');
                        Console.Write(cmdHistory[historyIndex]);
                        historyIndex = Math.Max(0, historyIndex - 1);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if (historyIndex < cmdHistory.Count - 1)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(new string(' ', Console.WindowWidth)); // 清空当前行
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write('>');
                        historyIndex = Math.Min(cmdHistory.Count - 1, historyIndex + 1);
                        Console.Write(cmdHistory[historyIndex]);
                    }
                    else if (historyIndex == cmdHistory.Count - 1)
                    {
                        // 用户达到历史记录的末尾，清空当前行
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write('>');
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (string.IsNullOrEmpty(cmd))
                    {
                        continue;
                    }

                    if (!Parse(cmd))
                    {
                        continue;
                    }
                    Visit(tree);
                    Run();
                    cmdHistory.Add(cmd);
                    historyIndex = cmdHistory.Count - 1;
                    nextline = true;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (cmd.Length > 0)
                    {
                        cmd = cmd.Substring(0, cmd.Length - 1);
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(">{0}", cmd);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (Console.CursorLeft >= 1)
                    {
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (Console.CursorLeft < cmd.Length)
                    {
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    }
                }
                else
                {
                    char c = keyInfo.KeyChar;
                    cmd += c;

                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(">{0}", cmd);

                }
            }
        }

        public void TestInteraction()
        {
            //cs 调用 js
            string code = "function fib(n) {if(n<3){return n;} return fib(n-1)+fib(n-2);}";
            //js 调用 cs
            code += "print(\"js call cs fib(5)=\", csfib(5));";
            //注册cs函数
            InteractAPI.RegFunc(ctx, Interaction.JsCallCs_Fibonacci, "csfib");
            Parse(code);
            Visit(tree);
            Dump();
            Run();
            //调用js函数
            Interaction.CsCallJs_Fibonacci(ctx);
        }

    }
}
