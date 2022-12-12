import { Schema, model } from "mongoose";

// 1. Create an interface representing a document in MongoDB.
interface IUser {
  username: string;
  password: string;
  lastAuthentication: Date;
}

const userSchema = new Schema<IUser>({
  //Campos

  username: String,
  password: String,

  lastAuthentication: Date,
});

const User = model<IUser>("Users", userSchema);
module.exports = User;
