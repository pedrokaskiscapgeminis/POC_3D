"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const mongoose = require("mongoose");
//Importamos el modelo de usuario
const bcrypt = require("bcrypt");
const User = require("../model/User");
module.exports = (app) => {
    //Routes
    app.post("/users", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
        //Obtenemos el usuario y la contraseña del mensaje
        const password = req.body.password;
        const username = req.body.username;
        console.log(req.body);
        //Comprobamos que los campos no son vacíos
        if (username == "" || password == "") {
            res.status(400).send("Invalid credentials");
            return;
        }
        //Obtenemos el usuario y comprobamos que los datos son correctos
        let userAccount = yield User.findOne({ username: username });
        if (userAccount != null) {
            if (yield bcrypt.compareSync(password, userAccount.password)) {
                userAccount.lastAuthentication = Date.now();
                yield userAccount.save();
                //Creamos la respuesta y la devolvemos
                let userResponse = {
                    username: userAccount.username,
                    _id: userAccount._id,
                    lastAuthentication: userAccount.lastAuthentication,
                };
                res.status(200).send(userResponse);
                return;
            }
        }
        res.status(400).send("Invalid credentials");
        return;
    }));
    app.post("/users/create", (req, res) => __awaiter(void 0, void 0, void 0, function* () {
        //Obtenemos el usuario y la contraseña del mensaje
        const password = req.body.password;
        const username = req.body.username;
        //Comprobamos que no esté vacío
        if (username == "" || password == "") {
            res.status(400).send("Invalid credentials");
            return;
        }
        //Comprobamos que exista el usuario, si no existe se crea la nueva cuenta
        let userAccount = yield User.findOne({ username: username });
        if (userAccount == null) {
            console.log("Create new account");
            //Encriptamos la contraseña
            let hashedPassword = yield bcrypt.hash(password, 10);
            let newUser = new User({
                username: username,
                password: hashedPassword,
                lastAuthentication: Date.now(),
            });
            yield newUser.save();
            //Creamos la respuesta y la devolvemos
            let userResponse = {
                username: newUser.username,
                _id: newUser._id,
                lastAuthentication: newUser.lastAuthentication,
            };
            res.status(201).send(userResponse);
            return;
        }
        //Informamos de que existe el usuario
        else {
            res.status(400).send("Username is already taken");
        }
        return;
    }));
};
//# sourceMappingURL=authenticationRoutes.js.map