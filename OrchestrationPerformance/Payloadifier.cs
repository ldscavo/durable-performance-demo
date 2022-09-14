using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchestrationPerformance
{
  public class Payloadifier
  {
    public static List<Payload> BOINK(int length) => Enumerable.Range(0, length).Select(_ => Boink()).ToList();

    public static Payload Boink() =>
      new()
      {
        Id = Guid.NewGuid(),
        Name = GenString(min: 25, max: 45),
        Color = GenColor(),
        FavoriteAnimals = GenAnimalList(length: new Random().Next(1, 15))
      };

    private static string GenString(int min, int max)
    {
      var random = new Random();

      var chars =
        Enumerable.Range('A', 'Z').Concat(Enumerable.Range('a', 'z'))
          .Select(c => (char)c).ToList();
      
      return new string(Enumerable.Repeat(chars, random.Next(min, max)).Select(s => s[random.Next(s.Count)]).ToArray());
    }

    private static Color GenColor() => (Color)new Random().Next(0, 5);

    private static Animal GenAnimal() => (Animal)new Random().Next(0, 6);

    private static List<Animal> GenAnimalList(int length) => Enumerable.Range(0, length).Select(n => GenAnimal()).ToList();
  }

  public class Payload
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Color Color { get; set; }
    public List<Animal> FavoriteAnimals { get; set; }
  }

  public enum Color
  {
    Green,
    PaleGreen,
    BoldGreen,
    VibrantGreen,
    Brown
  }

  public enum Animal
  {
    Gazelle,
    Turtle,
    Salmon,
    GrossCow,
    Aardvark,
    VeryLargeSwanWithAnEyePatchAndPirateHat
  }
}
