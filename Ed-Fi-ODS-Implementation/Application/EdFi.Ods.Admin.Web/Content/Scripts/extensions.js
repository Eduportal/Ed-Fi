// Function Extensions
Function.prototype.method = function (name, func) {
    if (!func) {
        return this.prototype[name];
    }
    if (!this.prototype[name]) {
        this.prototype[name] = func;
        return this;
    }
    return undefined;
};

// String Extensions
String.prototype.method = Function.prototype.method;

String.method('endsWith', function (suffix) {
    return this.indexOf(suffix, this.length - suffix.length) !== -1;
});

String.method('replaceNewlinesWithBreaks', function () {
    return this.replace('\r\n', '<br/>').replace('\n', '<br/>');
});