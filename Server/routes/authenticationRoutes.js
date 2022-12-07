const mongoose = require("mongoose");

const User = require("../model/User");
module.exports = (app) => {
  //Routes
  app.post("/users", async (req, res) => {
    console.log(req.body);
    const { username, password } = req.body;
    if (username == "" || password == "") {
      res.send(400,"Invalid credentials");
      return;
    }

    let userAccount = await User.findOne({ username: username });
    if (userAccount != null) {
      if (password == userAccount.password) {
        userAccount.lastAuthentication = Date.now();
        await userAccount.save();

        res.send(200,userAccount);
        return;
      }
    }

    res.send(400,"Invalid credentials");
    return;
  });

  app.post("/users/create", async (req, res) => {
    console.log(req.body);
    const { username, password } = req.body;
    if (username == "" || password == "") {
      res.send(400,"Invalid credentials");
      return;
    }

    let userAccount = await User.findOne({ username: username });
    if (userAccount == null) {
      console.log("Create new account");

      let newUser = new User({
        username: username,
        password: password,

        lastAuthentication: Date.now(),
      });

      await newUser.save();

      res.send(200,newUser);
      return;
    } else {
      res.send(400,"Username is already taken");
      
    }
    return
  });
};
