"use strict";
exports.__esModule = true;
exports.formatTime = void 0;
function zeroPad(value, length) {
    var sval = value.toString();
    while (sval.length < length) {
        sval = '0' + sval;
    }
    return sval;
}
function formatTime(seconds) {
    var ms = Math.floor(seconds * 1000) % 1000;
    var secs = Math.floor(seconds % 60);
    var mins = Math.floor(seconds / 60);
    return "".concat(zeroPad(mins, 2), ":").concat(zeroPad(secs, 2), ".").concat(zeroPad(ms, 3));
}
exports.formatTime = formatTime;
