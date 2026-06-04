using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace LaboratoireProgrammation.Project.ModernHospital.Services;

public static class FactureGenerator {
    public static string ExporterFactureHtml(Facture facture, PatientActif patient, HospitalInfo hospital, List<LigneFacture> lignes, string doctorName = "Docteur Inconnu") {
        var nomComplet = patient.Nom ?? "";
        var parts = nomComplet.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var nom = parts.Length > 0 ? parts[0] : "inconnu";
        var prenom = parts.Length > 1 ? parts[1] : "inconnu";

        var safeNom = SanitizeName(nom);
        var safePrenom = SanitizeName(prenom);
        
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var facturesDir = Path.Combine(baseDir, "Factures");
        var patientDir = Path.Combine(facturesDir, $"{safeNom}_{safePrenom}");

        if (!Directory.Exists(patientDir)) {
            Directory.CreateDirectory(patientDir);
        }

        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        var uuid = Guid.NewGuid().ToString("N").Substring(0, 4);
        var fileName = $"facture-{facture.IdFacture}-{safeNom}-{safePrenom}-{dateStr}-{uuid}.html";
        var filePath = Path.Combine(patientDir, fileName);

        var html = GenererHtml(facture, patient, hospital, lignes, nomComplet, doctorName);
        File.WriteAllText(filePath, html);
        return filePath;
    }

    private static string SanitizeName(string name) {
        if (string.IsNullOrWhiteSpace(name)) return "inconnu";
        name = name.ToLowerInvariant();
        name = Regex.Replace(name, @"[^\w]", "");
        return name;
    }

    private static string GenererHtml(Facture facture, PatientActif patient, HospitalInfo hospital, List<LigneFacture> lignes, string nomComplet, string doctorName) {
        var lignesHtml = string.Join("\n", lignes.Select(l => $@"
            <tr>
                <td>{l.TypePrestation}</td>
                <td>{l.Description}</td>
                <td style='text-align:center;'>{l.Quantite}</td>
                <td style='text-align:right;'>${l.PrixUnitaire:F2}</td>
                <td style='text-align:right;'>${l.SousTotal:F2}</td>
            </tr>
        "));

        return $@"<!DOCTYPE html>
<html lang='fr'>
<head>
    <meta charset='UTF-8'>
    <title>Facture {facture.IdFacture}</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7f6; color: #333; margin: 0; padding: 40px; }}
        .invoice-box {{ max-width: 800px; margin: auto; padding: 30px; border: 1px solid #eee; background: #fff; box-shadow: 0 0 10px rgba(0, 0, 0, 0.15); border-top: 5px solid #00d4aa; }}
        .header {{ display: flex; justify-content: space-between; align-items: flex-start; padding-bottom: 20px; border-bottom: 2px solid #eee; }}
        .hospital-info h1 {{ margin: 0; color: #00d4aa; font-size: 24px; text-transform: uppercase; }}
        .hospital-info p {{ margin: 5px 0; color: #777; }}
        .invoice-details {{ text-align: right; }}
        .invoice-details h2 {{ margin: 0; color: #333; }}
        .invoice-details p {{ margin: 5px 0; color: #555; }}
        .patient-info {{ margin-top: 30px; margin-bottom: 30px; }}
        .patient-info h3 {{ border-bottom: 1px solid #eee; padding-bottom: 5px; color: #555; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ padding: 12px; border-bottom: 1px solid #eee; text-align: left; }}
        th {{ background-color: #f9f9f9; color: #333; }}
        .total-row {{ font-weight: bold; font-size: 18px; }}
        .total-val {{ text-align: right; color: #ff4d6a; }}
        .footer {{ margin-top: 50px; text-align: center; color: #777; font-size: 12px; border-top: 1px solid #eee; padding-top: 20px; }}
    </style>
</head>
<body>
    <div class='invoice-box'>
        <div class='header'>
            <div class='hospital-info'>
                <h1>{hospital?.Nom ?? "Modern Hospital"}</h1>
                <p>123 Avenue de la Santé, 75000 Paris</p>
                <p>Directeur: {hospital?.DirecteurGeneral ?? "Inconnu"}</p>
                <p>Jour de Simulation: {hospital?.JourSimulation ?? facture.JourEmission}</p>
            </div>
            <div class='invoice-details'>
                <h2>FACTURE #{facture.IdFacture}</h2>
                <p>Date d'émission: {DateTime.Now:dd/MM/yyyy}</p>
                <p>Statut: <strong style='color:#00d4aa;'>{facture.Statut}</strong></p>
            </div>
        </div>

        <div class='patient-info'>
            <h3>Informations du Patient</h3>
            <p><strong>Nom:</strong> {nomComplet}</p>
            <p><strong>Classe:</strong> {patient.Classe}</p>
        </div>

        <table>
            <thead>
                <tr>
                    <th>Type</th>
                    <th>Description</th>
                    <th style='text-align:center;'>Qté</th>
                    <th style='text-align:right;'>Prix Unitaire</th>
                    <th style='text-align:right;'>Sous-Total</th>
                </tr>
            </thead>
            <tbody>
                {lignesHtml}
            </tbody>
            <tfoot>
                <tr class='total-row'>
                    <td colspan='4' style='text-align:right;'>Total à Payer :</td>
                    <td class='total-val'>${facture.MontantTotal:F2}</td>
                </tr>
            </tfoot>
        </table>

        <div class='footer'>
            <p>Merci de votre confiance. Ce document est généré automatiquement.</p>
            <div style='margin-top: 30px; text-align: right; padding-right: 20px;'>
                <p>Le médecin traitant,</p>
                <p style='font-family: ""Brush Script MT"", ""Segoe Print"", cursive, sans-serif; font-size: 26px; color: #1a1a1a; margin: 5px 0;'>{doctorName}</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }
}
