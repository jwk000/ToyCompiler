
//对象声明
var t = { a=1, b=2, c="123", d=[1, 2, 3, 4, 5], e={ x=100, y=200 } };
//对象访问
for (var k, v in t) {
    print(k, v);
}
