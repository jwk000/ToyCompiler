# ToyCompiler

一个类似js的脚本语言解释器，使用C#语言编写，持续完善中。

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
//TODO 函数作为第一类值，闭包，类

//变量声明
var a = 42;
var b = (a + 3) / 6;
var c = a >= b * 5 ? (a <= b * 6 ? b : 1) : 0;
print(a, b, c);

//for循环
for (var i = 0; i < 10; i++) {
    a -= i;
    if (a <= 0 || b - a > 5) {
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
print(s)

//函数声明
function f(a, b, c) {
    var d = a + (b - c) * 10;
    return d;
}

//函数调用
var d = f(a, b, 2);
print(d);

//数组声明
var arr = [1, 2, 3, "a", "b", "c"];
//数组访问
for (var i = 0; i < arr.len(); i++) {
    print(arr[i]);
}

//对象声明
var t = { a=1, b=2, c="123", d=[1, 2, 3, 4, 5] };
//对象访问
for (var k, v in t) {
    print(k, v);
}
```
