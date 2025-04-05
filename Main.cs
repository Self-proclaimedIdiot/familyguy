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
	
	private float zoomLevel = 1.0f;
	private const float zoomSpeed = 0.1f;
	private const float minZoom = 0.5f;
	private const float maxZoom = 2.0f;
	private Vector2 zoomCenter;

	// Переменные для перемещения
	private const float scrollSpeed = 20.0f;

	public void InitPerson(PersonModel model)
	{
		PackedScene scene = (PackedScene)GD.Load("res://Scenes/Person.tscn");
		Person person = (Person)scene.Instantiate();
		AddChild(person);
		person.model = model;
		Sprite2D sprite = person.GetChild<Sprite2D>(0);
		sprite.Texture = (Texture2D)GD.Load(model.IsMale ? "res://Images/male.png" : "res://Images/female.png");
	}

	public override void _Ready()
	{
		PersonModel Cain = people.Find(p => p.Id == "67ef00c0330b5bbd5b57a19d").First();
		Cain.SetSource(people);
		Person Obyortka = new Person { model = Cain };
		List<Person> ancestors = Obyortka.GetFamily(1, 1);
		GD.Print(Cain.FirstName);
		foreach (var person in ancestors)
		{
			GD.Print(person.model.FirstName + " " + person.Generation);
		}
		InitPerson(Cain);
	}

	public override void _Process(double delta)
	{
		// Обработка зума колесиком мыши только при зажатом Ctrl
		float zoomFactor = 0.0f;
		float scrollDirection = 0.0f;
		
		if (Input.IsActionJustReleased("zoom_in") && Input.IsKeyPressed(Key.Ctrl))
		{
			zoomFactor = zoomSpeed; 
		}
		else if (Input.IsActionJustReleased("zoom_out") && Input.IsKeyPressed(Key.Ctrl))
		{
			zoomFactor = -zoomSpeed; 
		}
		else if (Input.IsActionJustReleased("zoom_in"))
		{
			scrollDirection = scrollSpeed; // Перемещение вниз
		}
		else if (Input.IsActionJustReleased("zoom_out"))
		{
			scrollDirection = -scrollSpeed; // Перемещение вверх
		}

		if (zoomFactor != 0.0f)
		{
			// Получаем позицию мыши в мировых координатах до зума
			zoomCenter = GetGlobalMousePosition();
			
			// Применяем зум
			float oldZoom = zoomLevel;
			zoomLevel = Mathf.Clamp(zoomLevel + zoomFactor, minZoom, maxZoom);
			
			// Вычисляем смещение для центрирования на курсоре
			Vector2 offset = zoomCenter - Position;
			Position += offset - (offset * zoomLevel / oldZoom);
			
			// Применяем масштаб
			Scale = new Vector2(zoomLevel, zoomLevel);
		}
		else if (scrollDirection != 0.0f)
		{
			// Перемещаем сцену вверх или вниз
			Position += new Vector2(0, scrollDirection);
		}
	}
}
