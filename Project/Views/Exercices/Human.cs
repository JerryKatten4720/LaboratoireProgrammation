namespace LaboratoireProgrammation.Project.Views.Exercices;

public class Human {
    public string Name { get; set; }
    public string Name2 { get; set; }
    public string Quality { get; set; }
    public float Salary { get; set; }
    public int Id { get; set; }

    public Human(string name, string name2, string quality, float salary) {
        Name = name;
        Name2 = name2;
        Quality = quality;
        Salary = salary;
        Id = Guid.NewGuid().GetHashCode();
    }

    public static String Serialize(Human human) {
        var json = new {
            Name = human.Name,
            Name2 = human.Name2,
            Quality = human.Quality,
            Salary = human.Salary,
            Id = human.Id
        };
        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);
        return jsonString;
    }

    public static Human? Deserialize(String json) {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Human>(json);
    }
}