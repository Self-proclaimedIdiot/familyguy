using Godot;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace FamilyTree
{
	public partial class Person : Node2D
	{
		public PersonModel model;
		public List<Person> GetAncestors(int generation)
		{
			List<Person> ancestors = new List<Person>();
			if (model.Father != null)
				ancestors.Add(model.Father);
			if (model.Mother != null)
				ancestors.Add(model.Mother);
			if (generation > 1)
			{
				if(model.Father != null)
				ancestors.AddRange(model.Father.GetAncestors(generation - 1));
				if(model.Mother != null)
				ancestors.AddRange(model.Mother.GetAncestors(generation - 1));
			}
			Dictionary<Person, int> repeats = new Dictionary<Person, int>();
			foreach(Person p in ancestors)
			{
				bool contains = false;
				foreach(var id in repeats.Keys)
				{
					if (id.model.Id == p.model.Id)
					{
						repeats[id]++;
						contains = true;
					}
				}
				if (!contains)
					repeats.Add(p, 1);
			}
			foreach (var id in repeats.Keys)
			{
				for (int i = 0; i < repeats[id] - 1; i++)
				{
					ancestors.Remove(id);
				}
			}
			return ancestors;
		}
	}
}
