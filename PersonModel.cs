using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Godot;
using System.Reflection.Metadata.Ecma335;
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
		public bool HasSource() => _source != null;
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
		public List<Person> Spouses { get
			{
				List <PersonModel> models = new List<PersonModel>();
				List<Person> Spouses = new List<Person>();
				List<PersonModel> children = _source.Find(p => IsMale ? p.FatherId == Id : p.MotherId == Id).ToList();
				foreach (var child in children)
				{
					if (models.Find(p => p.Id == (IsMale?child.MotherId:child.FatherId)) == null)
					{
						PersonModel spouse = _source.Find(p => p.Id == (IsMale ? child.MotherId : child.FatherId)).FirstOrDefaultAsync().Result;
						if (spouse != null)
						models.Add(spouse);
					}
				}
				foreach(var model in models)
				{
					model.SetSource(_source);
					Spouses.Add(new Person { model = model});
				}
				return Spouses;
			} }
		public Person Spouse
		{
			get
			{
				Person Spouse = null;
				DateOnly maxdate = new DateOnly(1, 1, 1);
				foreach(var spouse in Spouses)
				{
					foreach(var child in spouse.GetChildren())
					{
						if(child.model.BirthDate > maxdate)
						{
							Spouse = spouse;
							maxdate = child.model.BirthDate;
						}
					}
				}
				return Spouse;
			}
		}
	}
}
