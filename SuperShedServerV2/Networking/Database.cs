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
		GetCollection<Collections.Worker>(Collections.WORKERS);

	public static Collections.Admin? GetAdmin(ObjectId adminId) =>
		MainDatabase!.GetCollection<Collections.Admin>(Collections.ADMINS)
						.Find(admin => admin.Id == adminId)
						.FirstOrDefault();

	public static Collections.Admin? GetAdmin(string email, string password) =>
		MainDatabase!.GetCollection<Collections.Admin>(Collections.ADMINS)
						.Find(admin => email.Equals(admin.Email) &&
													password.Equals(admin.Password))
						.FirstOrDefault();

	public static Collections.Product? FindProduct(string productId) =>
		MainDatabase!.GetCollection<Collections.Product>(Collections.PRODUCTS)
						.Find(product => product.Id.ToString().Equals(productId))
						.FirstOrDefault();

	public static Collections.Manufacturer? GetManufacturer(ObjectId manufacturerId) =>
		MainDatabase!.GetCollection<Collections.Manufacturer>(Collections.MANUFACTURERS)
						.Find(manufacturer => manufacturer.Id.Equals(manufacturerId))
						.FirstOrDefault();


	public static void UpdateBuilding(Collections.Building newBuilding) =>
		MainDatabase!.GetCollection<Collections.Building>(Collections.BUILDINGS)
						.ReplaceOne(building =>
										building.Id.Equals(newBuilding.Id),
										newBuilding);

	public static List<Collections.Building> GetBuildings() =>
		GetCollection<Collections.Building>(Collections.BUILDINGS);

	public static Collections.Rack? FindRack(string rackId) =>
		MainDatabase!.GetCollection<Collections.Rack>(Collections.RACKS)
						.Find(rack => rack.Id.ToString() == rackId)
						.FirstOrDefault();

	public static void UpdateRack(Collections.Rack newRack) =>
		MainDatabase!.GetCollection<Collections.Rack>(Collections.RACKS)
						.ReplaceOne(rack =>
										rack.Id.Equals(newRack.Id),
										newRack);

	public static List<Collections.Rack> GetRacks() =>
		GetCollection<Collections.Rack>(Collections.RACKS);

	public static Collections.Rack CreateRack(string buildingId) {

		Collections.Rack rack = new() {

			BuildingId = new(buildingId),
			Position = new(),
			Size = new() {

				Length = 2,
				Width = 1

			},
			Shelves = 3,
			Spacing = 0.5f

		};

		MainDatabase!.GetCollection<Collections.Rack>(Collections.RACKS)
						.InsertOne(rack);

		return rack;

	}

	public static void DeleteRack(ObjectId rackId) =>
		MainDatabase!.GetCollection<Collections.Rack>(Collections.RACKS)
						.DeleteMany(rack =>
										rack.Id.Equals(rackId));

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

	public static void DeleteAuthToken(string userId) =>
		MainDatabase!.GetCollection<Collections.AuthToken>(Collections.AUTH_TOKENS)
						.DeleteMany(authToken =>
										authToken.UserId.ToString().Equals(userId));

	public static ObjectId? GetUserId(string authToken) =>
		MainDatabase!.GetCollection<Collections.AuthToken>(Collections.AUTH_TOKENS)
						.Find(token => authToken.Equals(token.Token))
						.FirstOrDefault()?
						.UserId;

	private static List<TCollection> GetCollection<TCollection>(string name)
		where TCollection : Collections.DatabaseObjectBase =>
		MainDatabase!.GetCollection<TCollection>(name)
						.AsQueryable()
						.ToList();

	private static string GenerateAuthToken() =>
		Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

	public static class Collections {

		public const string WORKERS = "workers";
		public const string ADMINS = "admins";
		public const string AUTH_TOKENS = "auth_tokens";
		public const string PRODUCTS = "products";
		public const string MANUFACTURERS = "manufacturers";
		public const string BUILDINGS = "buildings";
		public const string RACKS = "racks";

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

		public class Product : DatabaseObjectBase {

			public virtual ObjectId? ManufacturerId { get; set; }
			public virtual ProductSize? Size { get; set; }

			public class ProductSize {

				public virtual float Width { get; set; }
				public virtual float Length { get; set; }
				public virtual float Height { get; set; }

			}

		}

		public class Manufacturer : DatabaseObjectBase {

			public virtual string? Name { get; set; }

		}

		public class Building : DatabaseObjectBase {

			public virtual string? Name { get; set; }
			public virtual BuildingSize? Size { get; set; }

			public class BuildingSize {

				public virtual int Width { get; set; }
				public virtual int Length { get; set; }
				public virtual int Height { get; set; }

			}

		}

		public class Rack : DatabaseObjectBase {

			public virtual ObjectId? BuildingId { get; set; }
			public virtual RackPosition? Position { get; set; }
			public virtual RackSize? Size { get; set; }
			public virtual int Shelves { get; set; }
			public virtual float Spacing { get; set; }
			public virtual float Rotation { get; set; }

			public class RackPosition {

				public virtual int X { get; set; }
				public virtual int Z { get; set; }

			}

			public class RackSize {

				public virtual int Width { get; set; }
				public virtual int Length { get; set; }

			}

		}

		public abstract class DatabaseObjectBase {

			public virtual ObjectId Id { get; set; }

			public virtual string StringId => Id.ToString()!;

		}

	}

}