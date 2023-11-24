//bool值，浮点数，字符串，变量声明，四则运算，逻辑运算，控制语句
//数组，对象，函数声明，函数调用，变量作用域和生命周期，
//TODO 函数作为第一类值，闭包，类，

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
var str = "arr=[";
for (var i = 0; i < len(arr); i++) {
    str = str + arr[i];
}

//对象声明
var t = { a=1, b=2, c="123", d=[1, 2, 3, 4, 5] };
//对象访问
for (var k, v in t) {
    print(k, v);
}
