using FamilyTree;
using Godot;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
public partial class Main : Node2D
{
	static MongoClient client = new MongoClient("mongodb://localhost:27017/");
	static IMongoDatabase db = client.GetDatabase("FamilyGuy");
	static IMongoCollection<PersonModel> people = db.GetCollection<PersonModel>("Person");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PersonModel Cain = people.Find(p => p.Id == "67ef08f6613b520123b54646").First();
		Cain.SetSource(people);
		Person Obyortka = new Person {model = Cain};
		GD.Print(Cain.FirstName);
		List<Person> ancestors = Obyortka.GetAncestors(3);
		GD.Print(ancestors.Count);
		foreach(var p in ancestors)
		{
			GD.Print(p.model.FirstName);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
