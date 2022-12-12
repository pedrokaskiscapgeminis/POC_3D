"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const mongoose_1 = require("mongoose");
const userSchema = new mongoose_1.Schema({
    //Campos
    username: String,
    password: String,
    lastAuthentication: Date,
});
const User = (0, mongoose_1.model)("Users", userSchema);
module.exports = User;
//# sourceMappingURL=User.js.map