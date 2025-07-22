import sys
import pandas as pd
from app_optimisation_salles import (
    optimiser_glouton,
    optimiser_genetique,
    charger_salles,
    exporter_excel
)

def run_optimizer(path_resa, path_salles, algo="glouton", seuil_bon=0.85, seuil_bas=0.3, buffer=0):
    df_salles = charger_salles(path_salles)
    df_resa_initial = pd.read_excel(path_resa)

    # 1. Gérer les réservations NombreInscrit = 0
    df_resa_to_optimize = df_resa_initial[df_resa_initial['NombreInscrit'] > 0].copy()
    df_resa_zero_inscrit = df_resa_initial[df_resa_initial['NombreInscrit'] == 0].copy()
    df_resa_zero_inscrit['Nom Salle'] = None
    df_resa_zero_inscrit['Statut Occupation'] = 'Non Attribuée (Inscrits=0)'

    df_result_first_pass = None
    if algo == "glouton":
        # Pour l'algorithme glouton, il est essentiel que optimiser_glouton
        # puisse renvoyer les réservations attribuées ET l'état des salles mises à jour.
        # Idéalement, la fonction devrait retourner un tuple: (df_resultats, salles_disponibles_apres_pass)
        df_result_first_pass = optimiser_glouton(df_resa_to_optimize.copy(), df_salles.copy(), seuil_bon, seuil_bas, buffer)
        
    elif algo == "genetique":
        # Pour le génétique, l'idéal est de modifier la fitness interne
        df_result_first_pass = optimiser_genetique(df_resa_to_optimize.copy(), df_salles.copy(), seuil_bon, seuil_bas)
    else:
        raise ValueError("❌ Algo inconnu. Utilise 'glouton' ou 'genetique'.")

    # Assurez-vous que 'Nom Salle' est bien la colonne qui indique l'attribution
    # et qu'elle contient des NaN ou None pour les non-attribuées
    df_unassigned = df_result_first_pass[df_result_first_pass['Nom Salle'].isnull()].copy()
    df_assigned_first_pass = df_result_first_pass.dropna(subset=['Nom Salle']).copy()

    # Si des réservations sont restées non attribuées et l'algo est glouton, tenter une seconde passe
    if algo == "glouton" and not df_unassigned.empty:
        print("Tentative de seconde passe pour les réservations non attribuées avec l'algorithme glouton...")
        
        # --- Implémentation de la seconde passe pour l'algorithme glouton ---
        # Cette partie nécessite que `optimiser_glouton` soit capable de :
        # 1. Recevoir l'état actuel des salles (celles déjà attribuées et leurs créneaux pris).
        # 2. Recevoir les réservations à prioriser (df_unassigned).
        # 3. Potentiellement utiliser des seuils assouplis pour cette passe.

        # Modification conceptuelle nécessaire dans optimiser_glouton :
        # La fonction devrait accepter un paramètre pour les salles "déjà utilisées"
        # ou un mécanisme interne pour gérer la disponibilité après la 1ère passe.
        # Et un paramètre pour des seuils d'occupation moins stricts ou une logique de "force d'attribution".

        # Exemple d'appel pour une seconde passe (nécessite modif. de optimiser_glouton)
        # Supposons que optimiser_glouton renvoie aussi les salles mises à jour
        # Par exemple: df_assigned_first_pass, df_updated_salles_after_pass1 = optimiser_glouton(...)
        
        # Pour cette seconde passe, nous pouvons assouplir les seuils:
        # seuil_bon_relaxed = 0.0 # Accepte n'importe quelle occupation positive
        # seuil_bas_relaxed = 0.0 # Idem
        
        # df_result_second_pass = optimiser_glouton(
        #     df_unassigned,                      # Uniquement les réservations non attribuées
        #     df_updated_salles_after_pass1,      # L'état des salles après la 1ère passe
        #     seuil_bon_relaxed, seuil_bas_relaxed, buffer,
        #     prioritize_unassigned=True          # Un flag pour indiquer à l'algo de forcer l'attribution
        # )

        # Pour le moment, sans modification de optimiser_glouton, nous devons simuler
        # ou gérer les résultats de manière simplifiée.
        # Si optimiser_glouton ne peut pas encore faire la 2e passe,
        # df_unassigned_after_second_pass restera les mêmes.
        df_result_second_pass = optimiser_glouton(df_unassigned.copy(), df_salles.copy(), 0.0, 0.0, buffer) # Essai avec seuils très bas
        df_newly_assigned = df_result_second_pass.dropna(subset=['Nom Salle']).copy()
        df_still_unassigned = df_result_second_pass[df_result_second_pass['Nom Salle'].isnull()].copy()

        # Recombine les résultats: ceux assignés à la 1ère passe, ceux assignés à la 2ème, et ceux toujours non attribués
        final_df_result = pd.concat([df_assigned_first_pass, df_newly_assigned, df_still_unassigned, df_resa_zero_inscrit]).reset_index(drop=True)

    else:
        # Si pas de 2ème passe (soit pas glouton, soit pas de non-attribuées), on combine simplement
        final_df_result = pd.concat([df_result_first_pass, df_resa_zero_inscrit]).reset_index(drop=True)

    # Assurez-vous que les réservations non attribuées (avec Nom Salle = NaN)
    # sont bien identifiées si elles n'ont pas été prises en charge par une 2ème passe.

    if final_df_result is not None:
        buffer_excel = exporter_excel(final_df_result, seuil_bon, seuil_bas)
        with open("resultat.xlsx", "wb") as f:
            f.write(buffer_excel.getvalue())
        print("✅ Optimisation terminée. Fichier 'resultat.xlsx' généré.")
    else:
        print("❌ Optimisation échouée.")

# -------- Lancement depuis la ligne de commande --------
if __name__ == "__main__":
    if len(sys.argv) < 4:
        print("❌ Utilisation : python core_optimizer.py <resa.xlsx> <salles.xlsx> <glouton|genetique> [seuil_bon] [seuil_bas] [buffer]")
        sys.exit(1)

    path_resa = sys.argv[1]
    path_salles = sys.argv[2]
    algo = sys.argv[3]
    seuil_bon = float(sys.argv[4]) if len(sys.argv) > 4 else 0.85
    seuil_bas = float(sys.argv[5]) if len(sys.argv) > 5 else 0.3
    buffer = int(sys.argv[6]) if len(sys.argv) > 6 else 0

    run_optimizer(path_resa, path_salles, algo, seuil_bon, seuil_bas, buffer)