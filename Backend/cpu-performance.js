"use strict";
exports.__esModule = true;
exports.CPUPerformance = void 0;
var CPUPerformance = /** @class */ (function () {
    function CPUPerformance(cpu, MHZ) {
        this.cpu = cpu;
        this.MHZ = MHZ;
        this.prevTime = 0;
        this.prevCycles = 0;
        this.samples = new Float32Array(64);
        this.sampleIndex = 0;
    }
    CPUPerformance.prototype.reset = function () {
        this.prevTime = 0;
        this.prevCycles = 0;
        this.sampleIndex = 0;
    };
    CPUPerformance.prototype.update = function () {
        if (this.prevTime) {
            var delta = performance.now() - this.prevTime;
            var deltaCycles = this.cpu.cycles - this.prevCycles;
            var deltaCpuMillis = 1000 * (deltaCycles / this.MHZ);
            var factor = deltaCpuMillis / delta;
            if (!this.sampleIndex) {
                this.samples.fill(factor);
            }
            this.samples[this.sampleIndex++ % this.samples.length] = factor;
        }
        this.prevCycles = this.cpu.cycles;
        this.prevTime = performance.now();
        var avg = this.samples.reduce(function (x, y) { return x + y; }) / this.samples.length;
        return avg;
    };
    return CPUPerformance;
}());
exports.CPUPerformance = CPUPerformance;
