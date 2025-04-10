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
		public List<bool> GenderOffsets = new List<bool>();
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
				father.GenderOffsets.AddRange(GenderOffsets);
				father.GenderOffsets.Add(true);
				ancestors.Add(father);
			}
			if (mother != null)
			{
				mother.Generation = Generation + 1;
				father.GenderOffsets.AddRange(GenderOffsets);
				mother.GenderOffsets.Add(false);
				ancestors.Add(mother);
			}
			if (generation > 1)
			{
				if(father != null)
				ancestors.AddRange(father.GetAncestors(generation - 1));
				if(mother != null)
				ancestors.AddRange(mother.GetAncestors(generation - 1));
			}
			//ClearDuplicates(ancestors);
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
		public List <Person> GetSpouses() 
		{
			return model.Spouses;
		}
		public Person GetLastSpouse()
		{
			return model.Spouse;
		}
		//метод - нейронная дристня, но пока оставлю
		public List<Person> GetFamily(int ancestorsDepth, int descendantsDepth)
		{
			List<Person> family = new List<Person>();
	
			// Добавляем самого человека
			this.Generation = 0;
			family.Add(this);
	
			// Получаем предков
			if (ancestorsDepth > 0)
			{
				var ancestors = GetAncestors(ancestorsDepth);
				family.AddRange(ancestors);
			}
	
			// Получаем потомков
			if (descendantsDepth > 0)
			{
				var descendants = GetDescendants(descendantsDepth);
				family.AddRange(descendants);
			}
	
			ClearDuplicates(family);
			return family;
		}
		public List<Person> GetFamilyNormal(int generation, int siblings_generation)
		{
			List<Person> ancestors = GetAncestors(generation);
			List<Person> family = new List<Person>();
			family.AddRange(ancestors);
			List<Person> parents = GetOneGenerationAncestors(siblings_generation);
			List<Person> children = new List<Person>();
			bool has_children = true;
			int current_generation = siblings_generation - 1;
			while (has_children)
			{
				foreach (Person p in parents)
				{
					List<Person> ones_children = p.GetChildren();
					foreach (Person p2 in ones_children)
					{
						p2.Generation = current_generation;
					}
					children.AddRange(ones_children);
				}
				if (children.Count == 0)
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

		private List<Person> GetDescendants(int depth)
		{
			List<Person> descendants = new List<Person>();
	
			if (depth <= 0) return descendants;
	
			foreach (var child in model.Children)
			{
				child.Generation = Generation - 1; // Первое поколение потомков
				descendants.Add(child);
				if (depth > 1)
				{
					var grandChildren = child.GetDescendants(depth - 1);
					descendants.AddRange(grandChildren);
				}
			}
			return descendants;
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
