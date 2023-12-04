//bool值，浮点数，字符串，变量声明，四则运算，逻辑运算，控制语句
//数组，对象，函数声明，函数调用，变量作用域和生命周期。


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
