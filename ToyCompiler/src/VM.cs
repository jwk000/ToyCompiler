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
        public const int NJump = 17;//NJMP
        public const int Call = 18;//Call
        public const int Ret = 19; //RET
        public const int Halt = 20;//Halt
        public const int EnterScope = 21;
        public const int LeaveScope = 22;
        public const int Next = 23;//迭代成功执行循环，失败跳出循环
        public const int Pop = 24;
    }

    //记录用来跳转的标签
    struct JumpLabel
    {
        public int ContinueLabel;
        public int BreakLabel;
        public JumpLabel(int c, int b) { ContinueLabel = c;BreakLabel = b; }
    }
    class Instruction
    {
        public int OpCode;
        public int OpInt;
        public double OpDouble;
        public string OpString;
        public Instruction(int opCode)
        {
            OpCode = opCode;
        }
        public static Instruction NOP = new Instruction(0);
        public static Stack<JumpLabel> JumpLabels = new Stack<JumpLabel>();
        public static int ReturnLabel = -1;//返回下一条指令地址
    }

    

    class Context
    {
        public int BP = -1;
        public int SP = -1;
        public int IP = 0;
        public List<Variant> Stack = new List<Variant>();
        public List<Instruction> Code;
        public Scope LocalScope;//当前作用域
        public Scope GlobalScope = new Scope();//全局作用域

    }
    class VM
    {
        Context ctx = new Context();

        public void Parse(StatList tree)
        {
            tree.OnVisit(ctx.Code);
        }

        public void Exec()
        {
            for(int i = 0; i < ctx.Code.Count; i++)
            {
                Instruction ins = ctx.Code[i];
                if(ins.OpCode == OpCode.Halt)
                {
                    break;
                }
                switch (ins.OpCode)
                {
                    //todo
                }
            }
        }
    }
}
