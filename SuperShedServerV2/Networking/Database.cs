using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

using System;
using System.Collections.Generic;
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

		MainDatabase = MongoClient.GetDatabase("warehouse");

		return true;

	}

	public static Collections.Worker? GetWorker(ObjectId workerId) =>
		MainDatabase!.GetCollection<Collections.Worker>(Collections.WORKERS)
						.Find(worker => worker.Id == workerId)
						.FirstOrDefault();

	public static List<Collections.Worker> GetWorkers() =>
		MainDatabase!.GetCollection<Collections.Worker>(Collections.WORKERS)
						.AsQueryable()
						.ToList();

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

								UserId = userId,
								Token = token

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

		public class Worker : DatabaseObjectBase {

			public virtual string? Name { get; set; }

		}

		public class Admin : DatabaseObjectBase {

			public virtual string? Email { get; set; }
			public virtual string? Password { get; set; }

		}

		public class AuthToken : DatabaseObjectBase {

			public virtual ObjectId UserId { get; set; }
			public virtual string? Token { get; set; }

		}

		public abstract class DatabaseObjectBase {

			public virtual ObjectId Id { get; set; }

			public virtual string StringId => Id.ToString()!;

		}

	}

}