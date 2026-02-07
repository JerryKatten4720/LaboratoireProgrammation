# Instructions pour les messages de commit

## Langue et Ton

- Rédige impérativement tous les messages en **français**.
- Utilise le **mode impératif** (ex: "Ajouter" au lieu de "A ajouté").
- Sois concis, technique et précis.
- N'ommet aucun fichier modifié / supprimé / ajouté / déplacé

## Structure du Message

Chaque message doit suivre rigoureusement cette structure :

1. **En-tête (Format Conventional Commits)** :
   `<type>: <description courte en minuscule>`
   (Types : feat, fix, refactor, docs, style, chore, test)

2. **Résumé du changement** :
   Une brève explication du "pourquoi" (le contexte).

3. **Détails par Catégorie** :
   Divise les modifications par catégories avec les titres et emojis suivants (ne pas utiliser de backticks ` `) :

    - ✨ **AJOUTS** : [Nom du fichier] - [Rôle]
    - ⚙️ **MODIFICATIONS** : [Nom du fichier] - [Détail]
    - 🗑️ **SUPPRESSIONS** : [Nom du fichier] - [Raison]
    - 🚚 **DÉPLACEMENT** : [Nom du fichier] -> [Nouvel Emplacement (relatif à la racine)]

4. **Pied de page (Optionnel)** :
    - Mentionne "Fixes #ID" si un ticket est détecté dans la branche.

## Règles de Qualité

- **Formatage** : Ne jamais entourer les noms de fichiers ou de dossiers par des accents graves (`). Utiliser
  exclusivement les crochets [ ].
- **Pertinence** : Ne pas lister les changements triviaux (espaces, imports).
- **Lisibilité** : Aligne bien les icônes pour un rendu propre dans l'historique Git de Rider.