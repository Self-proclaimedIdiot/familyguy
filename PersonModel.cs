using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Godot;
namespace FamilyTree
{
	public class PersonModel
	{
		private IMongoCollection<PersonModel> _source;
		public PersonModel(IMongoCollection<PersonModel> source)
		{
			_source = source;
		}
		public void SetSource(IMongoCollection<PersonModel> source)
		{
			_source = source;
		}
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public bool IsMale { get; set; }
		public string FirstName { get; set; }
		public string Surname { get; set; }
		public string? Patronim { get; set; }
		public DateOnly BirthDate { get; set; }
		public DateOnly? DeathDate { get; set; }
		public Person Father
		{
			get
			{
				var father = _source.Find(p => p.Id == FatherId).FirstOrDefaultAsync().Result;
				if (father != null)
				{
					father.SetSource(_source);
					var Father = new Person { model = father };
					return Father;
				}
				else return null;
			}
		}
		[BsonElement("fatherId")]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? FatherId { get; set; }
		public Person Mother {
			get
			{
				var mother = _source.Find(p => p.Id == MotherId).FirstOrDefaultAsync().Result;
				if (mother != null)
				{
					mother.SetSource(_source);
					var Mother = new Person { model = mother };
					return Mother;
				}
				else return null;
			}
		}
		[BsonElement("motherId")]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? MotherId { get; set; }
		public List<Person> Children { get
			{
				List<PersonModel> models =  _source.Find(p => IsMale?p.FatherId == Id:p.MotherId == Id).ToList();
				List<Person> Children = new List<Person>();
				foreach (var model in models)
				{
					model.SetSource(_source);
					Children.Add(new Person { model = model });
				}
				return Children;
			} }
	}
}
