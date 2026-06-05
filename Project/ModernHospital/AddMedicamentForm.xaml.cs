using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace LaboratoireProgrammation.Project.ModernHospital;

public partial class AddMedicamentForm : Window {
    private readonly DatabaseManager _db;
    private readonly Medicament? _medicament;

    public ObservableCollection<MedicamentCible> Cibles { get; } = new();
    public ObservableCollection<MedicamentIncompatible> Incompatibles { get; } = new();

    public AddMedicamentForm(DatabaseManager db, Medicament? medicament = null) {
        InitializeComponent();
        _db = db;
        _medicament = medicament;

        CbUnite.ItemsSource = _db.GetUnitesList();
        
        var cases = _db.GetCasCliniques();
        CbMaladieCible.ItemsSource = cases;
        CbMaladieIncompatible.ItemsSource = cases;

        LvCibles.ItemsSource = Cibles;
        LvIncompatibles.ItemsSource = Incompatibles;

        if (cases.Count > 0) {
            CbMaladieCible.SelectedIndex = 0;
            CbMaladieIncompatible.SelectedIndex = 0;
        }

        if (_medicament != null) {
            Title = "Modifier le Médicament";
            TbNom.Text = _medicament.Nom;
            TbDci.Text = _medicament.DCI;
            TbForme.Text = _medicament.Forme;
            TbStockActuel.Text = _medicament.StockActuel.ToString();
            TbStockMinimum.Text = _medicament.StockMinimum.ToString();
            TbPrix.Text = _medicament.PrixUnitaire.ToString();
            CbUnite.SelectedValue = _medicament.IdUnite;
            TbTempsLivraison.Text = _medicament.TempsLivraisonBase.ToString();

            var loadedCibles = _medicament.GetCiblesList();
            foreach (var c in loadedCibles) {
                Cibles.Add(c);
            }

            var loadedIncompatibles = _medicament.GetIncompatiblesList();
            foreach (var i in loadedIncompatibles) {
                Incompatibles.Add(i);
            }
        } else if (CbUnite.Items.Count > 0) {
            CbUnite.SelectedIndex = 0;
        }
    }

    private void BtnAddCible_Click(object sender, RoutedEventArgs e) {
        if (CbMaladieCible.SelectedItem is not CasClinique selectedCase) {
            PopupFactory.ShowAlert(this, "Sélectionnez une maladie cible.");
            return;
        }

        if (Cibles.Any(c => c.IdCas == selectedCase.IdCas)) {
            PopupFactory.ShowAlert(this, "Cette maladie cible est déjà ajoutée.");
            return;
        }

        if (Incompatibles.Any(i => i.IdCas == selectedCase.IdCas)) {
            PopupFactory.ShowAlert(this, "Cette maladie est déjà déclarée incompatible.");
            return;
        }

        if (!float.TryParse(TbBonusRemission.Text, out var bonus) || bonus < 0) {
            PopupFactory.ShowAlert(this, "Bonus de rémission invalide.");
            return;
        }

        if (!int.TryParse(TbQuantitePrescription.Text, out var qty) || qty <= 0) {
            PopupFactory.ShowAlert(this, "Quantité à prescrire invalide.");
            return;
        }

        Cibles.Add(new MedicamentCible {
            IdCas = selectedCase.IdCas,
            MaladieNom = selectedCase.Maladie,
            Bonus = bonus,
            Quantite = qty
        });
    }

    private void BtnRemoveCible_Click(object sender, RoutedEventArgs e) {
        if (sender is Button btn && btn.Tag is MedicamentCible cible) {
            Cibles.Remove(cible);
        }
    }

    private void BtnAddIncompatible_Click(object sender, RoutedEventArgs e) {
        if (CbMaladieIncompatible.SelectedItem is not CasClinique selectedCase) {
            PopupFactory.ShowAlert(this, "Sélectionnez une maladie incompatible.");
            return;
        }

        if (Incompatibles.Any(i => i.IdCas == selectedCase.IdCas)) {
            PopupFactory.ShowAlert(this, "Cette maladie incompatible est déjà ajoutée.");
            return;
        }

        if (Cibles.Any(c => c.IdCas == selectedCase.IdCas)) {
            PopupFactory.ShowAlert(this, "Cette maladie est déjà déclarée comme cible.");
            return;
        }

        if (!float.TryParse(TbMalusRemission.Text, out var malus) || malus < 0) {
            PopupFactory.ShowAlert(this, "Malus de rémission invalide.");
            return;
        }

        Incompatibles.Add(new MedicamentIncompatible {
            IdCas = selectedCase.IdCas,
            MaladieNom = selectedCase.Maladie,
            Malus = malus
        });
    }

    private void BtnRemoveIncompatible_Click(object sender, RoutedEventArgs e) {
        if (sender is Button btn && btn.Tag is MedicamentIncompatible incompatible) {
            Incompatibles.Remove(incompatible);
        }
    }

    private void BtnAutoGenerate_Click(object sender, RoutedEventArgs e) {
        var cases = _db.GetCasCliniques();
        if (cases.Count == 0) return;

        var rng = new Random();
        var existingNames = _db.GetMedicaments().Select(m => m.Nom.ToLowerInvariant()).ToHashSet();

        string name;
        int attempts = 0;
        do {
            name = GenerateDrugName(rng);
            attempts++;
        } while ((existingNames.Contains(name.ToLowerInvariant()) || name.Length < 5 || name.Length > 14) && attempts < 50);

        Cibles.Clear();
        Incompatibles.Clear();

        int numTargets = rng.Next(1, Math.Min(5, cases.Count + 1));
        var shuffled = cases.OrderBy(_ => rng.Next()).ToList();
        var targets = shuffled.Take(numTargets).ToList();
        var remaining = shuffled.Skip(numTargets).ToList();

        int numAdverse = rng.Next(0, Math.Min(3, remaining.Count + 1));
        var adverseList = remaining.Take(numAdverse).ToList();

        foreach (var t in targets) {
            Cibles.Add(new MedicamentCible {
                IdCas = t.IdCas,
                MaladieNom = t.Maladie,
                Bonus = rng.Next(5, 31),
                Quantite = rng.Next(1, 4)
            });
        }
        foreach (var a in adverseList) {
            Incompatibles.Add(new MedicamentIncompatible {
                IdCas = a.IdCas,
                MaladieNom = a.Maladie,
                Malus = rng.Next(5, 21)
            });
        }

        decimal price = GenerateAutoPrice(numTargets, numAdverse, rng);

        string[] formes = { "Comprimé", "Gélule", "Solution IV", "Injectable", "Sirop", "Patch", "Suppositoire" };
        string forme = formes[rng.Next(formes.Length)];

        TbNom.Text = name;
        TbDci.Text = GenerateDrugName(rng).ToLowerInvariant();
        TbForme.Text = forme;
        TbStockActuel.Text = rng.Next(20, 201).ToString();
        TbStockMinimum.Text = rng.Next(5, 21).ToString();
        TbPrix.Text = price.ToString("F2");
        TbTempsLivraison.Text = (rng.Next(1, 9) * 30).ToString();

        if (CbUnite.Items.Count > 0)
            CbUnite.SelectedIndex = rng.Next(CbUnite.Items.Count);

        BtnSave_Click(this, new RoutedEventArgs());
    }

    private static decimal GenerateAutoPrice(int numTargets, int numAdverse, Random rng) {
        double ratio = numTargets / Math.Max(1.0, numTargets + numAdverse);
        double noise = (rng.NextDouble() - 0.5) * 4.0;
        double price = 5.0 + ratio * 45.0 + noise;
        return (decimal)Math.Round(Math.Max(5.0, Math.Min(50.0, price)), 2);
    }

    private static readonly string[] Prefixes = {
        "Al", "Car", "Dex", "Flo", "Max", "Tri", "Neo", "Cop", "Zan", "Vel",
        "Pro", "Omi", "Ben", "Cel", "Lev", "Met", "Nar", "Oxa", "Pam", "Quin",
        "Riv", "Sol", "Tam", "Uni", "Var", "Xen", "Ylo", "Zel", "Ara", "Bro"
    };

    private static readonly string[] Roots = {
        "va", "zo", "ti", "pro", "be", "xi", "me", "lo", "ra", "pi",
        "co", "da", "fe", "gi", "hu", "ja", "ki", "lu", "mo", "nu",
        "pa", "ro", "su", "tu", "vi", "wo", "xe", "yo", "ze", "bi"
    };

    private static readonly string[] Suffixes = {
        "ax", "or", "ol", "am", "en", "ex", "ide", "ium", "is", "in",
        "on", "os", "yl", "ase", "ate", "one", "ine", "ane", "ile", "ene"
    };

    private static readonly string[] Modifiers = {
        " Chrono", " LP", " Fort", " Flash", " Dual", " Plus", " SR"
    };

    private static readonly HashSet<char> Vowels = new() { 'a', 'e', 'i', 'o', 'u', 'y' };

    private static string GenerateDrugName(Random rng) {
        int template = rng.Next(3);
        string prefix = Prefixes[rng.Next(Prefixes.Length)];
        string root = Roots[rng.Next(Roots.Length)];
        string suffix = Suffixes[rng.Next(Suffixes.Length)];

        string core;
        if (template == 0) {
            core = Combine(prefix, suffix);
        } else if (template == 1) {
            core = Combine(Combine(prefix, root), suffix);
        } else {
            string modifier = Modifiers[rng.Next(Modifiers.Length)];
            core = Combine(Combine(prefix, root), suffix) + modifier;
        }

        return char.ToUpper(core[0]) + core[1..].ToLowerInvariant();
    }

    private static string Combine(string a, string b) {
        if (a.Length == 0 || b.Length == 0) return a + b;

        char lastA = char.ToLower(a[^1]);
        char firstB = char.ToLower(b[0]);

        if (Vowels.Contains(lastA) && Vowels.Contains(firstB))
            return a[..^1] + b;

        bool tripleCons = !Vowels.Contains(lastA)
            && a.Length >= 2 && !Vowels.Contains(char.ToLower(a[^2]))
            && !Vowels.Contains(firstB);

        if (tripleCons)
            return a + "o" + b;

        return a + b;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(TbNom.Text)) {
            PopupFactory.ShowAlert(this, "Le nom est obligatoire.");
            return;
        }

        if (!int.TryParse(TbStockActuel.Text, out var sa) || sa < 0) {
            PopupFactory.ShowAlert(this, "Stock actuel invalide.");
            return;
        }

        if (!int.TryParse(TbStockMinimum.Text, out var sm) || sm < 0) {
            PopupFactory.ShowAlert(this, "Stock minimum invalide.");
            return;
        }

        if (!decimal.TryParse(TbPrix.Text, out var prix) || prix < 0) {
            PopupFactory.ShowAlert(this, "Prix unitaire invalide.");
            return;
        }

        if (CbUnite.SelectedValue is not int idUnite) {
            PopupFactory.ShowAlert(this, "Veuillez sélectionner une unité.");
            return;
        }

        if (!int.TryParse(TbTempsLivraison.Text, out var tl) || tl <= 0) {
            PopupFactory.ShowAlert(this, "Temps de livraison moyen invalide.");
            return;
        }

        string ciblesJson = JsonConvert.SerializeObject(Cibles);
        string incompatiblesJson = JsonConvert.SerializeObject(Incompatibles);

        try {
            if (_medicament == null) {
                _db.AddMedicament(TbNom.Text.Trim(), TbDci.Text.Trim(), TbForme.Text.Trim(), sa, sm, prix, idUnite, ciblesJson, incompatiblesJson, tl);
            } else {
                _db.UpdateMedicament(_medicament.IdMedicament, TbNom.Text.Trim(), TbDci.Text.Trim(), TbForme.Text.Trim(), sa, sm, prix, idUnite, ciblesJson, incompatiblesJson, tl);
            }
            DialogResult = true;
            Close();
        } catch (Exception ex) {
            PopupFactory.ShowAlert(this, "Erreur: " + ex.Message);
        }
    }
}
