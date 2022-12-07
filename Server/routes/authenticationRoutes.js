const mongoose = require("mongoose");

//bcrypt
const bcrypt = require("bcrypt");
const saltRounds = 10;



const User = require("../model/User");
module.exports = (app) => {
  //Routes
  app.post("/users", async (req, res) => {
    
    const { username, password } = req.body;
    if (username == "" || password == "") {
      res.status(400).send("Invalid credentials");
      return;
    }
  
    let userAccount = await User.findOne({ username: username });
    if (userAccount != null) {
       
      if (await bcrypt.compare(password,userAccount.password)) {
        userAccount.lastAuthentication = Date.now();
        await userAccount.save();

        res.status(200).send({

          id: userAccount._id,
          username: userAccount.username,
          lastAuthentication: userAccount.lastAuthentication,

        
        });
        return;
      }
    }

    res.status(400).send("Invalid credentials");
    return;
  });

  app.post("/users/create", async (req, res) => {
    console.log(req.body);
    const { username, password } = req.body;
    if (username == "" || password == "") {
      res.status(400).send("Invalid credentials");
      return;
    }

    let userAccount = await User.findOne({ username: username });
    if (userAccount == null) {
      console.log("Create new account");

      //Hashing password

      let hashPassword = await bcrypt.hash(password, saltRounds);

      let newUser = new User({
        username: username,
        password: hashPassword,

        lastAuthentication: Date.now(),
      });

      await newUser.save();

      res.status(201).send({

        id: newUser._id,
        username: newUser.username,
        lastAuthentication: newUser.lastAuthentication,

      
      });
      return;
    } else {
      res.status(400).send("Username is already taken");
      
    }
    return
  });
};
