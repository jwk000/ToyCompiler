using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{

    class OpCode
    {
        public const int Nop = 0;//空操作
        public const int Push = 1; //Push 100
        public const int Add = 2; //Add
        public const int Sub = 3; //Sub
        public const int Mul = 4; //Mul
        public const int Div = 5; //Div
        public const int Rem = 6; //Rem
        public const int EQ = 7; //EQ
        public const int NE = 8; //NEQ
        public const int LT = 9; //LT
        public const int LE = 10;//LE
        public const int GT = 11;//GT
        public const int GE = 12;//GE
        public const int And = 13;//And
        public const int Or = 14; //Or
        public const int Not = 15;//Not
        public const int Jump = 16;//JMP
        public const int JumpTrue = 17;//NJMP
        public const int Call = 18;//Call
        public const int Ret = 19; //RET
        public const int Halt = 20;//Halt
        public const int EnterScope = 21;
        public const int LeaveScope = 22;
        public const int Next = 23;//迭代成功执行循环，失败跳出循环
        public const int Pop = 24;
        public const int Load = 25;//从当前作用域加载一个变量
        public const int Store = 26;//把一个变量写入当前作用域
        public const int Assign = 27;//赋值
        public const int Index = 28;//数组下标
        public const int Dot = 29;//对象成员
        public const int NewArray = 30;//
        public const int NewObj = 31;
        public const int Enum = 32;//栈顶是对象，取迭代器，入栈
        public const int JumpFalse = 33;
        public const int SLoad = 34;//栈上的变量压入栈顶
        public const int Print = 35;
        public const int Len = 36;

        public static string[] OpCodeNames = new string[]
        {
            "NOP","PUSH","ADD","SUB","MUL","DIV","REM","EQ","NE","LT","LE","GT","GE","AND","OR","NOT","JUMP","JUMPTRUE",
            "CALL","RET","HALT","ENTERSCOPE","LEAVESCOPE","NEXT","POP","LOAD","STORE","ASSIGN","INDEX","DOT","NEWARRAY","NEWOBJ",
            "ENUM","JUMPFALSE","SLOAD","PRINT","LEN"
        };
    }

    //记录用来跳转的标签
    struct JumpLabel
    {
        public Instruction ContinueJump;
        public Instruction BreakJump;
        public JumpLabel(Instruction c, Instruction bj) { ContinueJump = c; BreakJump = bj; }
    }
    class Instruction
    {
        public int Op;
        public int OpInt;
        public string OpStr;
        public Variant OpVar;
        public Instruction(int op)
        {
            Op = op;
        }

        public override string ToString()
        {
            string param = "";
            switch (Op){
                case OpCode.Push:
                    param = OpVar.ToString();
                    break;
                case OpCode.Jump:
                case OpCode.JumpFalse:
                case OpCode.JumpTrue:
                case OpCode.Next:
                case OpCode.Index:
                case OpCode.SLoad:
                    param = OpInt.ToString();
                    break;
                case OpCode.Load:
                case OpCode.Store:
                case OpCode.Dot:
                    param = OpStr;
                    break;
                    
            }
            return $"{OpCode.OpCodeNames[Op],-12} {param}";
        }

        public static Instruction NOP = new Instruction(0);
        public static Stack<JumpLabel> JumpLabels = new Stack<JumpLabel>();
    }



    class Context
    {
        public int BP = -1;
        public int SP = -1;
        public int IP = 0;
        public List<Variant> Stack = new List<Variant>();
        public List<Instruction> Code = new List<Instruction>();
        public Scope LocalScope;//当前作用域
        public Scope GlobalScope = new Scope();//全局作用域
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

    class VM
    {
        Context ctx = new Context();

        public void Visit(StatList tree)
        {
            Buildin.Print().OnVisit(ctx.Code);
            Buildin.Len().OnVisit(ctx.Code);
            tree.OnVisit(ctx.Code);
            //ctx.GlobalScope.Merge(Env.GlobalScope);
        }

        public void DumpInstructions()
        {
            StringBuilder sb = new StringBuilder();
            for(int i=0;i<ctx.Code.Count;i++)
            {
                Instruction ins = ctx.Code[i];
                sb.AppendLine($"{i,4:0}: {ins}");
            }
            Console.WriteLine(sb.ToString());
        }

        public void Exec()
        {
            ctx.LocalScope = ctx.GlobalScope;
            for (ctx.IP = 0; ctx.IP < ctx.Code.Count;)
            {
                Instruction ins = ctx.Code[ctx.IP];
                if (ins.Op == OpCode.Halt)
                {
                    break;
                }
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
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a + b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.Sub:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a - b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.Mul:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a * b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.Div:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a / b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.Rem:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a % b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.EQ:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a == b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.NE:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a != b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.LT:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a < b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.LE:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a <= b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.GT:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a > b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.GE:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a >= b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.And:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            Variant c = a && b;
                            ctx.Stack.Push(c);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.Or:
                        {
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
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
                            Variant a = ctx.Stack.Pop();
                            Variant b = ctx.Stack.Pop();
                            a.Assign(b);
                            ctx.Stack.Push(a);
                            ctx.LocalScope.SetVariant(a);
                            ctx.SP--;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.Index:
                        {
                            Variant v = ctx.Stack.Pop();
                            Variant u = v.arr.GetAt(ins.OpInt);
                            ctx.Stack.Push(u);
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
                            Variant vArr = ctx.Stack.Pop();
                            Variant vNum = ctx.Stack.Pop();
                            vArr.arr = new VArray();
                            for (int n = 0; n < vNum.num; n++)
                            {
                                vArr.arr.Add(ctx.Stack.Pop());
                            }
                            ctx.LocalScope.SetVariant(vArr);
                            ctx.Stack.Push(vArr);
                            ctx.SP -= (int)vNum.num + 2;
                            ctx.IP++;
                            break;
                        }
                    case OpCode.NewObj:
                        {//栈顶是变量名，下面是元素个数，下面是元素，依次出栈构造变量写入作用域
                            Variant vObj = ctx.Stack.Pop();
                            Variant vNum = ctx.Stack.Pop();
                            vObj.obj = new VObject();
                            for (int n = 0; n < vNum.num; n++)
                            {
                                vObj.obj.Add(ctx.Stack.Pop().str, ctx.Stack.Pop());
                            }
                            ctx.LocalScope.SetVariant(vObj);
                            ctx.Stack.Push(vObj);
                            ctx.SP -= (int)vNum.num * 2 + 2;
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
                            if (obj.variantType == VariantType.Array)
                            {
                                e.enu = obj.arr.GetEnumerator();
                                //下标写入作用域
                                key.variantType = VariantType.Number;
                                key.num = 0;
                                ctx.LocalScope.SetVariant(key);
                            }
                            else if (obj.variantType == VariantType.Object)
                            {
                                e.enu = obj.obj.GetEnumerator();
                                //key
                                key.variantType = VariantType.String;
                                ctx.LocalScope.SetVariant(key);
                            }
                            ctx.Stack.Push(e);
                            ctx.IP++;
                            break;
                        }
                    case OpCode.Next:
                        {
                            //栈:obj，k，v，enu
                            Variant e = ctx.Stack.Peek(1);
                            Variant v = ctx.Stack.Peek(2);
                            Variant k = ctx.Stack.Peek(3);
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
                                ctx.Stack.Pop();
                                ctx.Stack.Pop();
                                ctx.Stack.Pop();
                                ctx.Stack.Pop();
                                ctx.SP -= 4;
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
                    case OpCode.JumpTrue:
                        {
                            if (ctx.Stack.Pop() == true)
                            {
                                ctx.IP = ins.OpInt;
                            }
                            else
                            {
                                ctx.IP++;
                            }
                            break;
                        }
                    case OpCode.JumpFalse:
                        {
                            if (ctx.Stack.Pop() == false)
                            {
                                ctx.IP = ins.OpInt;
                            }
                            else
                            {
                                ctx.IP++;
                            }
                            break;
                        }
                    case OpCode.Call:
                        {
                            //栈：函数变量，参数列表，参数数量，返回地址，BP
                            ctx.Stack.Push(ctx.BP);
                            ctx.SP++;
                            ctx.BP = ctx.SP;
                            Scope scope = new Scope();
                            scope.SetUpScope(ctx.GlobalScope);
                            ctx.LocalScope = scope;
                            int argNum = (int)ctx.Stack.Peek(3);
                            Variant func = ctx.Stack.Peek(4 + argNum);
                            for(int i = 0; i < argNum; i++)
                            {
                                Variant v = new Variant();
                                v.id = func.fun.mParams[i].desc;
                                v.Assign(ctx.Stack.Peek(3 + argNum - i));
                                scope.SetVariant(v);
                            }
                            ctx.IP = func.label;

                            break;
                        }
                    case OpCode.Ret:
                        {
                            //栈上保留的都是返回值
                            List<Variant> rets = new List<Variant>();
                            while (ctx.Stack.Count > ctx.BP)
                            {
                                rets.Add(ctx.Stack.Pop());
                            }
                            ctx.BP = (int)ctx.Stack.Pop();//上层BP出栈
                            ctx.IP = (int)ctx.Stack.Pop();//返回地址出栈
                            int argNum = (int)ctx.Stack.Pop();//参数数量出栈
                            for (int i = 0; i < argNum; i++)
                            {
                                ctx.Stack.Pop();//参数出栈
                            }
                            ctx.Stack.Pop();//函数变量出栈
                            ctx.SP -= rets.Count + 4 + argNum;
                            break;
                        }
                    case OpCode.Print:
                        {
                            int argNum = (int)ctx.Stack.Peek(3);
                            for (int i = 0; i < argNum; i++)
                            {
                                Variant v = ctx.Stack.Peek(3 + argNum - i);
                                Console.Write($"{v}\t");
                            }
                            Console.WriteLine();
                            ctx.IP++;
                            break;
                        }
                    case OpCode.Len:
                        {
                            Variant v = ctx.Stack.Peek(4);
                            Variant r = v.arr.Length();
                            ctx.Stack.Push(r);
                            ctx.SP++;
                            ctx.IP++;
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }
}
