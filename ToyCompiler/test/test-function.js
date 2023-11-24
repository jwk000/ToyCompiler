
//函数声明
function f(a, b, c) {
    var d = a + (b - c) * 10;
    return d;
}

//函数调用
var d = f(10, 20, 8);
print(d);


//递归函数
function fib(n) {
    if (n < 3) { return n; }
    return fib(n - 1) + fib(n - 2);
}
print(fib(10));
