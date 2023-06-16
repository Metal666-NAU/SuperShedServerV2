using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

using System;
using System.Security.Cryptography;

namespace SuperShedServerV2;

public static class Database {

	public static MongoClient? MongoClient { get; set; }

	public static IMongoDatabase? MainDatabase { get; set; }

	public static bool Initialize() {

		ConventionRegistry.Register("CamelCase", new ConventionPack {

			new CamelCaseElementNameConvention()

		}, type => true);

		string? connectionString = Environment.GetEnvironmentVariable("SUPERSHED_MONGODB_URI");

		if(connectionString == null) {

			Output.Error("SUPERSHED_MONGODB_URI environment variable not set!");

			return false;

		}

		MongoClient = new(connectionString);

		MainDatabase = MongoClient.GetDatabase("warehouse", new() { });

		/*BsonDocument? document = await MongoClient.GetDatabase("sample_mflix")
											.GetCollection<BsonDocument>("movies")
											.FindAsync(Builders<BsonDocument>.Filter.Eq("title", "Back to the Future"))
											.First();

		Console.WriteLine(document);*/

		return true;

	}

	/*public static bool TryGetUser(string email, string password, out Collections.User? user) {

		user = MainDatabase!.GetCollection<Collections.User>(Collections.USERS)
							.Find(user =>
									user.Email!.Equals(email) &&
									user.Password!.Equals(password))
							.FirstOrDefault();

		return user != null;

	}

	public static string LogUserIn(Collections.User user) {

		string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

		MainDatabase!.GetCollection<Collections.LoginToken>(Collections.LOGIN_TOKENS)
						.InsertOne(new() {

							Token = token,
							UserId = user.Id

						});

		return token;

	}

	public static bool TryGetUser(ObjectId userId, out Collections.User? user) {

		user = MainDatabase!.GetCollection<Collections.User>(Collections.USERS)
							.Find(user => user.Id == userId)
							.FirstOrDefault();

		return user != null;

	}

	public static bool ValidateLoginToken(string token, out Collections.User? user) {

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

	}*/

	public static Collections.Worker? GetWorker(ObjectId workerId) =>
		MainDatabase!.GetCollection<Collections.Worker>(Collections.WORKERS)
						.Find(worker => worker.Id == workerId)
						.FirstOrDefault();

	public static Collections.Admin? GetAdmin(ObjectId adminId) =>
		MainDatabase!.GetCollection<Collections.Admin>(Collections.ADMINS)
						.Find(admin => admin.Id == adminId)
						.FirstOrDefault();

	public static Collections.Admin? GetAdmin(string email, string password) =>
		MainDatabase!.GetCollection<Collections.Admin>(Collections.ADMINS)
						.Find(admin => email.Equals(admin.Email) &&
													password.Equals(admin.Password))
						.FirstOrDefault();

	public static string FindOrCreateAuthToken(ObjectId userId) {

		string? token = MainDatabase!.GetCollection<Collections.AuthToken>(Collections.AUTH_TOKENS)
										.Find(authToken => authToken.UserId.Equals(userId))
										.FirstOrDefault()?
										.Token;

		if(token == null) {

			token = GenerateAuthToken();

			MainDatabase!.GetCollection<Collections.AuthToken>(Collections.AUTH_TOKENS)
							.InsertOne(new() {

								Token = token,
								UserId = userId

							});

		}

		return token;

	}

	public static ObjectId? GetUserId(string authToken) =>
		MainDatabase!.GetCollection<Collections.AuthToken>(Collections.AUTH_TOKENS)
						.Find(token => authToken.Equals(token.Token))
						.FirstOrDefault()?
						.UserId;

	private static string GenerateAuthToken() =>
		Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

	public static class Collections {

		public const string WORKERS = "workers";
		public const string ADMINS = "admins";
		public const string AUTH_TOKENS = "auth_tokens";
		public const string GOODS = "goods";

		/*public class User {

			public virtual ObjectId Id { get; set; }
			public virtual string? Email { get; set; }
			public virtual string? Password { get; set; }
			//public virtual ObjectId RoleId { get; set; }

		}*/

		public class Worker {

			public virtual ObjectId Id { get; set; }

		}

		public class Admin {

			public virtual ObjectId Id { get; set; }
			public virtual string? Email { get; set; }
			public virtual string? Password { get; set; }

		}

		public class AuthToken {

			public virtual ObjectId Id { get; set; }
			public virtual ObjectId UserId { get; set; }
			public virtual string? Token { get; set; }

		}

	}

}