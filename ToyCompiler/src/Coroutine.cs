using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{
    class Coroutine
    {
        public Context ctx = new Context();
        public List<Token> mParams;//形参
        public int label = 0;
        public Debugger dbg = null;
        public VM vm = null;

        public bool Terminated { get; set; }
        public void AttachDebugger(Debugger d)
        {
            dbg = d;
            dbg?.BindContext(ctx);
        }

        public void Run(int label = 0)
        {
            if(label>0) ctx.IP = label;
            if (ctx.IP >= ctx.Code.Count)
            {
                Terminated = true;
                return;
            }
            Instruction ins = ctx.Code[ctx.IP];
            if (ins.Op == OpCode.Halt)
            {
                Terminated = true;
                return;
            }
            dbg?.BeforeStep();
            
            switch (ins.Op)
            {
                case OpCode.Push:
                    {
                        ctx.Stack.Push(ins.OpVar);
                        ctx.SP++;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Pop:
                    {
                        ctx.Stack.Pop();
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Add:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a + b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Sub:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a - b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Mul:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a * b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Div:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a / b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Rem:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a % b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.EQ:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a == b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.NE:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a != b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.LT:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a < b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.LE:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a <= b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.GT:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a > b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.GE:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a >= b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.And:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a && b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Or:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        Variant c = a || b;
                        ctx.Stack.Push(c);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Not:
                    {
                        Variant a = ctx.Stack.Pop();
                        Variant c = !a;
                        ctx.Stack.Push(c);
                        ctx.IP++;
                        break;
                    }
                case OpCode.SLoad:
                    {
                        Variant v = ctx.Stack.Peek(ins.OpInt);
                        ctx.Stack.Push(v);
                        ctx.SP++;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Load:
                    {
                        Variant v = ctx.LocalScope.GetVariant(ins.OpStr);
                        ctx.Stack.Push(v);
                        ctx.SP++;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Store:
                    {
                        Variant v = ctx.Stack.Pop();
                        if (!string.IsNullOrEmpty(ins.OpStr)) { v.id = ins.OpStr; }
                        ctx.LocalScope.SetVariant(v);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Assign:
                    {
                        Variant b = ctx.Stack.Pop();
                        Variant a = ctx.Stack.Pop();
                        a.Assign(b);
                        ctx.Stack.Push(a);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Index:
                    {
                        Variant x = ctx.Stack.Pop();//下标
                        Variant v = ctx.Stack.Pop();//数组
                        Variant u = v.arr.GetAt((int)x.num);
                        ctx.Stack.Push(u);
                        ctx.SP--;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Dot:
                    {
                        Variant v = ctx.Stack.Pop();
                        Variant u = v.obj.Get(ins.OpStr);
                        ctx.Stack.Push(u);
                        ctx.IP++;
                        break;
                    }
                case OpCode.NewArray:
                    {//栈顶是变量名，下面是元素个数，下面是元素，依次出栈构造变量写入作用域

                        Variant vArr = new Variant();
                        vArr.variantType = VariantType.Array;
                        vArr.arr = new VArray();
                        for (int n = 0; n < ins.OpInt; n++)
                        {
                            vArr.arr.Add(ctx.Stack.Pop());
                        }
                        vArr.arr.Reverse();
                        ctx.Stack.Push(vArr);
                        ctx.SP -= ins.OpInt - 1;
                        ctx.IP++;
                        break;
                    }
                case OpCode.NewObj:
                    {//栈顶是变量名，下面是元素个数，下面是元素，依次出栈构造变量写入作用域
                        Variant vObj = new Variant();
                        vObj.variantType = VariantType.Object;
                        vObj.obj = new VObject();
                        for (int n = 0; n < ins.OpInt; n++)
                        {
                            var val = ctx.Stack.Pop();
                            var key = ctx.Stack.Pop();
                            vObj.obj.Add(key.str, val);
                        }
                        ctx.Stack.Push(vObj);
                        ctx.SP -= ins.OpInt * 2 - 1;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Enum:
                    {
                        //栈:obj，k，v
                        Variant val = ctx.Stack.Peek(1);
                        Variant key = ctx.Stack.Peek(2);
                        Variant obj = ctx.Stack.Peek(3);
                        Variant e = new Variant();
                        e.variantType = VariantType.Enum;
                        e.id = "e";
                        if (obj.variantType == VariantType.Array)
                        {
                            e.enu = obj.arr.GetEnumerator();
                            //下标写入作用域
                            key.variantType = VariantType.Number;
                            key.num = 0;
                            ctx.LocalScope.SetVariant(key);
                            ctx.LocalScope.SetVariant(val);
                        }
                        else if (obj.variantType == VariantType.Object)
                        {
                            e.enu = obj.obj.GetEnumerator();
                            //key
                            key.variantType = VariantType.String;
                            key.str = "-";
                            ctx.LocalScope.SetVariant(key);
                            ctx.LocalScope.SetVariant(val);
                        }
                        ctx.Stack.Push(e);
                        ctx.Stack.Push(ctx.BP);
                        ctx.SP += 2;
                        ctx.BP = ctx.SP;//新的栈帧，迭代器完成后还原栈帧
                        ctx.IP++;
                        break;
                    }
                case OpCode.Next:
                    {
                        //栈:obj，k，v，enu，bp
                        //Variant b = ctx.Stack.Peek(1);
                        Variant e = ctx.Stack.Peek(2);
                        Variant v = ctx.Stack.Peek(3);
                        Variant k = ctx.Stack.Peek(4);
                        if (e.enu.MoveNext())
                        {
                            if (k.variantType == VariantType.Number)
                            {
                                k.num++;
                                v.Assign(e.enu.Current as Variant);
                            }
                            else if (k.variantType == VariantType.String)
                            {
                                var kv = (KeyValuePair<string, Variant>)e.enu.Current;
                                k.str = kv.Key;
                                v.Assign(kv.Value);
                            }
                            ctx.IP++;
                        }
                        else
                        {
                            //清理栈，跳转
                            ctx.BP = (int)ctx.Stack.Pop();
                            ctx.Stack.Pop();
                            ctx.Stack.Pop();
                            ctx.Stack.Pop();
                            ctx.Stack.Pop();
                            ctx.SP -= 5;
                            ctx.IP = ins.OpInt;
                        }
                        break;
                    }
                case OpCode.EnterScope:
                    {
                        Scope scope = new Scope();
                        scope.SetUpScope(ctx.LocalScope);
                        ctx.LocalScope = scope;
                        ctx.IP++;
                        break;
                    }
                case OpCode.LeaveScope:
                    {
                        ctx.LocalScope = ctx.LocalScope.GetUpScope();
                        ctx.IP++;
                        break;
                    }
                case OpCode.Jump:
                    {
                        ctx.IP = ins.OpInt;
                        break;
                    }
                case OpCode.NJump:
                    {
                        if (ctx.Stack.Pop() == false)
                        {
                            ctx.IP = ins.OpInt;
                        }
                        else
                        {
                            ctx.IP++;
                        }
                        ctx.SP--;
                        break;
                    }
                case OpCode.Call:
                    {
                        //调用前的栈：函数变量，参数列表，参数数量，返回地址
                        int argNum = (int)ctx.Stack.Peek(2);
                        Variant func = ctx.Stack.Peek(3 + argNum);
                        if (func.api != null) //宿主函数
                        {
                            int retNum = func.api.Invoke(ctx);
                            List<Variant> rets = new List<Variant>();
                            for (int i = 0; i < retNum; i++)
                            {
                                rets.Add(ctx.Stack.Pop());
                            }
                            ctx.IP = (int)ctx.Stack.Pop();//返回地址出栈
                            ctx.Stack.Pop();//参数数量出栈
                            for (int i = 0; i < argNum; i++)
                            {
                                ctx.Stack.Pop();//参数出栈
                            }
                            ctx.Stack.Pop();//函数变量出栈
                                            //返回值压栈
                            for (int i = 0; i < rets.Count; i++)
                            {
                                ctx.Stack.Push(rets[i]);
                            }
                            ctx.SP -= 3 + argNum;
                        }
                        else //脚本函数
                        {
                            //栈：函数变量，参数列表，参数数量，返回地址，作用域，BP
                            ctx.Stack.Push(ctx.LocalScope);
                            ctx.Stack.Push(ctx.BP);
                            ctx.SP += 2;
                            ctx.BP = ctx.SP;

                            //新的作用域写入参数
                            Scope scope = new Scope();
                            scope.SetUpScope(ctx.GlobalScope);
                            ctx.LocalScope = scope;

                            for (int i = 0; i < argNum; i++)
                            {
                                Variant v = new Variant();
                                v.id = func.fun.mParams[i].desc;
                                v.Assign(ctx.Stack.Peek(4 + argNum - i));
                                scope.SetVariant(v);
                            }
                            ctx.IP = func.label;
                        }

                        break;
                    }
                case OpCode.Ret:
                    {
                        //栈：函数变量，参数列表，参数数量，返回地址，作用域，BP，返回值
                        //返回值暂存
                        List<Variant> rets = new List<Variant>();
                        while (ctx.Stack.Count > ctx.BP + 1)
                        {
                            rets.Add(ctx.Stack.Pop());
                        }
                        ctx.BP = (int)ctx.Stack.Pop();//上级BP
                        ctx.LocalScope = (Scope)ctx.Stack.Pop();//上级作用域
                        ctx.IP = (int)ctx.Stack.Pop();//返回地址出栈
                        int argNum = (int)ctx.Stack.Pop();//参数数量出栈
                        for (int i = 0; i < argNum; i++)
                        {
                            ctx.Stack.Pop();//参数出栈
                        }
                        ctx.Stack.Pop();//函数变量出栈
                                        //返回值压栈
                        for (int i = 0; i < rets.Count; i++)
                        {
                            ctx.Stack.Push(rets[i]);
                        }
                        ctx.SP -= 5 + argNum;
                        //栈：返回值
                        break;
                    }
                case OpCode.Print:
                    {
                        //栈：函数变量，参数列表，参数数量，返回地址，作用域，BP
                        int argNum = (int)ctx.Stack.Peek(4);
                        for (int i = 0; i < argNum; i++)
                        {
                            Variant v = ctx.Stack.Peek(4 + argNum - i);
                            Console.Write($"{v}\t");
                        }
                        Console.WriteLine();
                        ctx.IP++;
                        break;
                    }
                case OpCode.Len:
                    {
                        //栈：函数变量，参数列表，参数数量，返回地址，作用域，BP
                        Variant v = ctx.Stack.Peek(5);
                        Variant r = v.arr.Length();
                        ctx.Stack.Push(r);
                        ctx.SP++;
                        ctx.IP++;
                        break;
                    }
                case OpCode.Clear:
                    {
                        //清理栈帧，每条语句执行后栈帧应该是空的
                        while (ctx.Stack.Count > ctx.BP + 1)
                        {
                            ctx.Stack.Pop();
                            ctx.SP--;
                        }
                        ctx.IP++;
                        break;
                    }
                case OpCode.CoYield://和resume的区别是自身不会压栈，不用返回
                    {
                        //co栈：main,args...,argnum
                        int argNum = (int)ctx.Stack.Pop();
                        Coroutine co = (Coroutine)ctx.Stack.Peek(argNum + 1);
                        for (int i = 0; i < argNum; i++)
                        {
                            co.ctx.Stack.Push(ctx.Stack.Pop());
                        }
                        ctx.Stack.Pop();//main出栈
                        ctx.SP -= argNum + 2;
                        ctx.BP--;
                        ctx.IP++;
                        co.ctx.SP += argNum;
                        co.AttachDebugger(dbg);  
                        vm.SetCurCo(co);
                        //main栈：args...
                        break;
                    }
                case OpCode.CoResume://和yield的区别是会把自身压栈，用于返回
                    {
                        //main栈：co,args...,argnum
                        int argNum = (int)ctx.Stack.Pop();
                        Coroutine co = (Coroutine)ctx.Stack.Peek(argNum + 1);
                        co.vm = vm;
                        co.ctx.Stack.Push(this);//自身压栈
                        co.ctx.BP++;
                        //第一次进入协程需要传递参数
                        if(co.ctx.IP == co.label)
                        {
                            co.ctx.GlobalScope = ctx.GlobalScope;
                            co.ctx.LocalScope = co.ctx.GlobalScope;

                            //新的作用域写入参数
                            Scope scope = new Scope();
                            scope.SetUpScope(co.ctx.GlobalScope);
                            co.ctx.LocalScope = scope;

                            for (int i = 0; i < argNum; i++)
                            {
                                Variant v = new Variant();
                                v.id = co.mParams[i].desc;
                                v.Assign(ctx.Stack.Peek(argNum - i));
                                scope.SetVariant(v);
                            }
                            for (int i = 0; i < argNum; i++)
                            {
                                ctx.Stack.Pop();
                            }
                            co.ctx.SP++;
                        }
                        else //再次进入协程直接传参数
                        {
                            for (int i = 0; i < argNum; i++)
                            {
                                co.ctx.Stack.Push(ctx.Stack.Pop());
                            }
                            co.ctx.SP += argNum+1;
                        }

                        
                        ctx.Stack.Pop();//co出栈
                        ctx.SP -= argNum + 2;
                        ctx.IP++;

                        co.AttachDebugger(dbg);
                        vm.SetCurCo(co);
                        //co栈：main
                        break;
                    }
                default:
                    break;
            }

            dbg?.AfterStep(ins);


        }

    }
}
