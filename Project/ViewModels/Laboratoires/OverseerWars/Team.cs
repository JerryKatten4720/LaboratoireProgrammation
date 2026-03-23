using LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars.DwellerComponent;

namespace LaboratoireProgrammation.Project.ViewModels.Laboratoires.OverseerWars;

public class Team {
    
    public static List<Team> Teams = new List<Team>();
    
    public int TeamIndex;
    public List<Dweller> Dwellers;
    public Dweller Overseer;

    public double Energy { get; set; } public double EnergyMax { get; set; }
    public double Food { get; set; } public double FoodMax { get; set; }
    public double Electricity { get; set; } public double ElectricityMax { get; set; }
    public double Wood { get; set; } public double WoodMax { get; set; }
    public double Metals { get; set; } public double MetalsMax { get; set; }
    public double Caps { get; set; }

    public String Name;

    public int ActionPoints = 10;
    
    public Team(int teamIndex, String name) {
        TeamIndex = teamIndex;
        Name = name;
    }

    public void AddDweller(Dweller dweller) {
        dweller.Team = this;
        if (Dwellers == null) { Dwellers = new List<Dweller>(); }
        Dwellers.Add(dweller);
    }
    public void RemoveDweller(Dweller dweller) { Dwellers.Remove(dweller); }
    
    public void Initialize() {
        EnergyMax = 200;
        FoodMax = 200;
        ElectricityMax = 200;
        WoodMax = 200;
        MetalsMax = 200;
        Caps = 200;
        
        Energy = 150;
        Food = 150;
        Electricity = 150;
        Wood = 150;
        Metals = 150;
    }
    
    public void UpdateResources() {
        double surplus;
        if (Energy > EnergyMax) {
            surplus = Energy - EnergyMax; Energy = EnergyMax;
            Caps += surplus / 10;
        }
        
        if (Food > FoodMax) {
            surplus = Food - FoodMax; Food = FoodMax;
            Caps += surplus / 10;
        }
        
        if (Electricity > ElectricityMax) {
            surplus = Electricity - ElectricityMax; Electricity = ElectricityMax;
            Caps += surplus / 10;
        }
        
        if (Wood > WoodMax) {
            surplus = Wood - WoodMax; Wood = WoodMax;
            Caps += surplus / 10;
        }
        
        if (Metals > MetalsMax) {
            surplus = Metals - MetalsMax; Metals = MetalsMax;
            Caps += surplus / 10;
        }
    }
    
    public static Team GetTeam(int teamIndex) { return Teams[teamIndex]; }
    
    public void NewTurn() {
        ActionPoints = 10;
    }

    
}