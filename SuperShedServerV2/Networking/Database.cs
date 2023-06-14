using MongoDB.Bson;
using MongoDB.Driver;

using System;
using System.Security.Cryptography;

namespace SuperShedServerV2;

public class Database {

	public virtual MongoClient? MongoClient { get; set; }

	public virtual IMongoDatabase? MainDatabase { get; set; }

	public virtual bool Initialize() {

		string? connectionString = Environment.GetEnvironmentVariable("SUPERSHED_MONGODB_URI");

		if(connectionString == null) {

			Output.Error("SUPERSHED_MONGODB_URI environment variable not set!");

			return false;

		}

		MongoClient = new(connectionString);

		MainDatabase = MongoClient.GetDatabase("warehouse");

		/*BsonDocument? document = await MongoClient.GetDatabase("sample_mflix")
											.GetCollection<BsonDocument>("movies")
											.FindAsync(Builders<BsonDocument>.Filter.Eq("title", "Back to the Future"))
											.First();

		Console.WriteLine(document);*/

		return true;

	}

	public virtual bool TryGetUser(string email, string password, out Collections.User? user) {

		user = MainDatabase!.GetCollection<Collections.User>(Collections.USERS)
							.Find(user =>
									user.Email!.Equals(email) &&
									user.Password!.Equals(password))
							.FirstOrDefault();

		return user != null;

	}

	public virtual string LogUserIn(Collections.User user) {

		string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

		MainDatabase!.GetCollection<Collections.LoginToken>(Collections.LOGIN_TOKENS)
						.InsertOne(new() {

							Token = token,
							UserId = user.Id

						});

		return token;

	}

	public virtual bool TryGetUser(ObjectId userId, out Collections.User? user) {

		user = MainDatabase!.GetCollection<Collections.User>(Collections.USERS)
							.Find(user => user.Id == userId)
							.FirstOrDefault();

		return user != null;

	}

	public virtual bool ValidateLoginToken(string token, out Collections.User? user) {

		Collections.LoginToken? loginToken = MainDatabase!.GetCollection<Collections.LoginToken>(Collections.LOGIN_TOKENS)
															.Find(loginToken =>
																	loginToken.Token != null &&
																	loginToken.Token.Equals(token))
															.FirstOrDefault();

		if(loginToken == null) {

			user = null;

			return false;

		}

		return TryGetUser(loginToken.UserId, out user);

	}

	public static class Collections {

		public const string USERS = "users";
		public const string LOGIN_TOKENS = "login_tokens";
		public const string GOODS = "goods";

		public class User {

			public virtual ObjectId Id { get; set; }
			public virtual string? Email { get; set; }
			public virtual string? Password { get; set; }
			//public virtual ObjectId RoleId { get; set; }

		}

		public class LoginToken {

			public virtual ObjectId Id { get; set; }
			public virtual ObjectId UserId { get; set; }
			public virtual string? Token { get; set; }

		}

	}

}