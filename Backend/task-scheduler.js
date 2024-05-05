"use strict";
exports.__esModule = true;
exports.MicroTaskScheduler = void 0;

var MicroTaskScheduler = /** @class */ (function () {
    function MicroTaskScheduler() {
        this.executionQueue = [];
        this.stopped = true;
    }

    MicroTaskScheduler.prototype.start = function () {
        if (this.stopped) {
            this.stopped = false;
            console.log("Task scheduler started.");
            this.runTasks();
        }
    };

    MicroTaskScheduler.prototype.stop = function () {
        this.stopped = true;
        console.log("Task scheduler stopped.");
    };

    MicroTaskScheduler.prototype.postTask = function (fn) {
        if (!this.stopped) {
            this.executionQueue.push(fn);
            // console.log("Task added.");
        }
    };

    MicroTaskScheduler.prototype.runTasks = function () {
        var _this = this;
        setImmediate(() => {
            if (!_this.stopped && _this.executionQueue.length > 0) {
                // console.log("Running tasks...");
                var task = _this.executionQueue.shift();
                // console.log("Executing task...");
                task();
                _this.runTasks(); // Schedule the next task
            }
        });
    };

    return MicroTaskScheduler;
}());

exports.MicroTaskScheduler = MicroTaskScheduler;
