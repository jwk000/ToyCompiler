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
        public const int NJump = 17;
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
        public const int CallCS = 33;//调用cs函数
        public const int SLoad = 34;//栈上的变量压入栈顶
        public const int Print = 35;
        public const int Len = 36;
        public const int Clear = 37;//清空栈帧
        public const int CoYield = 38;
        public const int CoResume = 39;

        public static string[] OpCodeNames = new string[]
        {
            "NOP","PUSH","ADD","SUB","MUL","DIV","REM","EQ","NE","LT","LE","GT","GE","AND","OR","NOT","JUMP","NJUMP",
            "CALL","RET","HALT","ENTERSCOPE","LEAVESCOPE","NEXT","POP","LOAD","STORE","ASSIGN","INDEX","DOT","NEWARRAY","NEWOBJ",
            "ENUM","CALLCS","SLOAD","PRINT","LEN","CLEAR","COYIELD","CORESUME"
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
        public int CodeLine;
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
            switch (Op)
            {
                case OpCode.Push:
                    param = OpVar.ToString();
                    break;
                case OpCode.Jump:
                case OpCode.NJump:
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
            return $"{CodeLine,4:0000} {OpCode.OpCodeNames[Op],-12} {param}";
        }

        public static Instruction NOP = new Instruction(0);
        public static Stack<JumpLabel> JumpLabels = new Stack<JumpLabel>();
    }

}
