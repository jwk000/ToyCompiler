var a = 50;
var b = 20;

//for循环
for (var i = 0; i < 10; i++) {
    a -= i*3;
    if (a <= 0 || b - a > 5) {
        break;
    } else {
        b++;
        continue;
    }
}
print(a, b);
