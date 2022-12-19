"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const express = require("express");
const keys = require("./config/keys");
const cors = require('cors');
const mongoose_1 = __importDefault(require("mongoose"));
//Variable principal
const app = express();
app.use(cors());
//Parser 
app.use(express.urlencoded({ extended: true }));
app.use(express.json());
//Setup DB
mongoose_1.default.connect(keys.mongoURI);
//Setup database models
const User = require("./model/User");
//Setup the routes
require('./routes/authenticationRoutes')(app);
app.listen(keys.port, () => {
    console.log("Server is ready!");
});
//# sourceMappingURL=server.js.map