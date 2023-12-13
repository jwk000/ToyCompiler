using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ToyCompiler
{
    class Program
    {
        //tc -e file.js 解释器模式
        //tc -v file.js 虚拟机模式
        //tc -d file.js 调试器模式
        //tc -c file.js 编译器模式
        //tc -i 交互模式(开发中)
        //tc -t testcase 运行测试用例
        static void Main(string[] args)
        {
            string runMode = args[0];

            VM vm = new VM();

            if (runMode == "-e")
            {
                //直接解释执行
                string script = File.ReadAllText(args[1]);
                vm.Exec(script);
            }
            else if (runMode == "-v")
            {
                //编译成指令执行
                string script = File.ReadAllText(args[1]);
                vm.Compile(script);
                vm.Run();
            }
            else if (runMode == "-d")
            {
                //带调试器执行指令
                string script = File.ReadAllText(args[1]);
                vm.AttachDebugger();
                vm.Compile(script);
                vm.Dump();
                vm.Run();
            }
            else if (runMode == "-c")
            {
                //编译成指令执行
                string script = File.ReadAllText(args[1]);
                vm.Compile(script);
                vm.Dump();
            }
            else if (runMode == "-i")
            {
                //交互式执行
                vm.REPL();
            }
            else if(runMode == "-t")
            {
                //测试用例
                string testcase = args[1];
                if (testcase == "interact")
                {
                    vm.TestInteraction();
                }
            }
            Console.WriteLine("press any key to exit...");
            Console.Read();
        }
    }



}
