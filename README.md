
# Optimium – Optimisation intelligente des réservations de salles

Optimium est une application Python avec interface graphique Streamlit permettant d’optimiser l’affectation des réservations de salles à partir d’un fichier Excel, en maximisant le taux d’occupation tout en respectant les contraintes de capacité et de conflits horaires. L’application propose une interface web interactive et un exécutable autonome.

---

## Fonctionnalités principales

- **Optimisation intelligente** des réservations de salles (algorithmes glouton et génétique)
- **Détection et gestion des doublons** dans les réservations
- **Correction automatique** du nombre d’inscrits par groupe
- **Rapports détaillés** et analyses graphiques
- **Export Excel** avec formatage conditionnel
- **Interface multilingue** (français/anglais)
- **Utilisation en mode graphique (Streamlit) ou exécutable (.exe)**

---

## Structure du projet

- `opti.py` : Script principal Streamlit, interface utilisateur et logique d’optimisation
- `run_app.py` : Lanceur pour exécuter l’application en mode exécutable (PyInstaller)
- `ExtractSalleRouenCESI.xlsx` : Exemple de catalogue de salles (Excel)
- `requirements.txt` : Dépendances Python nécessaires
- `README.md` : Documentation du projet

---

## Installation

### 1. Prérequis

- Python 3.7+
- pip (gestionnaire de paquets Python)
- (Optionnel) Pour l’exécutable : Windows

### 2. Installation des dépendances

```bash
pip install -r requirements.txt
```

---

## Utilisation

### 1. Lancer l’application en mode graphique (Streamlit)

```bash
streamlit run opti.py
```

Ouvre ensuite [http://localhost:8501](http://localhost:8501) dans ton navigateur.

### 2. Générer et utiliser l’exécutable Windows

1. Installe PyInstaller si besoin :
   ```bash
   pip install pyinstaller
   ```
2. Génére l’exécutable :
   ```bash
   pyinstaller --onefile --collect-all streamlit run_app.py
   ```
3. Place le fichier `opti.py` dans le dossier `dist/` à côté de `run_app.exe`.
4. Lance l’exécutable :
   ```powershell
   .\dist\run_app.exe
   ```
5. Ouvre [http://localhost:8501](http://localhost:8501) dans ton navigateur.

---

## Fonctionnement détaillé

### a) Chargement et nettoyage des données

- Chargement du catalogue de salles (`ExtractSalleRouenCESI.xlsx`)
- Suppression des doublons et normalisation des noms de salles
- Import des réservations utilisateur (Excel)
- Vérification des colonnes essentielles et détection des incohérences

### b) Détection et gestion des doublons

- Identification des réservations identiques (CodeAnalytique, date, heure, inscrits)
- Conservation de la réservation principale, marquage des doublons

### c) Optimisation de l’affectation des salles

- **Algorithme glouton intelligent** : attribution séquentielle optimisée
- **Algorithme génétique** : optimisation globale par évolution de solutions

### d) Export et rapports

- Export des résultats en Excel avec formatage conditionnel
- Analyses graphiques : taux d’occupation, utilisation des salles, diagnostics

---

## Conseils d’utilisation

- Vérifiez la cohérence des noms de salles dans vos fichiers sources
- Nettoyez les fichiers Excel pour éviter les réservations incomplètes ou incohérentes
- Utilisez l’algorithme génétique pour les cas complexes avec de nombreux conflits

---

## Dépendances principales

- streamlit
- pandas
- numpy
- openpyxl

---

## Auteur

Optimium – Projet CESI 2024

Pour toute question ou suggestion, ouvrez une issue ou contactez l’équipe projet.
