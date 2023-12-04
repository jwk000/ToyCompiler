using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCompiler
{

    static class Env
    {
        public static List<Variant> RunStack = new List<Variant>();//函数运行栈
        public static Scope LocalScope;//当前作用域
        public static Scope GlobalScope = new Scope();//全局作用域

        public static void RegisterBuildinFunctions()
        {
            RegFun(Interaction.Print());
            RegFun(Interaction.Len());

        }

        static void RegFun(FunStat fun)
        {
            Variant v = new Variant();
            v.variantType = VariantType.Function;
            v.fun = fun;
            v.id = fun.mFunID.desc;
            GlobalScope.SetVariant(v);
        }

    }


}
