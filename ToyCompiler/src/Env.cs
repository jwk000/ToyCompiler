using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{

    /* 
     * exp = assign
     * assign = condition | unary assign_op assign
     * condition = or | or '?' exp ':' condition
     * assign_op = '='|'*=' |'/='|'%='|'+='|'-='|'<<='|'>>='|'&='|'^='|'|='
     * or = and | or '||' and
     * and = bitor | and '&&' bitor
     * bitor = bitxor |  bitor '|' bitxor
     * birxor= bitand | bitxor '^' bitand
     * bitand = equal | bitand '&' equal
     * equal = relation | equal '==' relation | equal '!=' relation
     * relation = shift | relation relation_op shift
     * relation_op = '<'|'>'|'<='|'>='
     * shift = add | shift '<<' | '>>' add
     * add = mul | add '+-' mul
     * mul = unary | mul '/*%' unary
     * unary = postfix | '++' unary | '--' unary | '~!+-' unary
     * postfix = prim | postfix '[' exp ']' | postfix '(' args ')' | postfix '++'| postfix '--'
     * prim = ID | NUM | STR | '(' exp ')'
     * ------------------------------------------------------------------
     * 
     * stat = label_stat | exp_stat | compound_stat | select_stat | iterat_stat | jump_stat
     * label_stat : id ':' stat| 'case' const_exp ':' stat| 'default' ':' stat
     * exp_stat : exp ';' | ';'
     * compound_stat = '{' stat_list '}'
     * statlist = stat | statlist stat
     * select_stat = 'if' '(' exp ')' stat | 'if' '(' exp ')' stat 'else' stat | 'switch' '(' exp ')' stat
     * iterat_stat = 'while' '(' exp ')' stat| 'for' '(' exp? ';' exp? ';' exp? ')' stat
     * 
     */


    static class Env
    {
        public static List<Variant> RunStack = new List<Variant>();//函数运行栈
        public static Scope LocalScope;//当前作用域
        public static Scope GlobalScope = new Scope();//全局作用域

        public static void RegisterBuildinFunctions()
        {
            RegFun(Buildin.Print());
            RegFun(Buildin.Len());

        }

        static void RegFun(FunStat fun)
        {
            Variant v = new Variant();
            v.variantType = VariantType.Function;
            v.fun = fun;
            v.id = fun.mFunID.desc;
            GlobalScope.AddVariant(v);
        }

    }


}
