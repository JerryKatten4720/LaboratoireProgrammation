using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace LaboratoireProgrammation.Project.ModernHospital.Services;

public static class FactureGenerator {
    private static readonly string[] SignatureFonts = {
        "Alex Brush",
        "Dancing Script",
        "Great Vibes",
        "Shadows Into Light",
        "Sacramento",
        "Parisienne",
        "Allura"
    };

    public static string ExporterFactureHtml(
        Facture facture, 
        PatientActif patient, 
        HospitalInfo hospital, 
        List<LigneFacture> lignes, 
        string doctorName = "Docteur Inconnu", 
        string accentColor = "#0ea5e9") {
        
        var nomComplet = patient.Nom ?? string.Empty;
        var directoryPath = GetPatientDirectory(nomComplet);
        var filePath = GenerateFilePath(directoryPath, facture, nomComplet);
        var htmlContent = GenererHtml(facture, patient, hospital, lignes, nomComplet, doctorName, accentColor);
        
        File.WriteAllText(filePath, htmlContent);
        
        return filePath;
    }

    public static string ExporterFactureHopitalHtml(
        FactureDisplayItem item, 
        HospitalInfo hospital, 
        List<LigneFacture> lignesDetails = null) {
        
        var directoryPath = GetHopitalDirectory();
        var filePath = GenerateHopitalFilePath(directoryPath, item);
        var htmlContent = GenererHopitalHtml(item, hospital, lignesDetails);

        File.WriteAllText(filePath, htmlContent);
        
        return filePath;
    }

    public static List<LigneFacture> GenererLignesCreationChambre(decimal coutEntretien, int capaciteLits) {
        var coutBase = 15000m + coutEntretien;
        var prixParLit = 0.20m * coutEntretien;

        return new List<LigneFacture> {
            new LigneFacture {
                TypePrestation = "Coût de base de création",
                Description = "Frais fixes d'installation (15 000 $) et maintenance de base de l'unité",
                Quantite = 1,
                PrixUnitaire = coutBase,
                SousTotal = coutBase
            },
            new LigneFacture {
                TypePrestation = "Supplément capacité lits",
                Description = $"Majoration de 20% du coût d'entretien par lit installé (+{capaciteLits} lits)",
                Quantite = capaciteLits,
                PrixUnitaire = prixParLit,
                SousTotal = prixParLit * capaciteLits
            }
        };
    }

    private static string GetPatientDirectory(string nomComplet) {
        var parts = nomComplet.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var nom = SanitizeName(parts.Length > 0 ? parts[0] : "inconnu");
        var prenom = SanitizeName(parts.Length > 1 ? parts[1] : "inconnu");
        
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var patientDir = Path.Combine(baseDir, "Factures", $"{nom}_{prenom}");

        if (!Directory.Exists(patientDir)) {
            Directory.CreateDirectory(patientDir);
        }

        return patientDir;
    }

    private static string GetHopitalDirectory() {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var hopitalDir = Path.Combine(baseDir, "Factures", "hopital");
        
        if (!Directory.Exists(hopitalDir)) {
            Directory.CreateDirectory(hopitalDir);
        }

        return hopitalDir;
    }

    private static string GenerateFilePath(string directoryPath, Facture facture, string nomComplet) {
        var parts = nomComplet.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var nom = SanitizeName(parts.Length > 0 ? parts[0] : "inconnu");
        var prenom = SanitizeName(parts.Length > 1 ? parts[1] : "inconnu");
        
        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        var billCode = GetBillCode(facture);
        
        return Path.Combine(directoryPath, $"facture-{billCode}-{nom}-{prenom}-{dateStr}.html");
    }

    private static string GenerateHopitalFilePath(string directoryPath, FactureDisplayItem item) {
        return Path.Combine(directoryPath, $"facture-hopital-{item.CodeFacture}.html");
    }

    private static string GetBillCode(Facture facture) {
        return string.IsNullOrEmpty(facture.CodeFacture) ? facture.IdFacture.ToString() : facture.CodeFacture;
    }

    private static string SanitizeName(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            return "inconnu";
        }
        
        return Regex.Replace(name.ToLowerInvariant(), @"[^\w]", "");
    }

    private static string GenererHtml(
        Facture facture, 
        PatientActif patient, 
        HospitalInfo hospital, 
        List<LigneFacture> lignes, 
        string nomComplet, 
        string doctorName, 
        string accentColor) {
        
        var sb = new StringBuilder();
        var billCode = GetBillCode(facture);
        var random = new Random(billCode.GetHashCode());
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='fr'>");
        sb.AppendLine(GenerateHtmlHead(billCode, accentColor));
        sb.AppendLine("<body>");
        
        sb.AppendLine(GeneratePrintButton());
        
        sb.AppendLine("<div class='invoice-wrapper'>");
        sb.AppendLine(GenerateInvoiceHeader(billCode, facture.JourEmission, facture.Statut, hospital));
        sb.AppendLine(GeneratePatientSection(patient, nomComplet));

        var stayLine = lignes.FirstOrDefault(l => l.TypePrestation == "Chambre" && l.Description == "Frais de séjour");
        int stayDays = stayLine?.Quantite ?? 1;

        var treatmentLine = lignes.FirstOrDefault(l => l.TypePrestation == "Chambre" && l.Description == "Supplément de frais de séjour");
        int treatmentHours = treatmentLine?.Quantite ?? 0;

        sb.AppendLine(GenerateHospitalisationSection(stayDays, treatmentHours));
        sb.AppendLine(GenerateInvoiceTable(lignes, facture));
        sb.AppendLine(GenerateFooter(doctorName, random));
        sb.AppendLine("</div>");
        
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        
        return sb.ToString();
    }

    private static string GenererHopitalHtml(FactureDisplayItem item, HospitalInfo hospital, List<LigneFacture> lignesDetails) {
        var sb = new StringBuilder();
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='fr'>");
        sb.AppendLine(GenerateHopitalHtmlHead(item.CodeFacture));
        sb.AppendLine("<body>");
        
        sb.AppendLine(GeneratePrintButton());
        
        sb.AppendLine("<div class='invoice-wrapper'>");
        sb.AppendLine(GenerateHopitalInvoiceHeader(item, hospital));
        sb.AppendLine(GenerateHopitalDetailsSection(item));
        sb.AppendLine(GenerateHopitalInvoiceTable(item, lignesDetails));
        sb.AppendLine(GenerateHopitalFooter());
        sb.AppendLine("</div>");
        
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string GenerateHtmlHead(string billCode, string accentColor) {
        return $@"
    <head>
        <meta charset='UTF-8'>
        <title>Facture {billCode}</title>
        <link href='https://fonts.googleapis.com/css2?family=Alex+Brush&family=Allura&family=Dancing+Script&family=Great+Vibes&family=Inter:wght@300;400;500;600;700&family=Parisienne&family=Sacramento&family=Shadows+Into+Light&display=swap' rel='stylesheet'>
        <style>
            :root {{ 
                --primary: {accentColor}; 
                --text-main: #1e293b; 
                --text-muted: #64748b; 
                --border: #e2e8f0; 
                --bg: #f8fafc; 
            }}
            body {{ font-family: 'Inter', sans-serif; background-color: var(--bg); color: var(--text-main); margin: 0; padding: 40px; line-height: 1.5; }}
            .invoice-wrapper {{ max-width: 850px; margin: 0 auto; background: #ffffff; border-radius: 12px; box-shadow: 0 10px 30px rgba(0,0,0,0.05); overflow: hidden; }}
            .invoice-header {{ display: flex; justify-content: space-between; padding: 40px; background: #ffffff; border-bottom: 1px solid var(--border); }}
            .brand-info h1 {{ margin: 0; color: var(--primary); font-size: 28px; font-weight: 700; letter-spacing: -0.5px; }}
            .brand-info p {{ margin: 4px 0; color: var(--text-muted); font-size: 14px; }}
            .meta-info {{ text-align: right; }}
            .meta-info h2 {{ margin: 0 0 10px 0; font-size: 20px; font-weight: 600; color: var(--text-main); text-transform: uppercase; letter-spacing: 1px; }}
            .badge {{ display: inline-block; padding: 6px 12px; border-radius: 99px; font-size: 12px; font-weight: 600; text-transform: uppercase; background: #dcfce7; color: #166534; }}
            .section {{ padding: 30px 40px; }}
            .patient-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 20px; background: #f8fafc; padding: 24px; border-radius: 8px; border: 1px solid var(--border); }}
            .data-group label {{ display: block; font-size: 12px; font-weight: 600; color: var(--text-muted); text-transform: uppercase; margin-bottom: 4px; }}
            .data-group span {{ font-size: 15px; font-weight: 500; color: var(--text-main); }}
            table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
            th {{ padding: 16px; text-align: left; background: #f1f5f9; color: var(--text-muted); font-weight: 600; font-size: 13px; text-transform: uppercase; }}
            td {{ padding: 16px; border-bottom: 1px solid var(--border); font-size: 14px; color: var(--text-main); }}
            .amount-col {{ text-align: right; }}
            .center-col {{ text-align: center; }}
            .total-section {{ display: flex; justify-content: flex-end; padding: 20px 40px; background: #f8fafc; border-top: 2px solid var(--border); }}
            .total-box {{ width: 300px; }}
            .total-row {{ display: flex; justify-content: space-between; padding: 8px 0; }}
            .total-row.final {{ font-size: 20px; font-weight: 700; color: var(--primary); border-top: 1px solid var(--border); margin-top: 8px; padding-top: 16px; }}
            .footer {{ padding: 40px; text-align: center; border-top: 1px solid var(--border); }}
            .signature-box {{ margin-top: 30px; text-align: right; }}
            .signature-text {{ font-size: 56px; color: #0f172a; margin: 10px 0 0 0; opacity: 0.85; display: inline-block; }}
            
            .print-actions {{ max-width: 850px; margin: 0 auto 20px auto; text-align: right; }}
            .btn-print {{ background-color: var(--primary); color: #ffffff; border: none; padding: 10px 20px; font-size: 14px; font-weight: 600; border-radius: 6px; cursor: pointer; transition: opacity 0.2s; font-family: 'Inter', sans-serif; }}
            .btn-print:hover {{ opacity: 0.9; }}

            @media print {{
                .print-actions {{ display: none; }}
                body {{ padding: 0; background-color: #ffffff; }}
                .invoice-wrapper {{ box-shadow: none; border-radius: 0; }}
            }}
        </style>
    </head>";
    }

    private static string GenerateHopitalHtmlHead(string codeFacture) {
        return $@"
    <head>
        <meta charset='UTF-8'>
        <title>Facture Hôpital {codeFacture}</title>
        <link href='https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap' rel='stylesheet'>
        <style>
            :root {{ 
                --primary: #ef4444; 
                --text-main: #1e293b; 
                --text-muted: #64748b; 
                --border: #e2e8f0; 
                --bg: #f8fafc; 
            }}
            body {{ font-family: 'Inter', sans-serif; background-color: var(--bg); color: var(--text-main); margin: 0; padding: 40px; line-height: 1.5; }}
            .invoice-wrapper {{ max-width: 850px; margin: 0 auto; background: #ffffff; border-radius: 12px; box-shadow: 0 10px 30px rgba(0,0,0,0.05); overflow: hidden; }}
            .invoice-header {{ display: flex; justify-content: space-between; padding: 40px; background: #ffffff; border-bottom: 1px solid var(--border); }}
            .brand-info h1 {{ margin: 0; color: var(--primary); font-size: 28px; font-weight: 700; letter-spacing: -0.5px; }}
            .brand-info p {{ margin: 4px 0; color: var(--text-muted); font-size: 14px; }}
            .meta-info {{ text-align: right; }}
            .meta-info h2 {{ margin: 0 0 10px 0; font-size: 20px; font-weight: 600; color: var(--text-main); text-transform: uppercase; letter-spacing: 1px; }}
            .badge {{ display: inline-block; padding: 6px 12px; border-radius: 99px; font-size: 12px; font-weight: 600; text-transform: uppercase; background: #fee2e2; color: #991b1b; }}
            .section {{ padding: 30px 40px; }}
            .patient-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 20px; background: #f8fafc; padding: 24px; border-radius: 8px; border: 1px solid var(--border); }}
            .data-group label {{ display: block; font-size: 12px; font-weight: 600; color: var(--text-muted); text-transform: uppercase; margin-bottom: 4px; }}
            .data-group span {{ font-size: 15px; font-weight: 500; color: var(--text-main); }}
            table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
            th {{ padding: 16px; text-align: left; background: #f1f5f9; color: var(--text-muted); font-weight: 600; font-size: 13px; text-transform: uppercase; }}
            td {{ padding: 16px; border-bottom: 1px solid var(--border); font-size: 14px; color: var(--text-main); }}
            .amount-col {{ text-align: right; }}
            .total-section {{ display: flex; justify-content: flex-end; padding: 20px 40px; background: #f8fafc; border-top: 2px solid var(--border); }}
            .total-box {{ width: 300px; }}
            .total-row.final {{ font-size: 20px; font-weight: 700; color: var(--primary); border-top: 1px solid var(--border); margin-top: 8px; padding-top: 16px; display: flex; justify-content: space-between; }}
            .footer {{ padding: 40px; text-align: center; border-top: 1px solid var(--border); }}
            .print-actions {{ max-width: 850px; margin: 0 auto 20px auto; text-align: right; }}
            .btn-print {{ background-color: var(--primary); color: #ffffff; border: none; padding: 10px 20px; font-size: 14px; font-weight: 600; border-radius: 6px; cursor: pointer; transition: opacity 0.2s; }}
            
            @media print {{
                .print-actions {{ display: none; }}
                body {{ padding: 0; background-color: #ffffff; }}
                .invoice-wrapper {{ box-shadow: none; border-radius: 0; }}
            }}
        </style>
    </head>";
    }

    private static string GeneratePrintButton() {
        return @"
        <div class='print-actions'>
            <button class='btn-print' onclick='window.print()'>Imprimer la facture</button>
        </div>";
    }

    private static string GenerateInvoiceHeader(string billCode, int simDay, string statut, HospitalInfo hospital) {
        var hospitalName = hospital?.Nom ?? "Modern Hospital";
        var director = hospital?.DirecteurGeneral ?? "Inconnu";

        return $@"
        <div class='invoice-header'>
            <div class='brand-info'>
                <h1>{hospitalName}</h1>
                <p>Rue de Londres 255, 4800 Liège</p>
                <p>Directeur: {director}</p>
                <p>Jour de Simulation: {simDay}</p>
            </div>
            <div class='meta-info'>
                <h2>FACTURE #{billCode}</h2>
                <p style='color: var(--text-muted); font-size: 14px; margin-bottom: 12px;'>Émise le {DateTime.Now:dd/MM/yyyy}</p>
                <span class='badge'>{statut}</span>
            </div>
        </div>";
    }

    private static string GenerateHopitalInvoiceHeader(FactureDisplayItem item, HospitalInfo hospital) {
        var hospitalName = hospital?.Nom ?? "Modern Hospital";
        var director = hospital?.DirecteurGeneral ?? "Inconnu";

        return $@"
        <div class='invoice-header'>
            <div class='brand-info'>
                <h1>{hospitalName}</h1>
                <p>Rue de Londres 255, 4800 Liège</p>
                <p>Directeur: {director}</p>
                <p>Jour de Simulation: {item.JourEmission}</p>
            </div>
            <div class='meta-info'>
                <h2>FACTURE HÔPITAL #{item.CodeFacture}</h2>
                <p style='color: var(--text-muted); font-size: 14px; margin-bottom: 12px;'>Émise le {DateTime.Now:dd/MM/yyyy}</p>
                <span class='badge'>{item.Statut}</span>
            </div>
        </div>";
    }

    private static string GeneratePatientSection(PatientActif patient, string nomComplet) {
        return $@"
        <div class='section'>
            <div class='patient-grid'>
                <div class='data-group'>
                    <label>Patient</label>
                    <span>{nomComplet}</span>
                </div>
                <div class='data-group'>
                    <label>Pathologie / Raison</label>
                    <span>{patient.Maladie}</span>
                </div>
                <div class='data-group'>
                    <label>Classe</label>
                    <span>{patient.Classe}</span>
                </div>
            </div>
        </div>";
    }

    private static string GenerateHospitalisationSection(int stayDays, int treatmentHours) {
        return $@"
        <div class='section' style='padding-top: 0;'>
            <div class='patient-grid' style='background: #f0fdf4; border-color: #bbf7d0;'>
                <div class='data-group'>
                    <label style='color: #15803d; font-size: 12px; font-weight: 600; text-transform: uppercase;'>Temps d'hospitalisation (séjour)</label>
                    <span style='color: #166534; font-size: 15px; font-weight: bold;'>{stayDays} jour(s)</span>
                </div>
                <div class='data-group'>
                    <label style='color: #15803d; font-size: 12px; font-weight: 600; text-transform: uppercase;'>Durée de traitement requis</label>
                    <span style='color: #166534; font-size: 15px; font-weight: bold;'>{treatmentHours} heure(s)</span>
                </div>
            </div>
        </div>";
    }

    private static string GenerateHopitalDetailsSection(FactureDisplayItem item) {
        return $@"
        <div class='section'>
            <div class='patient-grid'>
                <div class='data-group'>
                    <label>Débiteuse</label>
                    <span>Administration Hospitalière</span>
                </div>
                <div class='data-group'>
                    <label>Type de Facture</label>
                    <span>{item.PatientNom}</span>
                </div>
            </div>
        </div>";
    }

    private static string GenerateInvoiceTable(List<LigneFacture> lignes, Facture facture) {
        var lignesHtml = string.Join("\n", lignes.Select(l => $@"
            <tr>
                <td><strong>{l.TypePrestation}</strong></td>
                <td>{l.Description}</td>
                <td class='center-col'>{l.Quantite}</td>
                <td class='amount-col'>${l.PrixUnitaire:F2}</td>
                <td class='amount-col'>${l.SousTotal:F2}</td>
            </tr>"));

        return $@"
        <div class='section' style='padding-top: 0;'>
            <table>
                <thead>
                    <tr>
                        <th>Type</th>
                        <th>Description</th>
                        <th class='center-col'>Qté</th>
                        <th class='amount-col'>Prix Unitaire</th>
                        <th class='amount-col'>Sous-Total</th>
                    </tr>
                </thead>
                <tbody>
                    {lignesHtml}
                </tbody>
            </table>
        </div>
        <div class='total-section'>
            <div class='total-box'>
                <div class='total-row final'>
                    <span>Total à Payer</span>
                    <span>${facture.MontantTotal:F2}</span>
                </div>
            </div>
        </div>";
    }

    private static string GenerateHopitalInvoiceTable(FactureDisplayItem item, List<LigneFacture> lignesDetails) {
        var lignesHtml = string.Empty;

        if (lignesDetails != null && lignesDetails.Any()) {
            lignesHtml = string.Join("\n", lignesDetails.Select(l => $@"
                <tr>
                    <td><strong>{l.TypePrestation}</strong></td>
                    <td>{l.Description}</td>
                    <td class='amount-col'>${l.SousTotal:F2}</td>
                </tr>"));
        } else {
            lignesHtml = $@"
                <tr>
                    <td><strong>{item.PatientNom}</strong></td>
                    <td>{item.Description}</td>
                    <td class='amount-col'>${item.MontantTotal:F2}</td>
                </tr>";
        }

        return $@"
        <div class='section' style='padding-top: 0;'>
            <table>
                <thead>
                    <tr>
                        <th>Prestation</th>
                        <th>Description</th>
                        <th class='amount-col'>Montant</th>
                    </tr>
                </thead>
                <tbody>
                    {lignesHtml}
                </tbody>
            </table>
        </div>
        <div class='total-section'>
            <div class='total-box'>
                <div class='total-row final'>
                    <span>Total Payé par l'Hôpital</span>
                    <span>${item.MontantTotal:F2}</span>
                </div>
            </div>
        </div>";
    }

    private static string GenerateFooter(string doctorName, Random random) {
        var fontIndex = random.Next(SignatureFonts.Length);
        var selectedFont = SignatureFonts[fontIndex];
        
        var rotation = (random.NextDouble() * 4) - 2;
        var skew = (random.NextDouble() * 6) - 3;
        
        var signatureStyle = $"font-family: '{selectedFont}', cursive; transform: rotate({rotation:F2}deg) skewX({skew:F2}deg);";

        return $@"
        <div class='footer'>
            <p style='color: var(--text-muted); font-size: 14px;'>Merci de votre confiance. Ce document est généré automatiquement.</p>
            <div class='signature-box'>
                <p style='font-size: 14px; color: var(--text-muted); text-transform: uppercase; font-weight: 600; letter-spacing: 1px;'>Le médecin traitant</p>
                <p class='signature-text' style=""{signatureStyle}"">{doctorName}</p>
            </div>
        </div>";
    }

    private static string GenerateHopitalFooter() {
        return @"
        <div class='footer'>
            <p style='color: var(--text-muted); font-size: 14px;'>Ce document administratif est généré automatiquement par le système comptable.</p>
        </div>";
    }
}