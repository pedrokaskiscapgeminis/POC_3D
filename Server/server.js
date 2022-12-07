const express = require("express");
const keys = require("./config/keys");
const app = express();
const bodyParser = require('body-parser');

//Parser 

app.use(bodyParser.urlencoded({extended: false}));


//Setup DB
const mongoose = require("mongoose");
mongoose.connect(keys.mongoURI, {
  useNewUrlParser: true,
  useUnifiedTopology: true,
});

//Setup database models
const User = require("./model/User");


//Setup the routes
require('./routes/authenticationRoutes')(app)

app.listen(keys.port, () => {
  console.log("Server is ready!");
});
