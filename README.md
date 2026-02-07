<div align="center">

# 🖥️ Laboratoire de Programmation

### *Application WPF avec esthétique terminal rétro-futuriste*

[![.NET](https://img.shields.io/badge/.NET-Core-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?style=for-the-badge&logo=windows)](https://docs.microsoft.com/fr-fr/dotnet/desktop/wpf/)
[![C#](https://img.shields.io/badge/C%23-11.0-239120?style=for-the-badge&logo=csharp)](https://docs.microsoft.com/fr-fr/dotnet/csharp/)
[![MySQL](https://img.shields.io/badge/MySQL-Connector-4479A1?style=for-the-badge&logo=mysql&logoColor=white)](https://www.mysql.com/)

<img src="https://img.shields.io/badge/Statut-En%20D%C3%A9veloppement-yellow?style=for-the-badge" alt="Status">
<img src="https://img.shields.io/badge/Licence-%C3%89ducative-green?style=for-the-badge" alt="License">

---
Ce fichier README.md a été réalisé par Claude.AI, afin de gagner du temps - @anto.cldl
---

*Une application éducative inspirée des terminaux classiques, combinant exercices interactifs, gestion de bases de
données et intégration IA dans une interface rétro captivante.*

[Fonctionnalités](#-fonctionnalités) • [Installation](#-installation) • [Utilisation](#-guide-dutilisation) • [Architecture](#-architecture)

</div>

---

## 📋 Table des matières

- [Vue d'ensemble](#-vue-densemble)
- [Fonctionnalités clés](#-fonctionnalités-clés)
- [Stack technique](#-stack-technique)
- [Structure du projet](#-structure-du-projet)
- [Installation](#-installation)
- [Guide d'utilisation](#-guide-dutilisation)
- [Exercices & Laboratoires](#-exercices--laboratoires)
- [Architecture](#-architecture)
- [Design visuel](#-design-visuel)
- [Améliorations futures](#-améliorations-futures)

---

## 🎯 Vue d'ensemble

**Laboratoire de Programmation** est une application éducative WPF au style terminal vintage, inspirée des interfaces
ROBCO de Fallout. Elle combine des exercices pratiques de programmation avec une expérience visuelle immersive dans un
environnement de terminal phosphorescent vert.

### 🌟 Philosophie de conception

- **Esthétique rétro** : Terminal phosphorescent avec effets CRT authentiques
- **Apprentissage interactif** : Exercices pratiques pour l'UI et la gestion de données
- **Interface ligne de commande** : Navigation nostalgique avec fonctionnalités modernes
- **Intégration IA** : Chatbot MemfyAI personnalisé

---

## 🚀 Fonctionnalités clés

### 🖥️ Interface Terminal

- **Navigation par commandes** : Système de commandes classique (`help`, `clear`, `exit`)
- **Historique** : Navigation dans les commandes précédentes (↑/↓)
- **Ajustement dynamique** : Taille de police ajustable en temps réel
- **Horodatage** : Toutes les sorties avec timestamp
- **Codage couleur** : Messages différenciés (info, avertissement, erreur, succès)

### 📚 Exercices interactifs

#### **Exercice 1 & 1B** - Maîtrise de l'interface utilisateur

- Basculement dynamique de la visibilité des boutons
- Attribution aléatoire de polices de caractères
- Manipulation dynamique de la taille de police
- Randomisation des couleurs de fond
- Permutation de position d'images dans une grille
- Basculement de visibilité indépendant pour chaque image

**Concepts clés** :

```csharp
// Manipulation de la grille en temps réel
if (Grid.GetColumn(IMG1) == 0) {
    Grid.SetColumn(IMG1, 2);
    Grid.SetColumn(IMG2, 0);
}
```

#### **Exercice 2** - Simulation de transfert de fichiers

- Système de double barre de progression (globale + fichier actuel)
- Animation de transfert de dossiers
- Exécution de tâches asynchrones
- Délais aléatoires pour un comportement réaliste
- État de succès avec feedback visuel

**Architecture asynchrone** :

```csharp
private async Task StartBackgroundWork() {
    while (!_isComplete) {
        for (int i = 0; i < 100; i++) {
            ProgressBar2.Dispatcher.Invoke(() => { 
                ProgressBar2.Value++; 
            });
            await Task.Delay(random.Next(10, 80));
        }
    }
}
```

#### **Laboratoire 1** - Système de gestion de base de données 🚧

- Interface de connexion MySQL avec validation
- Mécanisme de réessai (4 tentatives max)
- Curseur personnalisé pendant la connexion
- Indicateurs d'état visuels (succès/erreur/verrouillé)
- Opérations CRUD complètes *(en développement)*

### 🤖 Intégration MemfyAI

- Visualisation en streaming des réponses IA
- Traitement de chunks asynchrone via `IAsyncEnumerable`
- Persistance de session avec token de sortie
- Rendu de texte en temps réel
- Écran d'accord/disclaimer dédié

### 🎨 Effets visuels

- **Shader CRT** : Glitch, décroissance du phosphore, simulation de burn-in
- **Palette de couleurs** : 7 couleurs de terminal prédéfinies
- **Animations fluides** : Transitions d'opacité et fondus
- **Chrome personnalisé** : Fenêtre sans cadre avec contrôles sur mesure
- **Mise en page responsive** : Dimensionnement adaptatif

---

## 🛠️ Stack technique

| Catégorie               | Technologie                   | Usage                     |
|-------------------------|-------------------------------|---------------------------|
| **Framework**           | .NET Core 8+                  | Runtime applicatif        |
| **UI**                  | WPF                           | Interface utilisateur     |
| **Langage**             | C# 11.0                       | Logique applicative       |
| **Base de données**     | MySQL                         | Persistance des données   |
| **Driver BDD**          | MySqlConnector                | Connexion MySQL           |
| **Architecture**        | Pattern MVC personnalisé      | Organisation du code      |
| **Programmation async** | Async/Await, IAsyncEnumerable | Opérations non-bloquantes |
| **Client HTTP**         | HttpClient                    | Communication API IA      |
| **Typographie**         | Police Overseer + Consolas    | Esthétique terminal       |

---

## 📁 Structure du projet

```
LaboratoireProgrammation/
├── 📂 Public/
│   ├── 📂 Controller/
│   │   └── 📂 Misc/
│   │       └── MemfyAI.cs                 # Service contrôleur IA
│   │
│   ├── 📂 Model/
│   │   ├── 📂 Exercices/
│   │   │   ├── Exo - 1.xaml/.cs           # Exercice interaction boutons
│   │   │   ├── Exo - 1b.xaml/.cs          # Exercice manipulation images
│   │   │   ├── Exo - 2.xaml/.cs           # Simulation barres de progression
│   │   │   ├── BabyMode.xaml/.cs          # Interface navigation simplifiée
│   │   │   └── BackToMain.xaml/.cs        # Aide navigation
│   │   │
│   │   ├── 📂 Laboratoires/
│   │   │   └── 📂 Laboratoire - 1/
│   │   │       ├── ConnectControl.xaml/.cs    # Interface connexion BDD
│   │   │       └── DatabaseTopBar.xaml/.cs    # Barre navigation BDD
│   │   │
│   │   ├── 📂 Misc/
│   │   │   ├── GoBackButton.xaml/.cs      # Bouton retour réutilisable
│   │   │   └── MemfyAgreement.xaml/.cs    # Écran disclaimer IA
│   │   │
│   │   ├── MainWindow.xaml.cs             # Logique principale (475 lignes)
│   │   └── MainWindowConfiguration.cs     # Configuration fenêtre
│   │
│   ├── 📂 View/
│   │   ├── 📂 ResourceDictionary/
│   │   │   └── TerminalStyle.xaml         # Styles XAML partagés
│   │   ├── MainWindow.xaml                # Layout UI principal
│   │   └── Loader.xaml/.cs                # Écran de chargement
│   │
│   ├── 📂 Visual-Utils/
│   │   └── ColorUtils.cs                  # Palette couleurs & utilitaires
│   │
│   └── 📂 Laboratoires/
│       └── 📂 LabVault/
│           └── 📂 Effects/
│               └── 📂 Compiled/           # Effets shader compilés
│
├── 📂 Assets/
│   ├── 📂 fonts/overseer/                 # Police terminal personnalisée
│   ├── 📂 images/hourglass.png            # Curseur personnalisé
│   └── 📂 shaders/wrappers/               # Effets visuels CRT
│
└── 📂 Utils/
    ├── SizeUtils.cs                       # Utilitaires dimensionnement
    ├── FontUtils.cs                       # Randomisation polices
    ├── WindowUtils.cs                     # Helpers fenêtre parente
    └── SqlUtils.cs                        # Utilitaires base de données
```

---

## 💾 Installation

### Prérequis

- **Windows 10/11** (requis pour WPF)
- **.NET 8.0 SDK** ou supérieur
- **Visual Studio 2022** (recommandé) ou JetBrains Rider
- **MySQL Server** (pour Laboratoire 1)
- **Backend MemfyAI** (optionnel, pour fonctionnalités IA)

### Étapes d'installation

1. **Cloner le dépôt**
   ```bash
   git clone https://github.com/votrecompte/laboratoire-programmation.git
   cd laboratoire-programmation
   ```

2. **Restaurer les dépendances**
   ```bash
   dotnet restore
   ```

3. **Configurer MySQL** (pour Lab 1)
   ```sql
   CREATE DATABASE laboratoire_db;
   ```

4. **Configurer MemfyAI** (optionnel)
   ```csharp
   // Dans MemfyAI.cs, ligne 10 :
   BaseAddress = new Uri("http://localhost:8000")
   ```

5. **Compiler et exécuter**
   ```bash
   dotnet build
   dotnet run
   ```

---

## 🎮 Guide d'utilisation

### Commandes du terminal

```bash
# Afficher toutes les commandes disponibles
> help

# Obtenir les informations système
> info

# Effacer l'écran du terminal
> clear

# Ajuster la taille de police (utiliser ↑/↓)
> font

# En savoir plus sur le système
> about

# Lancer les exercices
> exo1      # Interactions boutons + Manipulation images
> exo2      # Simulation transfert fichiers

# Lancer le laboratoire
> lab1      # Système gestion base de données

# Lancer l'assistant IA
> memfyai   # Démarrer conversation MemfyAI

# Quitter l'application
> exit
```

### Astuces de navigation

- **Flèches** : Naviguer dans l'historique (↑ = précédent, ↓ = suivant)
- **Entrée** : Exécuter une commande ou confirmer
- **ESC** : Annuler le mode ajustement police
- **Contrôles fenêtre** : Minimiser, Maximiser, Fermer (coin supérieur droit)

### Mode Baby 🍼

Pour les utilisateurs préférant une interface graphique aux commandes terminal, le **Baby Mode** propose des boutons
cliquables pour tous les exercices.

---

## 🏋️ Exercices & Laboratoires

### 📝 Exercice 1 : Contrôle de composants UI

**Objectifs d'apprentissage** :

- Maîtriser la propriété `Visibility` de WPF
- Comprendre la programmation événementielle
- Utiliser `Grid.SetColumn()` pour layouts dynamiques
- Implémenter l'attribution aléatoire de polices

**Éléments interactifs** :

- Système de basculement bouton ON/OFF
- 3 boutons conditionnels (Police, Taille, Couleur)
- Mises à jour dynamiques du label
- Randomisation couleur de fond

---

### 🖼️ Exercice 1B : Manipulation de grille d'images

**Objectifs d'apprentissage** :

- Manipulation de colonnes de grille à l'exécution
- Gestion d'état booléen
- Basculement de visibilité conditionnelle
- Contrôle UI basé sur les coordonnées

**Éléments interactifs** :

- Permutation de position d'images (gauche ↔ droite)
- Basculement de visibilité indépendant pour chaque image
- Suivi d'état avec flag `_interverted`

---

### 📊 Exercice 2 : Simulation de progression asynchrone

**Objectifs d'apprentissage** :

- Programmation asynchrone avec `async/await`
- Mises à jour UI multi-thread avec `Dispatcher`
- Manipulation de barres de progression
- Coordination et complétion de tâches

**Fonctionnalités** :

- Suivi double progression (globale + fichier actuel)
- Transitions animées d'icônes de dossiers
- Simulation de délais aléatoires (10-80ms)
- État de succès avec transitions de couleurs

---

### 🗄️ Laboratoire 1 : Système de gestion de base de données

**Statut** : 🚧 *En développement actif*

**Fonctionnalités planifiées** :

- ✅ Interface connexion MySQL avec validation
- ✅ Mécanisme de réessai (4 tentatives) avec feedback visuel
- ✅ Curseur sablier personnalisé pendant connexion
- ✅ États colorés succès/erreur/verrouillé
- 🚧 Opérations CRUD complètes (Create, Read, Update, Delete)
- 🚧 Affichage grille de données
- 🚧 Fonctionnalités recherche et filtrage
- 🚧 Export CSV/Excel

**Implémentation actuelle** :

```csharp
var db = new SqlUtils(() => new MySqlConnection(
    $"Server={server};Database={database};User={user};Password={password}"
));

try { 
    db.AssertStatus(); 
    ConfirmConnection(); // État succès vert
}
catch (Exception ex) { 
    TrialsLeft--;
    if (TrialsLeft == 0) LockTrials(); // État verrouillé rouge
}
```

---

## 🏗️ Architecture

### Pattern actuel : MVC personnalisé (adapté WPF)

```
┌─────────────┐
│    View     │ ← Fichiers XAML + code-behind minimal
│  (UI Layer) │   
└──────┬──────┘
       │ Interactions utilisateur
       ↓
┌─────────────┐
│    Model    │ ← Logique applicative + état
│  (Business) │   MainWindow.xaml.cs gère la majorité de la logique
└──────┬──────┘
       │ Requêtes données
       ↓
┌─────────────┐
│ Controller  │ ← Services (MemfyAI)
│  (Services) │   
└─────────────┘
```

### ⚠️ Considérations architecturales

**Points à améliorer** :

- `MainWindow.xaml.cs` dans `/Model/` viole la séparation des préoccupations
- Code-behind contient 475 lignes de logique mixte UI + métier
- Manipulation directe de l'UI au lieu de data binding
- Absence de vraies classes Model (structures de données)

**Recommandé : Pattern MVVM**

```
View (XAML) ← DataBinding → ViewModel (Logique) → Model (Données) → Services
```

### Répartition des composants

#### **Controllers** (Services)

- `MemfyAI.cs` : Client HTTP streaming pour backend IA
    - Gestion de réponse async enumerable
    - Streaming texte par chunks
    - Support de cancellation token

#### **Models** (Logique mixte + UI)

- `MainWindow.xaml.cs` : Orchestrateur principal
    - Traitement de commandes (`ProcessCommand()`)
    - Lanceurs d'exercices (`FirstExo()`, `SecondExo()`)
    - Gestion sortie terminal
    - Mode conversation MemfyAI

#### **Views** (Layouts XAML)

- `MainWindow.xaml` : Structure fenêtre principale
- `Loader.xaml` : Écran de démarrage
- `TerminalStyle.xaml` : Ressources de style partagées

#### **Utilitaires visuels**

- `ColorUtils.cs` : Palette de couleurs centralisée
    - 7 couleurs sémantiques (Text, Warning, Critical, Success, etc.)
    - Convertisseur Hex-vers-Color
    - Générateur de couleurs aléatoires

---

## 🎨 Design visuel

### Palette de couleurs

| Nom             | Hex       | Usage                      |
|-----------------|-----------|----------------------------|
| **FancyText**   | `#fcfe4d` | Vert terminal (primaire)   |
| **Text**        | `#ededed` | Texte standard             |
| **UserInput**   | `#6ee9ff` | Écho commandes utilisateur |
| **Information** | `#fdff6e` | Messages info              |
| **Warning**     | `#ff8f4a` | États d'avertissement      |
| **Critical**    | `#c7312c` | Messages d'erreur          |
| **Success**     | `#29e62c` | États de succès            |

### Typographie

- **Police principale** : `Overseer` (police terminal personnalisée)
- **Police de secours** : `Consolas` (monospace)
- **Tailles** :
    - En-tête : 16px
    - Corps : 18px
    - Dynamique : Ajustable via commande `font`

### Système d'effets visuels

```csharp
// Propriétés du shader CRT (dans VaultShader)
- SmokeDensity: Simule les interférences du tube cathodique
- GlitchIntensity: Glitchs aléatoires de lignes de balayage
- PhosphorDecay: Effet de décroissance du phosphore vert
- BurnInIntensity: Simulation de brûlure d'écran
- Contrast & Brightness: Correction des couleurs
- TintColor: Couleur de superposition (par défaut : vert léger)
```

**Activation effets spéciaux** :

```csharp
// Entrée Laboratoire 1 - Mode alerte rouge
VaultShader.GlitchIntensity *= 5;
VaultShader.SmokeDensity = 1.01;
VaultShader.TintColor = Color.FromArgb(5, 255, 0, 0); // Teinte rouge
```

---

## 🔮 Améliorations futures

### Fonctionnalités planifiées

- [ ] **UI CRUD de base de données** - Compléter la gestion de données Lab 1
- [ ] **Exercices 3-5** - Défis de programmation supplémentaires
- [ ] **Support multilingue** - Internationalisation (FR/EN)
- [ ] **Système de configuration** - Préférences utilisateur persistantes
- [ ] **Système de succès** - Gamification avec suivi de progression
- [ ] **Personnalisation thèmes** - Jeux de couleurs alternatifs (ambre, bleu)
- [ ] **Feedback audio** - Bips de terminal et sons de frappe
- [ ] **Simulation système de fichiers** - Navigation répertoire virtuel
- [ ] **Module réseau** - Exercices programmation socket

### Améliorations techniques

- [ ] **Refactoring MVVM** - Implémentation architecture appropriée
- [ ] **Injection de dépendances** - Gestion durée de vie des services
- [ ] **Tests unitaires** - Couverture de tests xUnit
- [ ] **Système de logging** - Intégration Serilog
- [ ] **Gestion d'erreurs** - Gestion centralisée des exceptions
- [ ] **Documentation** - Commentaires XML + docs API

---

## 🤝 Contribution

Les contributions sont les bienvenues ! Ce projet éducatif est parfait pour apprendre WPF et les patterns C# modernes.

### Comment contribuer

1. **Fork** le dépôt
2. **Créer** une branche de fonctionnalité (`git checkout -b feature/NouvelleFonctionnalité`)
3. **Commit** vos changements (`git commit -m 'Ajout NouvelleFonctionnalité'`)
4. **Push** vers la branche (`git push origin feature/NouvelleFonctionnalité`)
5. **Ouvrir** une Pull Request

### Guidelines de contribution

- Suivre le style de code existant
- Ajouter des commentaires de documentation XML
- Tester sur Windows 10 & 11
- Mettre à jour le README si ajout de fonctionnalités
- Garder les commits atomiques et bien décrits

---

## 🐛 Problèmes connus

- ⚠️ **Architecture** : Dossier Model contient logique UI (nécessite refactoring)
- ⚠️ **Performance** : Les shaders CRT peuvent impacter les systèmes bas de gamme
- ⚠️ **Plateforme** : Windows uniquement (limitation WPF)
- ⚠️ **MemfyAI** : Nécessite un service backend externe
- ⚠️ **Lab 1** : Opérations CRUD pas encore implémentées

---

## 📄 Licence

Ce projet est sous **Licence d'Usage Éducatif** - gratuit pour l'apprentissage, l'enseignement et les usages non
commerciaux.

**Attribution requise** :

```
Auteur original : @anto.cldl
Projet : Laboratoire de Programmation
Année : 2026
```

---

## 👨‍💻 Auteur

**@anto.cldl**

*Développeur Étudiant | Passionné WPF | Amateur de Technologie Rétro*

---

## 🙏 Remerciements

- **Série Fallout** - Inspiration pour l'esthétique terminal
- **Anthropic Claude** - Assistance intégration IA
- **Communauté WPF** - Documentation et support
- **Équipe MySQL** - Connectivité base de données
- **Police Overseer** - Typographie terminal personnalisée

---

<div align="center">

### ⭐ Mettez une étoile si vous trouvez ce projet utile !

**Construit avec ❤️ et beaucoup de commandes terminal**

`> Système opérationnel. En attente d'entrée...`

---

```ascii
┌─────────────────────────────────────────────┐
│  ROBCO UNIFIED OPERATING SYSTEM v1.0        │
│  COPYRIGHT 2026 @ANTO.CLDL                  │
│  TOUS DROITS RÉSERVÉS                       │
│                                             │
│  > SYSTÈME PRÊT                             │
│  > MERCI DE VOTRE VISITE                    │
│  > APPUYEZ SUR UNE TOUCHE POUR CONTINUER... │
└─────────────────────────────────────────────┘
```

**[⬆ Retour en haut](#-laboratoire-de-programmation)**

</div>