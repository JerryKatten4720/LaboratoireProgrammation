# ☢️ OVERSEER WARS : Document de Design

> **Univers :** Jeu de stratégie au tour par tour avec des mécaniques de gestion, basé sur l'univers de *Fallout*.

---

## 🎯 1. Concept et Objectif

* **But du jeu :** Éliminer le Superviseur de l'abri adverse.
* **Condition de victoire :** Le Superviseur ne quitte jamais son abri. Pour l'atteindre, le joueur doit d'abord traverser la carte et franchir l'hexagone spécifique **"Porte de l'Abri Adverse"** avant d'engager le combat final.

---

## 🎲 2. Déroulement d'une Partie

### Mise en Place
Chaque joueur commence la partie avec un deck de départ standardisé :
* 👤 **4 Dwellers** (Cartes Personnages), dont 1 est désigné secrètement ou publiquement comme **Superviseur**.
* 🔫 **1 Arme** (Carte Arme).
* 🧥 **2 Tenues** (Cartes Tenues).

### Structure du Tour
Le jeu impose un rythme nerveux (*vibe RTS*) via deux mécaniques limitantes :
1. **Points d'Action (PA) :** Ressource allouée chaque tour pour effectuer des actions (déplacement, assignation à une salle, attaque, etc.).
2. **Timer ⏱️ :** Temps limité par tour. Si le compteur tombe à zéro, le tour passe automatiquement à l'adversaire.

---

## 🏗️ 3. Gestion de l'Abri et Économie

### Les Salles
Les Dwellers sont assignés aux différentes salles de l'abri (représentées sous forme de "tas" de cartes) pour les faire fonctionner.

| Salle | Fonction Principale |
| :--- | :--- |
| ⚡ **Générateur** | Produit de l'Électricité. |
| 🍅 **Jardins** | Produit de la Nourriture. |
| 💧 **Station d'Épuration** | Produit de l'Eau. |
| 🔫 **Fabrique d'Armes** | Permet de créer ou d'améliorer l'arsenal. |
| 🧥 **Fabrique de Tenues** | Permet de créer ou d'améliorer les équipements. |
| 🏋️ **Centre d'Entraînement** | Augmente les statistiques (*S.P.E.C.I.A.L.*) des Dwellers. |

### Centre Technologique (Bonus Globaux)
Débloque des améliorations passives pour l'équipe :
* **Visée fiable :** Dégâts infligés par tous les Dwellers **+1**.
* **Frank le tank :** Dégâts subis par tous les Dwellers **-1**.
* *Logistique avancée (Idée)* : Octroie **+1 PA** maximum par tour.
* *Premiers Secours (Idée)* : Les Dwellers récupèrent **1 PV/tour** dans l'abri.

### Les Ressources Vitales

* ⚡ **Électricité :** Permet de construire de nouvelles salles dans l'abri.
* 💧 **Eau & 🍅 Nourriture :** Maintiennent les Dwellers en vie.
  > ⚠️ **Pénurie :** Si l'eau ou la nourriture tombe à zéro, des dégâts passifs sont infligés à tous les Dwellers du joueur.

---

## 📊 4. Statistiques S.P.E.C.I.A.L.

Chaque statistique a un impact technique direct lors des phases d'exploration ou de combat.

| Statistique | Effet en Jeu |
| :--- | :--- |
| **S** (Force) | Détermine les dégâts infligés au **corps-à-corps**. |
| **P** (Perception) | Augmente les probabilités de réaliser un **coup critique**. |
| **E** (Endurance) | Définit et augmente les **Points de Vie** (PV) maximum. |
| **C** (Charisme) | Augmente les chances de provoquer un **échec critique** chez l'ennemi attaquant. |
| **I** (Intelligence) | Amplifie la **régénération de PV** lors de l'utilisation d'un Stimpack. |
| **A** (Agilité) | Augmente les chances de **fuir** (téléportation à l'abri) au lieu de mourir d'un coup mortel. |
| **L** (Chance) | Augmente les chances d'obtenir un **butin x2** ou d'un palier de rareté supérieur. |

---

## 🗺️ 5. Exploration des Terres Désolées

La carte extérieure est composée d'une dizaine de cases hexagonales.

* 🌫️ **Brouillard de Guerre :** Les hexagones sont face cachée au départ. Déplacer un Dweller sur une case permet de la retourner et de révéler son contenu.
* 🏪 **Lieux-dits :** Certaines cases représentent des zones uniques (*Super Duper Mart, Red Rocket*) disposant de tables de loot spécifiques.
* 🔍 **Fouille (Case vide) :** Si un Dweller arrive sur une case vide d'ennemis, il fouille. *Résultat aléatoire : obtention de nouveaux Dwellers, Armes, Tenues, Ressources ou Matières premières.*

> **Niveaux de Rareté du Butin :**
> Commun ➔ Non commun ➔ Rare ➔ Épique ➔ Légendaire

---

## ⚔️ 6. Système de Combat

* **Déclenchement :** Automatique dès que deux Dwellers (ou groupes) ennemis se retrouvent sur le même hexagone.
* **Résolution (Auto-battler) :** Le combat est entièrement géré par le système via des animations.
* **Déroulé :** Chaque Dweller choisit une cible au hasard et attaque, en prenant en compte la RNG pour les coups et échecs critiques.
* **Fuite d'urgence :** Un Dweller subissant un coup mortel a une chance (basée sur son *Agilité*) d'échapper à la mort et d'être téléporté à l'abri.
* **Loot Pvp :** Lorsqu'un Dweller est définitivement tué, le camp adverse récupère automatiquement tout le butin qu'il avait équipé (Arme, Tenue).

---

## 💻 7. Architecture Technique

Pour garantir un code aéré, propre et hautement modulaire :

### Les Interfaces (Polymorphisme)
Une interface de base pour permettre des transferts fluides d'un "tas" à l'autre dans l'interface utilisateur sans dupliquer la logique.
* `ICard`
    * `IDweller`
    * `IWeapon`
    * `IOutfit`

### Les Zones de Jeu (Les "Tas")
* `CardZone` : Une classe générique gérant la logique d'accueil des cartes.
* **L'Abri** est constitué d'une collection (`List` ou `Dictionary`) de `CardZone` représentant les salles.
* **La Carte** est constituée d'une matrice ou d'un réseau de `CardZone`.

### Les Managers (Séparation des Responsabilités)
* ⏱️ `TurnManager` : Gère le chronomètre global, le cycle de vie du tour, et la distribution/réinitialisation des PA.
* 🧮 `ResourceManager` : Surveille l'économie locale du joueur, valide la construction de salles, et applique les dégâts de pénurie (Eau/Nourriture).
* ⚔️ `CombatManager` : Écoute les événements de déplacement. Si une `CardZone` contient deux équipes différentes, il résout mathématiquement le combat, joue les animations, et redistribue le loot.