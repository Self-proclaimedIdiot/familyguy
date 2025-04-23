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

	// В классе Main добавьте эти константы
	private const float GENERATION_OFFSET = 300f; // Расстояние между поколениями
	private const float SIBLING_OFFSET = 100f;   // Расстояние между братьями/сестрами
	private const float GENDER_OFFSET = 100f;     // Смещение по полу для поколения 0

	public Person InitPerson(PersonModel model)
	{
		PackedScene scene = (PackedScene)GD.Load("res://Scenes/Person.tscn");
		Person person = (Person)scene.Instantiate();
		person.model = model;
		AddChild(person);
		Sprite2D sprite = person.GetChild<Sprite2D>(0);
		sprite.Texture = (Texture2D)GD.Load(person.model.IsMale ? "res://Images/male.png" : "res://Images/female.png");
		//labele.Texture = (Texture2D)GD.Load("res://Images/label.png");
		Label name = person.GetChild<Node2D>(3).GetChild<Label>(0);
		name.Text = model.FirstName + " " + model.Surname + (model.HasSource()?"":"(source not loaded)");
		// Позиционируем персонажа
		//PositionPerson(guy);
		return person;
	}

	public void SetAllInPlace(List<Person> family)
	{
		int maxgen = family[0].Generation;
		for(int i = 1;i < family.Count;i++)
		{
			if (family[i].Generation > maxgen)
				maxgen = family[i].Generation;
		}
		List<Person> elders = family.FindAll(p => p.Generation == maxgen);

	}
	public Person RayCastCheckForPerson()
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		var parameters = new PhysicsPointQueryParameters2D
		{
			Position = GetGlobalMousePosition(),
			CollideWithAreas = true,
			CollisionMask = 1,
		};
		var points = spaceState.IntersectPoint(parameters);
		if(points.Count > 0)
		{
			return points[0]["collider"].As<Area2D>().GetParent<Person>();
		}
		else return null;
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left)
		{
			if (@event.IsPressed())
			{
				Person person = RayCastCheckForPerson();
				if(person != null)
				{
					List<Person> siblings = person.GetSiblings();
					if (siblings.Count > 0)
					{
						if(person.model.IsMale)
						siblings.Add(person);
						siblings.Sort((p1,p2) => p1.model.BirthDate >  p2.model.BirthDate?1:-1);
						if (!person.model.IsMale)
						siblings.Add(person);
						float center = (person.model.Father != null && person.model.Mother != null ?
							(person.GetFatherOnScene().Position.X + person.GetMotherOnScene().Position.X) / 2 : person.Position.X);
						int number = 0;
						foreach(Person p in siblings)
						{
							Person current = GetChildren().OfType<Person>().ToList().Find(P => p.model.Id == P.model.Id);
							if (current == null)
							current = InitPerson(p.model);
							current.Position = new Vector2(center + 200 * (siblings.Count % 2 == 0?number - siblings.Count/2 + (number - siblings.Count/2 >= 0?1:0):number - siblings.Count/2), person.Position.Y);
							number++;
						}
						QueueRedraw();
					}
				}
			}
		}
	}
	public override void _Ready()
	{
		PersonModel Cain = people.Find(p => p.Id == "67f9814bf26ba744d29bf701").First();
		Cain.SetSource(people);
		Person body = InitPerson(Cain);
		body.Position = new Vector2(1200, 1000);
		body.GetAncestors(4);
		/*List<Person> family = new Person {model = Cain}.GetFamilyNormal(1,1);
		foreach(Person person in family)
			{
				GD.Print(person.model.FirstName);
			}
		*.
		// Очищаем предыдущие элементы (если есть)
		/*foreach (Node child in GetChildren())
		{
			if (child is Person)
			{
				child.QueueFree();
			}
		}

		// Создаем основного персонажа (поколение 0)
		InitPerson(new Person { model = Cain });
	
		// Получаем всех родственников
		List<Person> family = new Person { model = Cain }.GetFamily(2, 2); // 2 поколения вверх и 2 вниз
	
		// Отображаем всех родственников с правильным позиционированием
		foreach (var relative in family)
		{
			InitPerson(relative);
		}
	
		// Обновляем линии соединения
		QueueRedraw();
		*/
	}


	private void PositionPerson(Person person)
	{
		// Центральная позиция Cain (поколение 0)
		Vector2 center = new Vector2(400, 300); // Центр экрана
	
		// Вертикальная позиция (ось Y)
		float yPos = center.Y - person.Generation * GENERATION_OFFSET;
	
		// Горизонтальная позиция (ось X)
		float xPos = center.X;
	
		if (person.Generation == 0)
		{
			// Для поколения 0: мужчины справа, женщины слева
			xPos += person.model.IsMale ? GENDER_OFFSET : -GENDER_OFFSET;
		}
		else if (person.Generation > 0) // Предки
		{
			// Распределяем предков по горизонтали
			if (person.model.Father != null && person.model.Father.model.Id == person.model.Id)
				xPos -= SIBLING_OFFSET;
			else if (person.model.Mother != null && person.model.Mother.model.Id == person.model.Id)
				xPos += SIBLING_OFFSET;
		}
		else // Потомки
		{
			// Распределяем потомков по горизонтали
			xPos += SIBLING_OFFSET * (person.Generation * -1);
		}
	
		person.Position = new Vector2(xPos, yPos);
	}

	public override void _Draw()
	{
		base._Draw();
	
		// Получаем всех персонажей на сцене
		var persons = GetChildren().OfType<Person>().ToList();
	
		foreach (var person in persons)
		{
			// Рисуем линии к родителям
			var father = person.GetFatherOnScene();
			/*if (father != null)
			{
				DrawLine(person.Position, father.Position, Colors.Blue, 2);
			}*/
			var mother = person.GetMotherOnScene();
			/*if (mother != null)
			{
				DrawLine(person.Position, mother.Position, Colors.Red, 2);
			}*/
			var spouse = person.GetSpouseOnScene();
			if (spouse != null)
			{
				DrawLine(person.Position, spouse.Position, Colors.Green, 2);
			}
			if (mother != null && father != null)
			{
				DrawLine(person.Position, new Vector2((father.Position.X + mother.Position.X) / 2, father.Position.Y), Colors.Green, 2);
			}
			else if (mother != null || father != null)
			{
				DrawLine(person.Position, father != null ? father.Position : mother.Position, Colors.Green, 2);
			}
		}
	}
	
	private void DrawFamilyLines()
	{
		// Получаем всех персонажей на сцене
		var persons = GetChildren().OfType<Person>();

		foreach (var person in persons)
		{
			if (person.model.Father != null)
			{
				var father = persons.FirstOrDefault(p => p.model.Id == person.model.Father.model.Id);
				if (father != null)
				{
					DrawLine(person.Position, father.Position, Colors.Blue, 2);
				}
			}

			if (person.model.Mother != null)
			{
				var mother = persons.FirstOrDefault(p => p.model.Id == person.model.Mother.model.Id);
				if (mother != null)
				{
					DrawLine(person.Position, mother.Position, Colors.Red, 2);
				}
			}
		}
	}


	public override void _Process(double delta)
	{
		// Обработка зума колесиком мыши только при зажатом Ctrl
		float zoomFactor = 0.0f;
		float scrollDirectionY = 0.0f;
		float scrollDirectionX = 0.0f;

		if (Input.IsActionJustReleased("zoom_in") && Input.IsKeyPressed(Key.Ctrl))
		{
			zoomFactor = zoomSpeed;
		}
		else if (Input.IsActionJustReleased("zoom_out") && Input.IsKeyPressed(Key.Ctrl))
		{
			zoomFactor = -zoomSpeed;
		}
		else if (Input.IsActionJustReleased("zoom_in") && Input.IsKeyPressed(Key.Alt))
		{
			scrollDirectionX = -scrollSpeed; // Перемещение вправо (при увеличении)
		}
		else if (Input.IsActionJustReleased("zoom_out") && Input.IsKeyPressed(Key.Alt))
		{
			scrollDirectionX = scrollSpeed; // Перемещение влево (при уменьшении)
		}
		else if (Input.IsActionJustReleased("zoom_in"))
		{
			scrollDirectionY = scrollSpeed; // Перемещение вниз
		}
		else if (Input.IsActionJustReleased("zoom_out"))
		{
			scrollDirectionY = -scrollSpeed; // Перемещение вверх
		}


		if (zoomFactor != 0.0f)
		{
			// Получаем позицию мыши в мировых координатах до зума
			zoomCenter = GetGlobalMousePosition();

			// Применяем зум (БЕЗ ОГРАНИЧЕНИЙ)
			float oldZoom = zoomLevel;
			zoomLevel += zoomFactor; // Убрали Mathf.Clamp()

			// Защита от нулевого или отрицательного зума (чтобы не было ошибок)
			if (zoomLevel <= 0.01f) zoomLevel = 0.01f; // Минимальное значение, близкое к нулю

			// Вычисляем смещение для центрирования на курсоре
			Vector2 offset = zoomCenter - Position;
			Position += offset - (offset * zoomLevel / oldZoom);

			// Применяем масштаб
			Scale = new Vector2(zoomLevel, zoomLevel);
		}
		else if (scrollDirectionY != 0.0f)
		{
			// Перемещаем сцену вверх или вниз
			Position += new Vector2(0, scrollDirectionY);
		}
		else if (scrollDirectionX != 0.0f)
		{
			// Перемещаем сцену влево или вправо
			Position += new Vector2(scrollDirectionX, 0);
		}
	}
}
