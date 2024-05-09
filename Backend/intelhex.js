"use strict";
/**
 * Minimal Intel HEX loader
 * Part of AVR8js
 *
 * Copyright (C) 2019, Uri Shaked
 */
exports.__esModule = true;
exports.loadHex = void 0;
function loadHex(source, target) {
    for (var _i = 0, _a = source.split('\n'); _i < _a.length; _i++) {
        var line = _a[_i];
        if (line[0] === ':' && line.substr(7, 2) === '00') {
            var bytes = parseInt(line.substr(1, 2), 16);
            var addr = parseInt(line.substr(3, 4), 16);
            for (var i = 0; i < bytes; i++) {
                target[addr + i] = parseInt(line.substr(9 + i * 2, 2), 16);
            }
        }
    }
}
exports.loadHex = loadHex;
