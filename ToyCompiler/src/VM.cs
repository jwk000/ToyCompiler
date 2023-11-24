using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{
    
    class OpCode
    {
        const int Push = 1; //Push 100
        const int Add = 2; //Add
        const int Sub = 3; //Sub
        const int Mul = 4; //Mul
        const int Div = 5; //Div
        const int Rem = 6; //Rem
        const int EQ = 7; //EQ
        const int NE = 8; //NEQ
        const int LT = 9; //LT
        const int LE = 10;//LE
        const int GT = 11;//GT
        const int GE = 12;//GE
        const int And = 13;//And
        const int Or = 14; //Or
        const int Not = 15;//Not
        const int Jump = 16;//JMP
        const int NJump = 17;//NJMP
        const int Call = 18;//Call
        const int Ret = 19; //RET
        const int Halt = 20;//Halt

    }



    class Instruction
    {
        public int OpCode;
        public int ParamNum;

    }

    
    class VM
    {

    }
}
