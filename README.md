# ToyCompiler

一个类似js的脚本语言解释器，使用C#语言编写，持续完善中。

命令行说明

```bash
> tc -e file.js 解释器模式
> tc -v file.js 虚拟机模式
> tc -d file.js 调试器模式
> tc -c file.js 编译器模式
> tc -i 交互模式(开发中)
> tc -t testcase 运行测试用例
```

## 2023-12-5 更新

- 调整代码结构，vm作为核心类处理所有操作，vm负责词法分析、生成语法树、编译字节码、执行字节码、与csharp互操作；
- 增加了宿主语言C#和js的互操作API，实现了C#调用js，js调用C#；

互操作测试用例
```csharp
public static int JsCallCs_Fibonacci(VM vm)
{
    //从栈上获取参数
    int n = (int)vm.API_ArgToNumber(0);
    int r = fib(n);
    vm.API_PushNumber(r);
    return 1;//返回值数量
}

public static void CsCallJs_Fibonacci(VM vm)
{
    Variant v = 10;
    vm.API_Call("fib", v);
    int ret = (int)vm.API_PeekNumber(0);
    Console.WriteLine(ret);
}

public void TestInteraction()
{
    //cs 调用 js
    string code = "function fib(n) {if(n<3){return n;} return fib(n-1)+fib(n-2);}";
    //js 调用 cs
    code += "print(\"js call cs fib(5)=\", csfib(5));";
    //注册cs函数
    RegFunc(Interaction.JsCallCs_Fibonacci, "csfib");
    Parse(code);
    Visit(tree);
    Dump();
    Run();
    //调用js函数
    Interaction.CsCallJs_Fibonacci(this);
}    
```

## 2023-11-30 更新

- 增加了一个栈式虚拟机作为后端，工作原理如下：遍历语法树生成指令代码，解释执行指令代码；
- 增加了虚拟机调试器，支持's'单步执行，'r'执行，'b'断点，'p'打印变量，'sc'查看作用域，'bt'查看调用栈；
- 完善了测试用例；

指令集如下：
```csharp
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
        public const int JumpTrue = 17;
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
        public const int Clear = 37;//清空栈帧

```
 
2022-9-6 更新
---

目前实现的语法规则有
```c

/* 
 * exp = assign |vardecl| forin
 * assign = condition | condition assign_op assign
 * assign_op = '='|'*=' |'/='|'%='|'+='|'-='
 * vardecl = 'var' ID ('=' condition)?
 * forin = 'var' params 'in' postfix
 * condition = or | or '?' exp ':' condition
 * or = and ('||' and )*
 * and = equal ('&&' equal)*
 * equal = relation ('=='|'!=' relation)*
 * relation = add (relation_op add)*
 * relation_op = '<'|'>'|'<='|'>='
 * add = mul ('+'|'-' mul)*
 * mul = unary  ('/'|'*'|'%' unary)*
 * unary = postfix | '++' unary | '--' unary | '!+-' unary 
 * postfix = prim | postfix '++'| postfix '--' |postfix '(' args? ')'| postfix '[' exp ']'| postfix '.' ID
 * prim = ID | NUM | STR | '(' exp ')' | arr | obj
 * arr = '[' args? ']'
 * obj = '{' kvs? '}'
 * args = exp (',' exp)*
 * kvs = ID '=' exp (',' ID '=' exp)*
 * params = ID (, ID)*
 * 
 */
 
 /*
 * stat =  exp_stat | compound_stat | if_stat | while_stat | for_stat | jump_stat | fun_stat
 * exp_stat : exp ';' | ';'
 * compound_stat = '{' stat* '}'
 * if_stat = 'if' '(' exp ')' stat ('else' stat )?
 * while_stat = 'while' '(' exp ')' stat
 * for_stat = 'for' '(' exp? ';' exp? ';' exp? ')' stat | 'for' '(' exp 'in' exp ')' stat
 * jump_stat = 'break' ';'|'continue' ';' |'return' exp? ';'
 * fun_stat = 'function' ID '(' args ')' '{' stat* '}'
 * 
 */
```

现有功能的测试用例如下：
```js
//DONE：bool值，浮点数，字符串，变量声明，四则运算，逻辑运算，控制语句，数组，对象，函数声明，函数调用，变量作用域和生命周期


//变量声明
var a = 42;
var b = (a + 3) / 6;
var cc = a >= b * 5 ? (a <= b * 6 ? b : 1) : 0;
print(a, b, cc);

//for循环
a = 50;
b = 0;
//for循环
for (var i = 0; i < 10; i++) {
    a -= i * 3;//0 3 6 9 12 15 18
    if (a <= 0) {
        break;
    } else {
        b++;
        continue;
    }
}
print(a, b);

var s = "abc"; //字符串
//while循环
while (true) {
    s = s + s;
    break;
}
print(s);

//函数声明
function f(a, b, c) {
    var d = a + (b - c) * 10;
    return d;
}

//函数调用
var d = f(a, b, 2);
print(d);

//递归函数
function fib(n) {
    if (n < 3) { return n; }
    return fib(n - 1) + fib(n - 2);
}
print(fib(10));

//数组声明
var arr = [1, 2, 3, "a", "b", "c"];
//数组访问
for (var i = 0; i < len(arr); i++) {
    print(arr[i]);
}
//for in 循环
for (var i, v in arr) {
    print(i, v)
}

//对象声明
var t = { a=1, b=2, c="123", d=[1, 2, 3, 4, 5], e={ x=100, y=200 } };
//字段访问
print(t.a, t.c, t.e.x);
//对象访问
for (var k, v in t) {
    print(k, v);
}

```
