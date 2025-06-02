import streamlit as st
import pandas as pd
import os
from io import BytesIO
from datetime import datetime # Ajout pour la combinaison date/heure

# --- CONSTANTES ---
NOM_FICHIER_SALLES = "ExtractSalleRouenCESI.xlsx"
COLONNE_NOM_SALLE = "Nom Salle" 
COLONNE_CAPACITE_SALLE = "CapaciteSalle"

# Colonnes dans le fichier de réservations uploadé
COLONNE_NOMBRE_INSCRIT = "NombreInscrit"
COLONNE_ANCIENNE_SALLE_BOOKING = "NomSalle" # Ancienne salle dans le fichier de réservation
COLONNE_DATE_BOOKING = "Date" # Tel que spécifié par l'utilisateur
COLONNE_HEURE_DEBUT_BOOKING = "Début" # Tel que spécifié par l'utilisateur
COLONNE_HEURE_FIN_BOOKING = "Fin"   # Tel que spécifié par l'utilisateur

# Colonnes générées dans le DataFrame de sortie
COLONNE_SALLE_OPTIMISEE = "SalleOptimisee"
COLONNE_START_DATETIME = 'start_datetime' # Interne pour le traitement
COLONNE_END_DATETIME = 'end_datetime'     # Interne pour le traitement


@st.cache_data
def charger_donnees_salles(chemin_fichier):
    """Charge les données des salles depuis le fichier Excel spécifié."""
    try:
        df_salles = pd.read_excel(chemin_fichier)
        if COLONNE_NOM_SALLE not in df_salles.columns or COLONNE_CAPACITE_SALLE not in df_salles.columns:
            st.error(
                f"ERREUR CRITIQUE : Le fichier des salles '{chemin_fichier}' doit contenir les colonnes spécifiées.\n"
                f"Colonne nom de salle ATTENDUE : '{COLONNE_NOM_SALLE}'\n"
                f"Colonne capacité ATTENDUE : '{COLONNE_CAPACITE_SALLE}'\n"
                f"Colonnes TROUVÉES : {df_salles.columns.tolist()}"
            )
            return None
        df_salles[COLONNE_CAPACITE_SALLE] = pd.to_numeric(df_salles[COLONNE_CAPACITE_SALLE], errors='coerce')
        df_salles = df_salles.dropna(subset=[COLONNE_NOM_SALLE, COLONNE_CAPACITE_SALLE])
        df_salles[COLONNE_NOM_SALLE] = df_salles[COLONNE_NOM_SALLE].astype(str) # Assurer que les noms de salle sont des chaînes
        return df_salles
    except FileNotFoundError:
        st.error(f"Fichier des salles '{chemin_fichier}' non trouvé.")
        return None
    except Exception as e:
        st.error(f"Erreur chargement fichier salles : {e}")
        return None

def is_overlapping(new_start, new_end, existing_start, existing_end):
    """Vérifie si deux créneaux horaires se chevauchent."""
    return new_start < existing_end and new_end > existing_start

def is_room_free(room_schedule_list, new_start, new_end):
    """Vérifie si une salle est libre pour un nouveau créneau."""
    if pd.isna(new_start) or pd.isna(new_end): # Ne peut pas vérifier si les dates sont invalides
        return False
    for existing_start, existing_end in room_schedule_list:
        if is_overlapping(new_start, new_end, existing_start, existing_end):
            return False
    return True

def optimiser_reservations(df_reservations_input, df_salles_master):
    if df_reservations_input is None or df_salles_master is None:
        return None

    required_booking_cols = [COLONNE_NOMBRE_INSCRIT, COLONNE_DATE_BOOKING, COLONNE_HEURE_DEBUT_BOOKING, COLONNE_HEURE_FIN_BOOKING]
    for col in required_booking_cols:
        if col not in df_reservations_input.columns:
            st.error(f"Le fichier de réservations doit contenir la colonne '{col}'.")
            return None
            
    df_reservations = df_reservations_input.copy()

    # 1. Préparation des dates et heures
    try:
        # Convertir les colonnes de date et d'heure en chaînes pour une concaténation sûre
        df_reservations[COLONNE_DATE_BOOKING] = df_reservations[COLONNE_DATE_BOOKING].astype(str)
        df_reservations[COLONNE_HEURE_DEBUT_BOOKING] = df_reservations[COLONNE_HEURE_DEBUT_BOOKING].astype(str)
        df_reservations[COLONNE_HEURE_FIN_BOOKING] = df_reservations[COLONNE_HEURE_FIN_BOOKING].astype(str)

        # Supprimer les ".0" si les heures sont lues comme des flottants (ex: 10:00 lu comme 10.0)
        # et s'assurer que le format est gérable par to_datetime pour les heures.
        # Ceci est une supposition, le format réel des heures peut nécessiter un ajustement plus précis.
        # Par exemple, si l'heure est '10:3', pd.to_datetime peut avoir besoin d'aide.
        # Si les heures sont déjà des objets time ou datetime, cette conversion peut être simplifiée.

        df_reservations[COLONNE_START_DATETIME] = pd.to_datetime(
            df_reservations[COLONNE_DATE_BOOKING] + ' ' + df_reservations[COLONNE_HEURE_DEBUT_BOOKING], 
            dayfirst=True, errors='coerce' # dayfirst=True pour JJ/MM/AAAA
        )
        df_reservations[COLONNE_END_DATETIME] = pd.to_datetime(
            df_reservations[COLONNE_DATE_BOOKING] + ' ' + df_reservations[COLONNE_HEURE_FIN_BOOKING], 
            dayfirst=True, errors='coerce'
        )
    except Exception as e:
        st.error(f"Erreur lors de la conversion des dates/heures : {e}. Vérifiez les formats JJ/MM/AAAA pour les dates et HH:MM pour les heures.")
        return None

    df_reservations[COLONNE_NOMBRE_INSCRIT] = pd.to_numeric(df_reservations[COLONNE_NOMBRE_INSCRIT], errors='coerce')

    # Filtrer les réservations invalides (date/heure invalide, NombreInscrit invalide, fin <= début)
    condition_valide = (
        df_reservations[COLONNE_START_DATETIME].notna() &
        df_reservations[COLONNE_END_DATETIME].notna() &
        (df_reservations[COLONNE_END_DATETIME] > df_reservations[COLONNE_START_DATETIME]) &
        df_reservations[COLONNE_NOMBRE_INSCRIT].notna() &
        (df_reservations[COLONNE_NOMBRE_INSCRIT] > 0)
    )
    df_reservations_valides = df_reservations[condition_valide].copy()
    df_reservations_invalides = df_reservations[~condition_valide].copy()

    if df_reservations_valides.empty:
        st.warning("Aucune réservation valide (date/heure correcte, NombreInscrit > 0) trouvée à traiter.")
        # Préparer toutes les colonnes de sortie pour df_reservations_invalides
        df_reservations_invalides[COLONNE_SALLE_OPTIMISEE] = "Réservation invalide (date/heure/inscrits)"
        cols_gain = ['CapaciteSalleOptimisee', 'RatioOptimise', 'SiegesExcedentairesOptimises',
                     'AncienneSalle', 'CapaciteAncienneSalle', 'RatioAncien', 'SiegesExcedentairesAnciens',
                     'AmeliorationScoreFit', 'ReductionSiegesExcedentaires']
        for col in cols_gain:
            df_reservations_invalides[col] = pd.NA
        return df_reservations_invalides


    # 2. Trier les réservations valides: NombreInscrit (desc), puis start_datetime (asc)
    df_reservations_valides = df_reservations_valides.sort_values(
        by=[COLONNE_NOMBRE_INSCRIT, COLONNE_START_DATETIME], ascending=[False, True]
    ).reset_index(drop=True) # Reset index après tri

    # 3. Initialiser les plannings des salles
    room_schedules = {room_name: [] for room_name in df_salles_master[COLONNE_NOM_SALLE].unique()}
    map_nom_salle_a_capacite = pd.Series(df_salles_master[COLONNE_CAPACITE_SALLE].values, index=df_salles_master[COLONNE_NOM_SALLE]).to_dict()
    
    resultats_optimises_list = []

    for index, reservation in df_reservations_valides.iterrows():
        nombre_inscrits = reservation[COLONNE_NOMBRE_INSCRIT]
        current_booking_start_dt = reservation[COLONNE_START_DATETIME]
        current_booking_end_dt = reservation[COLONNE_END_DATETIME]
        
        # Init métriques
        nom_ancienne_salle_booking = reservation.get(COLONNE_ANCIENNE_SALLE_BOOKING)
        capacite_ancienne_salle, ratio_ancien, sieges_excedentaires_anciens = pd.NA, pd.NA, pd.NA
        salle_optimale_nom = "Aucune salle adaptée disponible"
        capacite_salle_optimisee, ratio_optimise, sieges_excedentaires_optimises = pd.NA, pd.NA, pd.NA
        amelioration_score_fit, reduction_sieges_excedentaires = pd.NA, pd.NA

        if pd.notna(nom_ancienne_salle_booking) and nom_ancienne_salle_booking in map_nom_salle_a_capacite:
            capacite_ancienne_salle = map_nom_salle_a_capacite[nom_ancienne_salle_booking]
            if pd.notna(capacite_ancienne_salle):
                ratio_ancien = capacite_ancienne_salle / nombre_inscrits
                sieges_excedentaires_anciens = capacite_ancienne_salle - nombre_inscrits
        
        # Trouver salles candidates
        potential_rooms_for_this_booking = []
        # Trier df_salles_master une fois pour l'itération (déjà fait en tant que df_salles_trie avant, mais on peut le refaire)
        # ou utiliser df_salles_master directement si l'ordre initial est OK pour la sélection.
        # Pour respecter la préférence "plus petite capacité d'abord pour un même ratio", on trie ici.
        salles_triees_pour_iteration = df_salles_master.sort_values(by=[COLONNE_CAPACITE_SALLE, COLONNE_NOM_SALLE])

        for _, salle in salles_triees_pour_iteration.iterrows():
            if salle[COLONNE_CAPACITE_SALLE] >= nombre_inscrits:
                if is_room_free(room_schedules[salle[COLONNE_NOM_SALLE]], current_booking_start_dt, current_booking_end_dt):
                    room_ratio = salle[COLONNE_CAPACITE_SALLE] / nombre_inscrits
                    potential_rooms_for_this_booking.append({
                        'nom': salle[COLONNE_NOM_SALLE],
                        'capacite': salle[COLONNE_CAPACITE_SALLE],
                        'ratio': room_ratio
                    })
        
        if potential_rooms_for_this_booking:
            # Trier les salles candidates : ratio (asc), puis capacité (asc), puis nom (asc)
            potential_rooms_for_this_booking.sort(key=lambda x: (x['ratio'], x['capacite'], x['nom']))
            
            salle_choisie = potential_rooms_for_this_booking[0]
            salle_optimale_nom = salle_choisie['nom']
            capacite_salle_optimisee = salle_choisie['capacite']
            ratio_optimise = salle_choisie['ratio']
            sieges_excedentaires_optimises = capacite_salle_optimisee - nombre_inscrits
            
            # Mettre à jour le planning de la salle choisie
            room_schedules[salle_optimale_nom].append((current_booking_start_dt, current_booking_end_dt))

        # Calcul du gain
        if pd.notna(ratio_ancien) and pd.notna(ratio_optimise):
            score_fit_ancien = abs(ratio_ancien - 1)
            score_fit_optimise = abs(ratio_optimise - 1)
            amelioration_score_fit = score_fit_ancien - score_fit_optimise
        
        if pd.notna(sieges_excedentaires_anciens) and pd.notna(sieges_excedentaires_optimises):
            reduction_sieges_excedentaires = sieges_excedentaires_anciens - sieges_excedentaires_optimises

        resultat_ligne = reservation.to_dict()
        resultat_ligne.update({
            COLONNE_SALLE_OPTIMISEE: salle_optimale_nom,
            'CapaciteSalleOptimisee': capacite_salle_optimisee,
            'RatioOptimise': ratio_optimise,
            'SiegesExcedentairesOptimises': sieges_excedentaires_optimises,
            'AncienneSalle': nom_ancienne_salle_booking, # Peut être différent de reservation.get si COLONNE_ANCIENNE_SALLE_BOOKING n'est pas dans les colonnes originales
            'CapaciteAncienneSalle': capacite_ancienne_salle,
            'RatioAncien': ratio_ancien,
            'SiegesExcedentairesAnciens': sieges_excedentaires_anciens,
            'AmeliorationScoreFit': amelioration_score_fit,
            'ReductionSiegesExcedentaires': reduction_sieges_excedentaires
        })
        # S'assurer que 'AncienneSalle' prend la valeur de la colonne originale si elle existe et que son nom est COLONNE_ANCIENNE_SALLE_BOOKING
        if COLONNE_ANCIENNE_SALLE_BOOKING in reservation:
            resultat_ligne['AncienneSalle'] = reservation[COLONNE_ANCIENNE_SALLE_BOOKING]
        else: # Si la colonne n'existe pas du tout dans l'input pour cette ligne
            resultat_ligne['AncienneSalle'] = pd.NA


        resultats_optimises_list.append(resultat_ligne)

    df_resultats_valides_optimises = pd.DataFrame(resultats_optimises_list)
    
    # Recombiner avec les réservations invalides
    df_final_complet = pd.concat([df_resultats_valides_optimises, df_reservations_invalides], ignore_index=True)
    
    # S'assurer que toutes les colonnes attendues sont présentes, même si l'une des parties était vide
    colonnes_attendues_en_sortie = list(df_reservations_input.columns) + \
                                   [COLONNE_SALLE_OPTIMISEE, 'CapaciteSalleOptimisee', 'RatioOptimise', 
                                    'SiegesExcedentairesOptimises', 'AncienneSalle', 'CapaciteAncienneSalle', 
                                    'RatioAncien', 'SiegesExcedentairesAnciens', 'AmeliorationScoreFit', 
                                    'ReductionSiegesExcedentaires']
    # Supprimer les colonnes start_datetime et end_datetime qui étaient pour usage interne
    if COLONNE_START_DATETIME in df_final_complet.columns:
        df_final_complet = df_final_complet.drop(columns=[COLONNE_START_DATETIME])
    if COLONNE_END_DATETIME in df_final_complet.columns:
        df_final_complet = df_final_complet.drop(columns=[COLONNE_END_DATETIME])
        
    for col in colonnes_attendues_en_sortie:
        if col not in df_final_complet.columns:
            df_final_complet[col] = pd.NA
            
    # Réorganiser les colonnes pour avoir les nouvelles colonnes à la fin ou dans un ordre logique
    # Ceci est optionnel mais peut améliorer la lisibilité du fichier Excel.
    # Pour l'instant, on retourne avec les colonnes telles quelles.

    return df_final_complet

# --- La fonction main() reste la même que celle que vous avez testée avec succès ---
# --- pour l'affichage des stats et le bouton de téléchargement.                ---
# --- Assurez-vous que la partie statistiques dans main() gère bien les pd.NA    ---
# --- dans les colonnes de ratio/gain (les .mean() etc. les ignorent par défaut) ---

def main():
    st.set_page_config(layout="wide")
    st.title("Optimisation d'Assignation des Salles Universitaires (avec Planification Horaire)")

    st.markdown(f"""
    Cette application optimise l'assignation des salles en fonction de leur capacité, du nombre d'inscrits, 
    et des **horaires de réservation**. Une salle peut être réutilisée si les créneaux ne se chevauchent pas.
    Les réservations sont priorisées par **nombre d'inscrits décroissant**, puis par heure de début.
    Elle utilise le fichier `{NOM_FICHIER_SALLES}` (qui doit être dans le même dossier que l'application)
    pour les informations sur les salles. Le critère d'optimisation est de trouver un ratio 
    `CapaciteSalle / NombreInscrit` le plus proche possible de 1 (et ≥ 1).
    """)

    chemin_complet_salles = os.path.join(os.path.dirname(__file__), NOM_FICHIER_SALLES)
    df_salles = charger_donnees_salles(chemin_complet_salles)

    if df_salles is not None:
        st.sidebar.subheader("Aperçu des Salles Disponibles")
        st.sidebar.dataframe(df_salles[[COLONNE_NOM_SALLE, COLONNE_CAPACITE_SALLE]].head(), hide_index=True)
        st.sidebar.info(f"{len(df_salles)} salles chargées.")

        fichier_reservations = st.file_uploader("Chargez votre fichier Excel de réservations (.xlsx)", type="xlsx")

        if fichier_reservations is not None:
            try:
                df_reservations_original = pd.read_excel(fichier_reservations)
                st.subheader("Aperçu des réservations chargées (5 premières lignes)")
                st.dataframe(df_reservations_original.head())

                if st.button("🚀 Lancer l'Optimisation"):
                    with st.spinner("Optimisation en cours... (cela peut prendre du temps avec la nouvelle logique)"):
                        df_optimise = optimiser_reservations(df_reservations_original, df_salles) # Passer df_reservations_original
                    
                    if df_optimise is not None:
                        st.subheader("Réservations Optimisées")
                        colonnes_cles_preview = [COLONNE_NOMBRE_INSCRIT, COLONNE_DATE_BOOKING, COLONNE_HEURE_DEBUT_BOOKING, COLONNE_HEURE_FIN_BOOKING,
                                                 COLONNE_ANCIENNE_SALLE_BOOKING, 'RatioAncien', 
                                                 COLONNE_SALLE_OPTIMISEE, 'RatioOptimise', 
                                                 'AmeliorationScoreFit', 'ReductionSiegesExcedentaires']
                        colonnes_a_afficher_preview = [col for col in colonnes_cles_preview if col in df_optimise.columns]
                        st.dataframe(df_optimise[colonnes_a_afficher_preview])

                        st.subheader("Statistiques d'Optimisation et de Gain")
                        df_assignations_reussies = df_optimise[
                            (df_optimise[COLONNE_SALLE_OPTIMISEE] != "Aucune salle adaptée disponible") &
                            (df_optimise[COLONNE_SALLE_OPTIMISEE] != "Réservation invalide (date/heure/inscrits)") & # Nouvelle condition
                            (df_optimise['CapaciteSalleOptimisee'].notna())
                        ].copy()

                        if not df_assignations_reussies.empty:
                            # ... (Les métriques et st.metric restent les mêmes que dans la version précédente) ...
                            total_reservations_traitees = len(df_optimise) 
                            nb_assignations_reussies = len(df_assignations_reussies)
                            
                            if total_reservations_traitees > 0:
                                pourcentage_reussite = (nb_assignations_reussies / total_reservations_traitees) * 100
                                st.metric(label="Taux d'assignation à une salle", value=f"{pourcentage_reussite:.2f}% ({nb_assignations_reussies}/{total_reservations_traitees})")
                            
                            st.metric(label="Ratio moyen optimisé (Capacité/Inscrits)", 
                                      value=f"{df_assignations_reussies['RatioOptimise'].mean():.2f}" 
                                      if pd.notna(df_assignations_reussies['RatioOptimise'].mean()) else "N/A")
                            st.metric(label="Moy. sièges excédentaires optimisés", 
                                      value=f"{df_assignations_reussies['SiegesExcedentairesOptimises'].mean():.2f}"
                                      if pd.notna(df_assignations_reussies['SiegesExcedentairesOptimises'].mean()) else "N/A")
                            st.metric(label="Total sièges excédentaires optimisés", 
                                      value=f"{df_assignations_reussies['SiegesExcedentairesOptimises'].sum():.0f}"
                                      if pd.notna(df_assignations_reussies['SiegesExcedentairesOptimises'].sum()) else "N/A")

                            df_comparaison_possible = df_assignations_reussies[
                                df_assignations_reussies['CapaciteAncienneSalle'].notna() &
                                df_assignations_reussies['AmeliorationScoreFit'].notna()
                            ].copy()

                            if not df_comparaison_possible.empty:
                                st.markdown("---")
                                st.subheader("Gain par rapport aux anciennes affectations (pour les réservations comparables)")
                                nb_comparaisons = len(df_comparaison_possible)
                                st.info(f"{nb_comparaisons} réservations avaient une ancienne affectation valide pour comparaison.")

                                st.metric(label="Amélioration moyenne du score de 'fit' (proximité à 1)", 
                                          value=f"{df_comparaison_possible['AmeliorationScoreFit'].mean():.2f}"
                                          if pd.notna(df_comparaison_possible['AmeliorationScoreFit'].mean()) else "N/A")
                                st.metric(label="Réduction totale des sièges excédentaires", 
                                          value=f"{df_comparaison_possible['ReductionSiegesExcedentaires'].sum():.0f}"
                                          if pd.notna(df_comparaison_possible['ReductionSiegesExcedentaires'].sum()) else "N/A")
                                st.metric(label="Réduction moyenne des sièges excédentaires par réservation comparable", 
                                          value=f"{df_comparaison_possible['ReductionSiegesExcedentaires'].mean():.2f}"
                                          if pd.notna(df_comparaison_possible['ReductionSiegesExcedentaires'].mean()) else "N/A")
                            else:
                                st.info("Aucune réservation avec ancienne affectation valide pour calculer le gain comparatif.")
                        else:
                            st.info("Aucune assignation réussie pour calculer les statistiques.")
                        
                        output = BytesIO()
                        with pd.ExcelWriter(output, engine='openpyxl') as writer:
                            df_optimise.to_excel(writer, index=False, sheet_name='ReservationsOptimisees')
                        excel_data = output.getvalue()
                        st.download_button(
                            label="📥 Télécharger le fichier Excel optimisé",
                            data=excel_data,
                            file_name="reservations_optimisees_planifiees.xlsx", # Nom de fichier mis à jour
                            mime="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        )
                    else:
                        st.error("L'optimisation n'a pas pu être complétée (df_optimise est None).")
            except Exception as e:
                st.error(f"Erreur majeure lors du traitement du fichier de réservations ou de l'optimisation : {e}")
                st.exception(e) # Affiche la trace de la pile pour le débogage
    else:
        st.warning(f"L'application ne peut pas fonctionner sans le fichier '{NOM_FICHIER_SALLES}'.")

if __name__ == "__main__":
    main()