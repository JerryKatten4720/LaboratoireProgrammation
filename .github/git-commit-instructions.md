# Instructions pour les messages de commit

## Langue et Ton
- Rédige impérativement tous les messages en **français**.
- Utilise le **mode impératif** (ex: "Ajouter" au lieu de "A ajouté").
- Sois concis, technique et précis.

## Structure du Message
Chaque message doit suivre rigoureusement cette structure :

1. **En-tête (Format Conventional Commits)** :
   `<type>: <description courte en minuscule>`
   (Types : feat, fix, refactor, docs, style, chore, test)

2. **Résumé du changement** :
   Une brève explication de "pourquoi" ce changement a été effectué (le contexte).

3. **Détails par Catégorie** :
   Divise les modifications par fichiers en utilisant des titres clairs. Regroupe les fichiers par action :
   Utilise obligatoirement les puces suivantes pour les listes : ✨ pour Ajout, ⚙️ pour Modification,🗑️ pour Suppression.
- ✨ **AJOUTS** : [Nom du fichier] - [Rôle]
- ⚙️ **MODIFICATIONS** : [Nom du fichier] - [Détail]
- 🗑️ **SUPPRESSIONS** : [Nom du fichier] - [Raison]

4. **Pied de page (Optionnel)** :
    - Mentionne "Fixes #ID" si un ticket est détecté.

## Règles de Qualité
- Ne pas lister les changements triviaux (ex: espaces, imports non utilisés).
- Si une modification impacte la logique métier, explique l'impact.
- Utilise des listes à puces pour la lisibilité.
- Aligne les icônes (emoji) pour un rendu visuel propre dans Rider.