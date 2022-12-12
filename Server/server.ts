
const express = require("express") ;
const keys = require( "./config/keys");
import mongoose from "mongoose";

//Variable principal
const app = express();


//Parser 


app.use(express.urlencoded({extended:false}));


//Setup DB

mongoose.connect(keys.mongoURI);

//Setup database models
const User = require("./model/User");


//Setup the routes
require('./routes/authenticationRoutes')(app)

app.listen(keys.port, () => {
  console.log("Server is ready!");
});
