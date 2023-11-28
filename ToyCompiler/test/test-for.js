var a = 50;
var b = 0;

//for循环
for (var i = 0; i < 10; i++) {
    a -= i*3;//0 3 6 9 12 15 18
    if (a <= 0) {
        break;
    } else {
        b++;
        continue;
    }
}
print(a, b);
