using FamilyTree;
using Godot;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
public partial class Main : Node2D
{
	private AppDbContext _context = new AppDbContext(new DbContextOptions<AppDbContext>());
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
		optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=FamilyGuy;Username=postgres;Password=12345678");
		AppDbContext _context = new AppDbContext(optionsBuilder.Options);
	List<Person> test = _context.Person.ToList();
		//GD.Print(_context.Person.Where(a => a.FirstName == "Perszy").FirstOrDefault().Surname);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
