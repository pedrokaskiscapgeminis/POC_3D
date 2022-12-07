const mongoose = require('mongoose');
const {Schema} = mongoose;

const userSchema = new Schema({

    //Campos

    username: String,
    password: String,

    lastAuthentication: Date,
})

module.exports = mongoose.model.Users || mongoose.model('Users', userSchema )
