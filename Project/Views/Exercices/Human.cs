using Newtonsoft.Json;

namespace LaboratoireProgrammation.Project.Views.Exercices;

public class Human {
    public Human(string name, string name2, string quality, float salary) {
        Name = name;
        Name2 = name2;
        Quality = quality;
        Salary = salary;
        Id = Guid.NewGuid().GetHashCode();
    }

    public string Name { get; set; }
    public string Name2 { get; set; }
    public string Quality { get; set; }
    public float Salary { get; set; }
    public int Id { get; set; }

    public static string Serialize(Human human) {
        var json = new {
            human.Name,
            human.Name2,
            human.Quality,
            human.Salary,
            human.Id
        };
        var jsonString = JsonConvert.SerializeObject(json, Formatting.Indented);
        return jsonString;
    }

    public static Human? Deserialize(string json) {
        return JsonConvert.DeserializeObject<Human>(json);
    }
}