const mongoose = require("mongoose");
import { Request, Response } from 'express';
//Importamos el modelo de usuario
const bcrypt = require("bcrypt")
const User = require("../model/User");

//Interfaz de respuesta del usuario
interface UserResponse {
  _id: string;
  username: string;
  lastAuthentication: Date;
}

module.exports = (app: any) => {
  //Routes
  app.post("/users", async (req: Request, res: Response) => {
    //Obtenemos el usuario y la contraseña del mensaje
    const password: string = req.body.password;
    const username: string = req.body.username;
    console.log(req.body)

    //Comprobamos que los campos no son vacíos
    if (username == "" || password == "") {
      res.status(400).send("Invalid credentials");
      return;
    }

    //Obtenemos el usuario y comprobamos que los datos son correctos
    let userAccount = await User.findOne({ username: username });

    if (userAccount != null) {
      if (await bcrypt.compareSync(password, userAccount.password)) {
        userAccount.lastAuthentication = Date.now();
        await userAccount.save();

        //Creamos la respuesta y la devolvemos
        let userResponse: UserResponse = {
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
  });

  app.post("/users/create", async (req: Request, res: Response) => {
    //Obtenemos el usuario y la contraseña del mensaje
    const password: string = req.body.password;
    const username: string = req.body.username;

    //Comprobamos que no esté vacío
    if (username == "" || password == "") {
      res.status(400).send("Invalid credentials");
      return;
    }

    //Comprobamos que exista el usuario, si no existe se crea la nueva cuenta
    let userAccount = await User.findOne({ username: username });
    if (userAccount == null) {
      console.log("Create new account");

      //Encriptamos la contraseña

      let hashedPassword = await bcrypt.hash(password,10);

      let newUser = new User({
        username: username,
        password: hashedPassword,

        lastAuthentication: Date.now(),
      });

      await newUser.save();

      //Creamos la respuesta y la devolvemos
      let userResponse: UserResponse = {
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
  });
};
