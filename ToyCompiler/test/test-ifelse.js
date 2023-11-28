
var a = 99;
if (a < 20) {
    print("a<20");
} else {
    print("a>=20");
}

var x = 100;
var n = 100;
while (n-->0) {
    if (a == x) {
        break;
    } else if (a < x) {
        x = x / 2;
        continue;
    } else {
        x = (x + x * 2) / 2;
    }
    print("x=", x);
}
print("a = ", x);
