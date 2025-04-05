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
	public void InitPerson(PersonModel model)
	{
		PackedScene scene = (PackedScene)GD.Load("res://Scenes/Person.tscn");
		Person person = (Person)scene.Instantiate();
		AddChild(person);
		person.model = model;
		Sprite2D sprite = person.GetChild<Sprite2D>(0);
		sprite.Texture = (Texture2D)GD.Load(model.IsMale ? "res://Images/male.png" : "res://Images/female.png");
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PersonModel Cain = people.Find(p => p.Id == "67ef00c0330b5bbd5b57a19d").First();
		Cain.SetSource(people);
		Person Obyortka = new Person {model = Cain};
		List<Person> ancestors = Obyortka.GetFamily(1,1);
		GD.Print(Cain.FirstName);
		foreach(var person in ancestors)
		{
			GD.Print(person.model.FirstName + " " + person.Generation);
		}
		InitPerson(Cain);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
