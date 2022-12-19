
const express = require("express") ;
const keys = require( "./config/keys");
const cors = require('cors');

import mongoose from "mongoose";

//Variable principal
const app = express();
app.use(cors())

//Parser 


app.use(express.urlencoded({extended:true}));
app.use(express.json())


//Setup DB

mongoose.connect(keys.mongoURI);

//Setup database models
const User = require("./model/User");


//Setup the routes
require('./routes/authenticationRoutes')(app)

app.listen(keys.port, () => {
  console.log("Server is ready!");
});
