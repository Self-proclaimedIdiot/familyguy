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
using System.Security.Cryptography.X509Certificates;

namespace FamilyTree
{
	public partial class Person : Node2D
	{
		public PersonModel model;
		public int Generation {  get; set; }
		public void ClearDuplicates(List<Person> list)
		{
			Dictionary<Person, int> repeats = new Dictionary<Person, int>();
			foreach (Person p in list)
			{
				bool contains = false;
				foreach (var id in repeats.Keys)
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
				list.RemoveAll(p => p.model.Id == id.model.Id);
				list.Add(id);
			}
		}
		public List<Person> GetAncestors(int generation)
		{
			List<Person> ancestors = new List<Person>();
			Person father = model.Father;
			Person mother = model.Mother;
			if (father != null)
			{
				father.Generation = Generation + 1;
				ancestors.Add(father);
			}
			if (mother != null)
			{
				mother .Generation = Generation + 1;
				ancestors.Add(mother);
			}
			if (generation > 1)
			{
				if(father != null)
				ancestors.AddRange(father.GetAncestors(generation - 1));
				if(mother != null)
				ancestors.AddRange(mother.GetAncestors(generation - 1));
			}
			ClearDuplicates(ancestors);
			return ancestors;
		}
		public List<Person> GetOneGenerationAncestors(int generation)
		{
			List<Person> ancestors = new List<Person>();
			if(generation == 1)
			{
				if (model.Father != null)
					ancestors.Add(model.Father);
				if(model.Mother != null)
					ancestors.Add(model.Mother);
			}
			if (generation > 1)
			{
				if (model.Father != null)
					ancestors.AddRange(model.Father.GetOneGenerationAncestors(generation - 1));
				if (model.Mother != null)
					ancestors.AddRange(model.Mother.GetOneGenerationAncestors(generation - 1));
			}
			ClearDuplicates(ancestors);
			return ancestors;
		}
		public List<Person> GetChildren()
		{
			return model.Children;
		}
		//todo
		public List<Person> GetFamily(int generation, int siblings_generation)
		{
			List<Person> ancestors = GetAncestors(generation);
			List<Person> family = new List<Person>();
			family.AddRange(ancestors);
			List<Person> parents = GetOneGenerationAncestors(siblings_generation);
			List<Person> children = new List<Person>();
			bool has_children = true;
			int current_generation = siblings_generation - 1;
			while(has_children)
			{
				foreach(Person p in parents)
				{
					List<Person> ones_children = p.GetChildren();
					foreach(Person p2 in ones_children)
					{
						p2.Generation = current_generation;
					}
					children.AddRange(ones_children);
				}
				if(children.Count == 0)
					has_children = false;
				else
				{
					family.AddRange(children);
					parents = new List<Person>();
					parents.AddRange(children);
					children = new List<Person>();
					current_generation--;
				}
			}
			ClearDuplicates(family);
			return family;
		}
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{

		}
	}
}
