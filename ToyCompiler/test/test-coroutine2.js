//生产者-消费者
coroutine productor() {
    var i = 0;
    while (true) {
        coyield(i++);
    }
}

function consumer() {
    var j = 0;
    while (true) {
        j = coresume(productor);
        print(j);
    }
}
consumer();