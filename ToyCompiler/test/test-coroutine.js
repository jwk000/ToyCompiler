//Ð­³Ì
coroutine foo (a) {
    print(a);
    var b = coyield(1);
    print(b);
}

var x = coresume(foo,2);
print(x);
coresume(foo, 3);



